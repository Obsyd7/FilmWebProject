﻿using AutoMapper;
using Data.Infrastructure;
using Model.Models;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using Web.Helpers;
using Web.ViewModels;

namespace Web.Controllers
{
    public class FilmsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly GenreHelper _genreHelper;

        public FilmsController(IUnitOfWork unitOfWork, GenreHelper genreHelper)
        {
            _unitOfWork = unitOfWork;
            _genreHelper = genreHelper;
        }

        [Authorize]
        public ActionResult Create()
        {
            var genresFromDb = _unitOfWork.Genres.GetAllGenres();
            var genresViewModel = Mapper.Map<List<Genre>, List<GenreViewModel>>(genresFromDb);
            var viewmodel = new FilmFormViewModel { Genres = genresViewModel };

            return View("FilmForm", viewmodel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Create(FilmFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var genresFromDb = _unitOfWork.Genres.GetAllGenres();
                var genresViewModel = Mapper.Map<List<Genre>, List<GenreViewModel>>(genresFromDb);

                viewModel.Genres = genresViewModel;

                return View("FilmForm", viewModel);
            }

            var newFilm = Mapper.Map<Film>(viewModel);

            var listOfSelectedGenres = _genreHelper.GetSelectedGenres(viewModel);

            newFilm.Genres = listOfSelectedGenres;

            _unitOfWork.Films.Add(newFilm);
            _unitOfWork.Complete();

            return RedirectToAction("Details", "Films", new { id = newFilm.Id });
        }

        public ActionResult List(string query = null)
        {
            var films = _unitOfWork.Films.GetAllFilms();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var lower = query.ToLower();

                films = films
                    .Where(g =>
                        g.Title.ToLower().Contains(lower)).ToList();
            }

            var filmViewModel = new FilmListViewModel { ListOfFilms = films };

            return View(filmViewModel);
        }


        public ActionResult Details(int id)
        {
            var film = _unitOfWork.Films.GetOneFilm(id);

            if (film == null)
                return HttpNotFound();

            return View(film);
        }

        [Authorize]
        public ActionResult Edit(int id)
        {
            if (id <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var film = _unitOfWork.Films.GetOneFilm(id);

            if (film == null)
                return HttpNotFound();

            //further refactoring required

            var allGenresFromDb = _unitOfWork.Genres.GetAllGenres();

            var genresViewModel = Mapper.Map<List<Genre>, List<GenreViewModel>>(allGenresFromDb);

            foreach (var genreForViewModel in genresViewModel)
                foreach (var currentFilmGenre in film.Genres)
                    if (currentFilmGenre.Id == genreForViewModel.Id)
                        genreForViewModel.IsChecked = true;

            var filmFormViewModel = Mapper.Map<FilmFormViewModel>(film);
            filmFormViewModel.Genres = genresViewModel;

            // further refactoring required

            return View("FilmForm", filmFormViewModel);
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Update(FilmFormViewModel viewModel)
        {
            if (!ModelState.IsValid)
            {
                var genresFromDb = _unitOfWork.Genres.GetAllGenres();
                var genresViewModel = Mapper.Map<List<Genre>, List<GenreViewModel>>(genresFromDb);

                viewModel.Genres = genresViewModel;

                return View("FilmForm", viewModel);
            }

            var filmDb = _unitOfWork.Films.GetOneFilm(viewModel.Id);

            if (filmDb == null)
                return HttpNotFound();

            filmDb.Genres.Clear();

            var genres = _genreHelper.GetSelectedGenres(viewModel);

            var filmUpdate = Mapper.Map<Film>(viewModel);
            filmUpdate.Genres = genres;

            filmDb = filmUpdate;
            _unitOfWork.Complete();

            return RedirectToAction("Details", new { id = filmDb.Id });
        }

        [HttpPost]
        public ActionResult Search(FilmListViewModel viewModel)
        {
            return RedirectToAction("List", "Films", new { query = viewModel.Search });
        }
    }
}