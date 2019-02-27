﻿using AutoMapper;
using FilmWebProject.Core.Models;
using FilmWebProject.Core.Repositories;
using FilmWebProject.Core.ViewModels;
using FilmWebProject.Persistence;
using System.Collections.Generic;
using System.Net;
using System.Web.Mvc;

namespace FilmWebProject.Controllers
{
    public class FilmsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly GenreRepository _genreRepository;
        private readonly FilmRepository _filmRepository;


        public FilmsController()
        {
            _context = new ApplicationDbContext();
            _genreRepository = new GenreRepository(_context);
            _filmRepository = new FilmRepository(_context);
        }

        [Authorize]
        public ActionResult Create()
        {
            var genresFromDb = _genreRepository.GetAllGenres();
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
                var genresFromDb = _genreRepository.GetAllGenres();
                var genresViewModel = Mapper.Map<List<Genre>, List<GenreViewModel>>(genresFromDb);

                viewModel.Genres = genresViewModel;

                return View("FilmForm", viewModel);
            }

            var newFilm = Mapper.Map<Film>(viewModel);
            newFilm.Genres = _genreRepository.GetSelectedGenres(viewModel);

            _context.Films.Add(newFilm);
            _context.SaveChanges();

            return RedirectToAction("Index", "Home");
        }

        public ActionResult List()
        {
            var films = _filmRepository.GetAllFilms();
            return View(films);
        }


        public ActionResult Details(int id)
        {
            var film = _filmRepository.GetOneFilm(id);

            if (film == null)
                return HttpNotFound();

            return View(film);
        }

        [Authorize]
        public ActionResult Edit(int id)
        {
            if (id <= 0)
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            var film = _filmRepository.GetOneFilm(id);

            if (film == null)
                return HttpNotFound();

            // further refactoring required

            var genresFromDb = _genreRepository.GetAllGenres();

            var genresEditViewModel = Mapper.Map<List<Genre>, List<GenreViewModel>>(genresFromDb);

            foreach (var genreEditViewModel in genresEditViewModel)
                foreach (var genreDetailsViewModel in film.Genres)
                    if (genreDetailsViewModel.Id == genreEditViewModel.Id)
                        genreEditViewModel.IsChecked = true;

            var filmFormViewModel = Mapper.Map<FilmFormViewModel>(film);
            filmFormViewModel.Genres = genresEditViewModel;

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
                var genresFromDb = _genreRepository.GetAllGenres();
                var genresViewModel = Mapper.Map<List<Genre>, List<GenreViewModel>>(genresFromDb);

                viewModel.Genres = genresViewModel;

                return View("FilmForm", viewModel);
            }

            var filmFromDb = _filmRepository.GetOneFilm(viewModel.Id);

            if (filmFromDb == null)
                return HttpNotFound();

            filmFromDb.Genres.Clear();

            var genres = _genreRepository.GetSelectedGenres(viewModel);

            filmFromDb.Update(genres, viewModel);
            _context.SaveChanges();

            return RedirectToAction("Details", new { id = filmFromDb.Id });
        }
    }
}