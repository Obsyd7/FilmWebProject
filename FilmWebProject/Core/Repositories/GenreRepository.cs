﻿using FilmWebProject.Persistence;

namespace FilmWebProject.Core.Repositories
{
    public class GenreRepository
    {
        private readonly ApplicationDbContext _context;

        public GenreRepository(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}