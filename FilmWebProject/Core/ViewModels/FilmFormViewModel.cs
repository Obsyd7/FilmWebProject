﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FilmWebProject.Core.ViewModels
{
    public class FilmFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Title { get; set; }

        [Required]
        public string Duration { get; set; }

        [Required]
        [Display(Name = "Release Date")]
        public string ReleaseDate { get; set; }

        [Required]
        public string BoxOffice { get; set; }

        [Required]
        public string Budget { get; set; }

        public string Production { get; set; }
        public string Studio { get; set; }

        public List<GenreViewModel> Genre { get; set; }

        public string Action => (Id != 0) ? "Update" : "Create";
    }

}