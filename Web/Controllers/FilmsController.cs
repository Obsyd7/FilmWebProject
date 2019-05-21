﻿using AutoMapper;
using Castle.Core.Internal;
using Model.Models;
using Service;
using Service.Dtos;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;
using Web.Helpers;
using Web.ViewModels;

namespace Web.Controllers
{
    public class FilmsController : Controller
    {
        private readonly IGenreHelper _genreHelper;
        private readonly IFilmService _filmService;

        public FilmsController(IFilmService filmService, IGenreHelper genreHelper)
        {
            _filmService = filmService;
            _genreHelper = genreHelper;
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Create()
        {
            var genresFromDb = _filmService.GetAllGenres();
            var genresViewModel = Mapper.Map<List<Genre>, List<GenreViewModel>>(genresFromDb);
            var viewmodel = new FilmFormViewModel { Genres = genresViewModel };

            return View("FilmForm", viewmodel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FilmFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var genresFromDb = _filmService.GetAllGenres();
                var genresViewModel = Mapper.Map<List<Genre>, List<GenreViewModel>>(genresFromDb);

                viewModel.Genres = genresViewModel;

                return View("FilmForm", viewModel);
            }

            var newFilm = Mapper.Map<Film>(viewModel);
            var listOfSelectedGenres = _genreHelper.GetSelectedGenres(viewModel);
            newFilm.Genres = listOfSelectedGenres;

            _filmService.AddNewFilm(newFilm);
            _filmService.Complete();

            return RedirectToAction("Details", "Films", new { id = newFilm.Id });
        }

        [HttpGet]
        public ActionResult List(FilmListParametersViewModel filmListParametersViewModel)
        {
            var filmListParametersDto = Mapper.Map<FilmListParameters>(filmListParametersViewModel);

            var films = !string.IsNullOrWhiteSpace(filmListParametersViewModel.QuerySearch)
                ? _filmService.GetFilmsBySearchQuery(filmListParametersDto)
                : _filmService.GetFilmsWithPagination(filmListParametersDto);

            if (!filmListParametersViewModel.Genres.IsNullOrEmpty() || !filmListParametersViewModel.Countries.IsNullOrEmpty())
                films = _filmService.FilterFilms(filmListParametersViewModel.Genres, filmListParametersViewModel.Countries, films);

            var filmViewModel = new FilmListViewModel
            {
                ListOfFilms = films,
                Genres = Mapper.Map<List<Genre>, List<GenreViewModel>>(_filmService.GetAllGenres()),
                FilmListParameters =
                {
                    MaxPageNumber = _filmService.GetAllFilmCount() / filmListParametersViewModel.PageSize,
                    CurrentPage = filmListParametersViewModel.CurrentPage,
                    //Genres = _filmService.GetGenresNamesForFiltering()
                }
            };

            return View(filmViewModel);
        }

        [HttpGet]
        public ActionResult Details(int id)
        {
            if (id <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var film = _filmService.GetFilmById(id);

            if (film == null)
                return HttpNotFound();

            return View(film);
        }


        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int id)
        {
            if (id <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var film = _filmService.GetFilmById(id);

            if (film == null)
                return HttpNotFound();

            var currentFilmGenres = film.Genres;
            var allGenresFromDb = _filmService.GetAllGenres();
            var genresViewModel = Mapper.Map<List<Genre>, List<GenreViewModel>>(allGenresFromDb);

            var filmFormViewModel = Mapper.Map<FilmFormViewModel>(film);
            filmFormViewModel.Genres = _genreHelper.GetAllAndSelectedGenres(genresViewModel, currentFilmGenres);

            return View("FilmForm", filmFormViewModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FilmFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var genresFromDb = _filmService.GetAllGenres();
                var genresViewModel = Mapper.Map<List<Genre>, List<GenreViewModel>>(genresFromDb);

                viewModel.Genres = genresViewModel;

                return View("FilmForm", viewModel);
            }

            var filmDb = _filmService.GetFilmById(viewModel.Id);

            if (filmDb == null)
                return HttpNotFound();

            filmDb.Genres.Clear();

            var genres = _genreHelper.GetSelectedGenres(viewModel);

            var filmUpdate = Mapper.Map<Film>(viewModel);
            filmUpdate.Genres = genres;

            filmDb = filmUpdate;
            _filmService.Complete();

            return RedirectToAction("Details", new { id = filmDb.Id });
        }

        [HttpPost]
        public ActionResult Search(FilmListParametersViewModel filmListParameters)
        {
            return RedirectToAction("List", "Films", filmListParameters);
        }
    }
}