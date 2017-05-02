using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using PopcornApi.Database;
using PopcornApi.Services.Caching;
using PopcornApi.Services.Logging;
using PopcornApi.Models.User;

namespace PopcornApi.Controllers
{
    [Route("api/[controller]")]
    public class UserController : Controller
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
        public UserController(ILoggingService loggingService, ICachingService cachingService)
        {
            _loggingService = loggingService;
            _cachingService = cachingService;
        }

        // GET api/user/985e70d3-6bdc-4c14-b1d5-de59b1a4b6c5
        [HttpGet("{userId}")]
        public async Task<IActionResult> Get(string userId)
        {
            var guid = Guid.Empty;
            if (Guid.TryParse(userId, out guid))
            {
                using (var context = new PopcornContextFactory().Create(new DbContextFactoryOptions()))
                {
                    if (await context.UserSet.Include(a => a.ShowHistory)
                        .Include(a => a.MovieHistory)
                        .Include(a => a.Language)
                        .AllAsync(a => a.MachineGuid != guid))
                    {
                        context.UserSet.Add(new User
                        {
                            MachineGuid = guid,
                            Language = new Language
                            {
                                Culture = "en"
                            },
                            MovieHistory = new List<MovieHistory>(),
                            ShowHistory = new List<ShowHistory>(),
                            DownloadLimit = 0,
                            UploadLimit = 0
                        });

                        await context.SaveChangesAsync();
                        var response = new UserJson
                        {
                            MachineGuid = guid,
                            Language = new LanguageJson
                            {
                                Culture = "en"
                            },
                            MovieHistory = new List<MovieHistoryJson>(),
                            ShowHistory = new List<ShowHistoryJson>(),
                            DownloadLimit = 0,
                            UploadLimit = 0
                        };

                        return Ok(response);
                    }

                    var user = await context.UserSet.Include(a => a.ShowHistory)
                        .Include(a => a.MovieHistory)
                        .Include(a => a.Language)
                        .FirstOrDefaultAsync(a => a.MachineGuid == guid);
                    return Ok(ConvertUserToJson(user));
                }
            }

            return BadRequest();
        }

        // Put api/user
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]UserJson userJson)
        {
            using (var context = new PopcornContextFactory().Create(new DbContextFactoryOptions()))
            {
                if (await context.UserSet.AllAsync(a => a.MachineGuid != userJson.MachineGuid))
                {
                    return BadRequest();
                }

                var user = await context.UserSet.Include(a => a.ShowHistory)
                    .Include(a => a.MovieHistory)
                    .Include(a => a.Language)
                    .FirstOrDefaultAsync(a => a.MachineGuid == userJson.MachineGuid);
                user.DownloadLimit = userJson.DownloadLimit;
                user.UploadLimit = userJson.UploadLimit;
                user.Language.Culture = userJson.Language.Culture;
                foreach (var showHistory in user.ShowHistory.Where(
                    a => userJson.ShowHistory.Any(b => b.ImdbId == a.ImdbId)))
                {
                    var updatedShowHistory =
                        userJson.ShowHistory.FirstOrDefault(a => a.ImdbId == showHistory.ImdbId);
                    showHistory.Favorite = updatedShowHistory.Favorite;
                    showHistory.Seen = updatedShowHistory.Seen;
                }

                foreach (var newShowHistory in userJson.ShowHistory.Where(
                    a => user.ShowHistory.All(b => b.ImdbId != a.ImdbId)))
                {
                    user.ShowHistory.Add(new ShowHistory
                    {
                        ImdbId = newShowHistory.ImdbId,
                        Favorite = newShowHistory.Favorite,
                        Seen = newShowHistory.Seen
                    });
                }

                foreach (var movieHistory in user.MovieHistory.Where(
                    a => userJson.MovieHistory.Any(b => b.ImdbId == a.ImdbId)))
                {
                    var updatedMovieHistory =
                        userJson.MovieHistory.FirstOrDefault(a => a.ImdbId == movieHistory.ImdbId);
                    movieHistory.Favorite = updatedMovieHistory.Favorite;
                    movieHistory.Seen = updatedMovieHistory.Seen;
                }

                foreach (var newMovieHistory in userJson.MovieHistory.Where(
                    a => user.MovieHistory.All(b => b.ImdbId != a.ImdbId)))
                {
                    user.MovieHistory.Add(new MovieHistory
                    {
                        ImdbId = newMovieHistory.ImdbId,
                        Favorite = newMovieHistory.Favorite,
                        Seen = newMovieHistory.Seen
                    });
                }

                await context.SaveChangesAsync();
                return Ok(ConvertUserToJson(user));
            }
        }

        /// <summary>
        /// Convert a user from database to json
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private UserJson ConvertUserToJson(User user)
        {
            return new UserJson
            {
                MachineGuid = user.MachineGuid,
                DownloadLimit = user.DownloadLimit,
                UploadLimit = user.UploadLimit,
                Language = new LanguageJson
                {
                    Culture = user.Language.Culture
                },
                ShowHistory = user.ShowHistory.Select(a => new ShowHistoryJson
                {
                    Favorite = a.Favorite,
                    ImdbId = a.ImdbId,
                    Seen = a.Seen
                }).ToList(),
                MovieHistory = user.ShowHistory.Select(a => new MovieHistoryJson
                {
                    Favorite = a.Favorite,
                    ImdbId = a.ImdbId,
                    Seen = a.Seen
                }).ToList()
            };
        }
    }
}