using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PopcornApi.Services.Caching;
using PopcornApi.Services.Logging;
using System.Collections.Generic;
using PopcornApi.Models.Trailer;
using System;
using VideoLibrary;

namespace PopcornApi.Controllers
{
    [Route("api/[controller]")]
    public class TrailerController : Controller
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
        public TrailerController(ILoggingService loggingService, ICachingService cachingService)
        {
            _loggingService = loggingService;
            _cachingService = cachingService;
        }

        // GET api/trailer/q4Zd-LxoK1c
        [HttpGet("{ytTrailerCode}")]
        public async Task<IActionResult> Get(string ytTrailerCode)
        {
            var cachedTrailer = _cachingService.GetCache(ytTrailerCode);
            if (cachedTrailer == null)
            {
                using (var service = Client.For(YouTube.Default))
                {
                    var videos = await service.GetAllVideosAsync("https://youtube.com/watch?v=" + ytTrailerCode);
                    if (videos != null && videos.Any())
                    {
                        var trailer = videos.Where(a => a.Format == VideoFormat.Mp4 && !a.Is3D)
                            .Aggregate((trailer1, trailer2) => trailer1.Resolution > trailer2.Resolution
                                ? trailer1
                                : trailer2);
                        var response = new TrailerResponse {TrailerUrl = await trailer.GetUriAsync()};
                        _cachingService.SetCache(ytTrailerCode, JsonConvert.SerializeObject(response),
                            TimeSpan.FromDays(180));
                        return Json(response);
                    }

                    return BadRequest();
                }
            }

            return Json(JsonConvert.DeserializeObject<TrailerResponse>(cachedTrailer));
        }
    }
}