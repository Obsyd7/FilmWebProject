﻿using FilmWebProject.Core.ViewModels;
using FilmWebProject.Persistance;
using System.Web.Mvc;

namespace FilmWebProject.Controllers
{
    public class CinematographersController : Controller
    {
        private ApplicationDbContext _context;

        public CinematographersController()
        {
            _context = new ApplicationDbContext();
        }

        [Authorize]
        public ActionResult Create()
        {
            ViewBag.ControllerName = "Photographers";

            var viewModel = new CreatePersonFormViewModel();

            return View("CreatePersonView", viewModel);
        }
    }
}