using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PopcornApi.Database;
using PopcornApi.Models.Language;
using PopcornApi.Services.Caching;
using PopcornApi.Services.Logging;
using Utf8Json;
using Utf8Json.Resolvers;

namespace PopcornApi.Controllers
{
    [Route("api/[controller]")]
    public class LanguageController : Controller
    {
        /// <summary>
        /// The caching service
        /// </summary>
        private readonly ICachingService _cachingService;

        /// <summary>
        /// The logging service
        /// </summary>
        private readonly ILoggingService _loggingService;

        public LanguageController(ICachingService cachingService, ILoggingService loggingService)
        {
            _cachingService = cachingService;
            _loggingService = loggingService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var hash = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    $@"type=language"));
            try
            {
                var cachedLanguages = await _cachingService.GetCache(hash);
                if (cachedLanguages != null)
                {
                    try
                    {
                        return Content(cachedLanguages, "application/json");
                    }
                    catch (Exception ex)
                    {
                        _loggingService.Telemetry.TrackException(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                _loggingService.Telemetry.TrackException(ex);
            }

            using (var context = new PopcornContextFactory().CreateDbContext(new string[0]))
            {
                var languages = context.LanguageSet;
                if (languages == null) return BadRequest();

                var languagesJson = languages.Select(language => new LanguageJson
                {
                    LanguageName = language.LanguageName
                }).ToList();
                var json = JsonSerializer.ToJsonString(languagesJson, StandardResolver.SnakeCase);
                await _cachingService.SetCache(hash, json, TimeSpan.FromDays(1));
                return Content(json, "application/json");
            }
        }
    }
}
