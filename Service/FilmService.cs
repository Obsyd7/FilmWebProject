﻿using Data.Infrastructure;
using Model.Models;
using Service.Dtos;
using Service.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Service
{
    public class FilmService : ServiceBase, IFilmService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly FilmServiceHelper _filmServiceHelper;
        private readonly FilmFilter _filmFiler;

        public FilmService(IUnitOfWork unitOfWork, FilmServiceHelper filmServiceHelper, FilmFilter filmFiler) : base(unitOfWork)
        {
            _unitOfWork = unitOfWork;
            _filmServiceHelper = filmServiceHelper;
            _filmFiler = filmFiler;
        }

        public List<Genre> GetAllGenres()
        {
            return _unitOfWork.Genres.GetAll();
        }

        public void AddNewFilm(Film film)
        {
            _unitOfWork.Films.Add(film);
        }

        public Film GetFilmById(int id)
        {
            return _unitOfWork.Films.GetById(id);
        }

        public void RemoveFilm(Film film)
        {
            _unitOfWork.Films.Delete(film);
        }

        public ApplicationUser GetUserById(string id)
        {
            return _unitOfWork.Users.GetUserById(id);
        }

        public void AddNewRating(Film film, ApplicationUser user, int value)
        {
            var newRating = new Rating { Film = film, User = user, Value = value };
            _unitOfWork.Ratings.Add(newRating);
        }

        public Rating GetUserRating(int filmId, string userId)
        {
            return _unitOfWork.Ratings.GetUserRating(filmId, userId);
        }

        public List<Film> GetFilms(FilmListParametersDto filmListParametersDto)
        {
            var pageSize = filmListParametersDto.PageSize;
            var pageNumber = filmListParametersDto.PageNumber - 1;

            if (_filmServiceHelper.IsSearched(filmListParametersDto))
                return _unitOfWork.Films.GetFilmsWithPagination(pageSize, pageNumber);

            var filmsFromDb = _unitOfWork.Films.GetAll();

            var result = _filmFiler.Filter(filmsFromDb, filmListParametersDto);

            return result.Skip(pageSize * pageNumber).Take(pageSize).ToList();
        }

        public int GetMaxPageNumber(int pageSize)
        {
            return _unitOfWork.Films.GetAllFilmCount() / pageSize;
        }


        //Try refactoring
        public List<GenreDto> GetAllAndSelectedGenres(ICollection<Genre> currentFilmGenres, List<GenreDto> genresDto)
        {
            foreach (var genreDto in genresDto)
                foreach (var currentFilmGenre in currentFilmGenres)
                    if (currentFilmGenre.Id == genreDto.Id)
                        genreDto.IsChecked = true;

            return genresDto;
        }

        public List<Genre> GetSelectedGenres(IEnumerable<GenreDto> filmDtoGenres)
        {
            return (from genreDto in filmDtoGenres where genreDto.IsChecked select _unitOfWork.Genres.GetById(genreDto.Id)).ToList();
        }
    }
}
