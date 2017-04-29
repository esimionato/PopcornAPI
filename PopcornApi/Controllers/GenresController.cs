using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PopcornApi.Attributes;
using PopcornApi.Services.Caching;
using PopcornApi.Services.Logging;
using System;
using System.Collections.Generic;
using PopcornApi.Helpers;
using PopcornApi.Models.Genres;

namespace PopcornApi.Controllers
{
    [Route("api/[controller]")]
    public class GenresController : Controller
    {
        /// <summary>
        /// The logging service
        /// </summary>
        private readonly ILoggingService _loggingService;

        /// <summary>
        /// The caching service
        /// </summary>
        private readonly ICachingService _cachingService;

        /// <summary>
        /// Movies
        /// </summary>
        /// <param name="loggingService">The logging service</param>
        /// <param name="cachingService">The caching service</param>
        public GenresController(ILoggingService loggingService, ICachingService cachingService)
        {
            _loggingService = loggingService;
            _cachingService = cachingService;
        }

        // GET api/genres
        [HttpGet]
        public async Task<IActionResult> Get([RequiredFromQuery] string language)
        {
            return
                Json(new GenreResponse
                {
                    Genres = new List<Genre>
                    {
                        new Genre
                        {
                            EnglishName = "Action",
                            Name = language == "fr" ? "Action" : "Action"
                        },
                        new Genre
                        {
                            EnglishName = "Adventure",
                            Name = language == "fr" ? "Aventure" : "Adventure"
                        },
                        new Genre
                        {
                            EnglishName = "Animation",
                            Name = language == "fr" ? "Animation" : "Animation"
                        },
                        new Genre
                        {
                            EnglishName = "Comedy",
                            Name = language == "fr" ? "Comédie" : "Comedy"
                        },
                        new Genre
                        {
                            EnglishName = "Crime",
                            Name = language == "fr" ? "Crime" : "Crime"
                        },
                        new Genre
                        {
                            EnglishName = "Documentary",
                            Name = language == "fr" ? "Documentaire" : "Documentary"
                        },
                        new Genre
                        {
                            EnglishName = "Drama",
                            Name = language == "fr" ? "Drame" : "Drama"
                        },
                        new Genre
                        {
                            EnglishName = "Family",
                            Name = language == "fr" ? "Familial" : "Family"
                        },
                        new Genre
                        {
                            EnglishName = "Fantasy",
                            Name = language == "fr" ? "Fantastique" : "Fantasy"
                        },
                        new Genre
                        {
                            EnglishName = "History",
                            Name = language == "fr" ? "Histoire" : "History"
                        },
                        new Genre
                        {
                            EnglishName = "Horror",
                            Name = language == "fr" ? "Horreur" : "Horror"
                        },
                        new Genre
                        {
                            EnglishName = "Music",
                            Name = language == "fr" ? "Musique" : "Music"
                        },
                        new Genre
                        {
                            EnglishName = "Mystery",
                            Name = language == "fr" ? "Mystère" : "Mystery"
                        },
                        new Genre
                        {
                            EnglishName = "Romance",
                            Name = language == "fr" ? "Romance" : "Romance"
                        },
                        new Genre
                        {
                            EnglishName = "Science Fiction",
                            Name = language == "fr" ? "Science-Fiction" : "Science Fiction"
                        },
                        new Genre
                        {
                            EnglishName = "TV Movie",
                            Name = language == "fr" ? "Téléfilm" : "TV Movie"
                        },
                        new Genre
                        {
                            EnglishName = "Thriller",
                            Name = language == "fr" ? "Thriller" : "Thriller"
                        },
                        new Genre
                        {
                            EnglishName = "War",
                            Name = language == "fr" ? "Guerre" : "War"
                        },
                        new Genre
                        {
                            EnglishName = "Western",
                            Name = language == "fr" ? "Western" : "Western"
                        },
                    }
                });
        }
    }
}