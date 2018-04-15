using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PopcornApi.Attributes;
using PopcornApi.Database;
using PopcornApi.Models.Episode;
using PopcornApi.Models.Image;
using PopcornApi.Models.Rating;
using PopcornApi.Models.Show;
using PopcornApi.Models.Torrent.Show;
using PopcornApi.Services.Caching;
using PopcornApi.Services.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using PopcornApi.Extensions;
using PopcornApi.Helpers;
using Utf8Json.Resolvers;
using JsonSerializer = Utf8Json.JsonSerializer;

namespace PopcornApi.Controllers
{
    [Route("api/[controller]")]
    public class ShowsController : Controller
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
        public ShowsController(ILoggingService loggingService, ICachingService cachingService)
        {
            _loggingService = loggingService;
            _cachingService = cachingService;
        }

        // GET api/shows
        [HttpGet]
        public async Task<IActionResult> Get([RequiredFromQuery] int page, [FromQuery] int limit,
            [FromQuery] int minimum_rating, [FromQuery] string query_term,
            [FromQuery] string genre, [FromQuery] string sort_by)
        {
            var nbShowsPerPage = 20;
            if (limit >= 20 && limit <= 50)
                nbShowsPerPage = limit;

            var currentPage = 1;
            if (page >= 1)
            {
                currentPage = page;
            }

            var queryTerm = string.Empty;
            if (!string.IsNullOrWhiteSpace(query_term))
            {
                queryTerm = query_term;
            }

            var genreFilter = string.Empty;
            if (!string.IsNullOrWhiteSpace(genre))
            {
                genreFilter = genre;
            }

            var hash = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    $@"type=shows&page={page}&limit={limit}&minimum_rating={minimum_rating}&query_term={
                            query_term
                        }&genre={genre}&sort_by={sort_by}"));
            try
            {
                var cachedShows = await _cachingService.GetCache(hash);
                if (cachedShows != null)
                {
                    try
                    {
                        return Content(cachedShows, "application/json");
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
                var skipParameter = new SqlParameter("@skip", (currentPage - 1) * nbShowsPerPage);
                var takeParameter = new SqlParameter("@take", nbShowsPerPage);
                var ratingParameter = new SqlParameter("@rating", minimum_rating);
                var queryParameter = new SqlParameter("@Keywords", string.Format(@"""{0}""", queryTerm));
                var genreParameter = new SqlParameter("@genre", genreFilter);
                var query = @"
                    SELECT 
                        Show.Title, Show.Year, Rating.Percentage, Rating.Loved, Rating.Votes, Rating.Hated, Rating.Watching, Show.LastUpdated, Image.Banner, Image.Poster, Show.ImdbId, Show.TvdbId, Show.GenreNames, COUNT(*) OVER () as TotalCount
                    FROM 
                        ShowSet AS Show
                    INNER JOIN 
                        ImageShowSet AS Image
                    ON 
                        Image.Id = Show.ImagesId
                    INNER JOIN 
                        RatingSet AS Rating
                    ON 
                        Rating.Id = Show.RatingId
                    WHERE
                        Show.NumSeasons <> 0";

                if (minimum_rating > 0 && minimum_rating < 10)
                {
                    query += @" AND
                        Rating >= @rating";
                }

                if (!string.IsNullOrWhiteSpace(query_term))
                {
                    query += @" AND
                        (CONTAINS(Title, @Keywords) OR CONTAINS(ImdbId, @Keywords) OR CONTAINS(TvdbId, @Keywords))";
                }

                if (!string.IsNullOrWhiteSpace(genre))
                {
                    query += @" AND
                        CONTAINS(GenreNames, @genre)";
                }

                if (!string.IsNullOrWhiteSpace(sort_by))
                {
                    switch (sort_by)
                    {
                        case "title":
                            query += " ORDER BY Show.Title ASC";
                            break;
                        case "year":
                            query += " ORDER BY Show.Year DESC";
                            break;
                        case "rating":
                            query += " ORDER BY Rating.Percentage DESC";
                            break;
                        case "loved":
                            query += " ORDER BY Rating.Loved DESC";
                            break;
                        case "votes":
                            query += " ORDER BY Rating.Votes DESC";
                            break;
                        case "watching":
                            query += " ORDER BY Rating.Watching DESC";
                            break;
                        case "date_added":
                            query += " ORDER BY Show.LastUpdated DESC";
                            break;
                        default:
                            query += " ORDER BY Show.LastUpdated DESC";
                            break;
                    }
                }
                else
                {
                    query += " ORDER BY Show.LastUpdated DESC";
                }

                query += @" OFFSET @skip ROWS 
                    FETCH NEXT @take ROWS ONLY";

                var showsQuery = await context.Database.ExecuteSqlQueryAsync(query, new CancellationToken(),
                    skipParameter, takeParameter,
                    ratingParameter, queryParameter,
                    genreParameter);
                var reader = showsQuery.DbDataReader;
                var count = 0;
                var shows = new List<ShowLightJson>();
                while (await reader.ReadAsync())
                {
                    var show = new ShowLightJson
                    {
                        Title = !await reader.IsDBNullAsync(0) ? reader.GetString(0) : string.Empty,
                        Year = !await reader.IsDBNullAsync(1) ? reader.GetInt32(1) : 0,
                        Rating = new RatingJson
                        {
                            Percentage = !await reader.IsDBNullAsync(2) ? reader.GetInt32(2) : 0,
                            Loved = !await reader.IsDBNullAsync(3) ? reader.GetInt32(3) : 0,
                            Votes = !await reader.IsDBNullAsync(4) ? reader.GetInt32(4) : 0,
                            Hated = !await reader.IsDBNullAsync(5) ? reader.GetInt32(5) : 0,
                            Watching = !await reader.IsDBNullAsync(6) ? reader.GetInt32(6) : 0
                        },
                        Images = new ImageShowJson
                        {
                            Banner = !await reader.IsDBNullAsync(8) ? reader.GetString(8) : string.Empty,
                            Poster = !await reader.IsDBNullAsync(9) ? reader.GetString(9) : string.Empty,
                        },
                        ImdbId = !await reader.IsDBNullAsync(10) ? reader.GetString(10) : string.Empty,
                        TvdbId = !await reader.IsDBNullAsync(11) ? reader.GetString(11) : string.Empty,
                        Genres = !await reader.IsDBNullAsync(12) ? reader.GetString(12) : string.Empty
                    };
                    shows.Add(show);
                    count = !await reader.IsDBNullAsync(13) ? reader.GetInt32(13) : 0;
                }

                var response = new ShowLightResponse
                {
                    TotalShows = count,
                    Shows = shows
                };

                var json = JsonSerializer.ToJsonString(response, StandardResolver.SnakeCase);
                await _cachingService.SetCache(hash, json, TimeSpan.FromDays(1));
                return Content(json, "application/json");
            }
        }

        // GET api/shows/light/tt3640424
        [HttpGet("light/{imdb}")]
        public async Task<IActionResult> GetLight(string imdb)
        {
            var hash = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"light:{imdb}"));
            try
            {
                var cachedShow = await _cachingService.GetCache(hash);
                if (cachedShow != null)
                {
                    try
                    {
                        return Content(cachedShow, "application/json");
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
                var imdbParameter = new SqlParameter("@imdbId", imdb);
                var query = @"
                    SELECT 
                        Show.Title, Show.Year, Rating.Percentage, Rating.Loved, Rating.Votes, Rating.Hated, Rating.Watching, Show.LastUpdated, Image.Banner, Image.Poster, Show.ImdbId, Show.TvdbId, Show.GenreNames
                    FROM 
                        ShowSet AS Show
                    INNER JOIN 
                        ImageShowSet AS Image
                    ON 
                        Image.Id = Show.ImagesId
                    INNER JOIN 
                        RatingSet AS Rating
                    ON 
                        Rating.Id = Show.RatingId
                    WHERE
                        Show.ImdbId = @imdbId";
                var showQuery =
                    await context.Database.ExecuteSqlQueryAsync(query, new CancellationToken(), imdbParameter);
                var reader = showQuery.DbDataReader;
                var show = new ShowLightJson();
                while (await reader.ReadAsync())
                {
                    show.Title = !await reader.IsDBNullAsync(0) ? reader.GetString(0) : string.Empty;
                    show.Year = !await reader.IsDBNullAsync(1) ? reader.GetInt32(1) : 0;
                    show.Rating = new RatingJson
                    {
                        Percentage = !await reader.IsDBNullAsync(2) ? reader.GetInt32(2) : 0,
                        Loved = !await reader.IsDBNullAsync(3) ? reader.GetInt32(3) : 0,
                        Votes = !await reader.IsDBNullAsync(4) ? reader.GetInt32(4) : 0,
                        Hated = !await reader.IsDBNullAsync(5) ? reader.GetInt32(5) : 0,
                        Watching = !await reader.IsDBNullAsync(6) ? reader.GetInt32(6) : 0
                    };
                    show.Images = new ImageShowJson
                    {
                        Banner = !await reader.IsDBNullAsync(8) ? reader.GetString(8) : string.Empty,
                        Poster = !await reader.IsDBNullAsync(9) ? reader.GetString(9) : string.Empty,
                    };
                    show.ImdbId = !await reader.IsDBNullAsync(10) ? reader.GetString(10) : string.Empty;
                    show.TvdbId = !await reader.IsDBNullAsync(11) ? reader.GetString(11) : string.Empty;
                    show.Genres = !await reader.IsDBNullAsync(12) ? reader.GetString(12) : string.Empty;
                }

                if (string.IsNullOrEmpty(show.ImdbId))
                    return BadRequest();

                var json = JsonSerializer.ToJsonString(show, StandardResolver.SnakeCase);
                await _cachingService.SetCache(hash, json, TimeSpan.FromDays(1));
                return Content(json, "application/json");
            }
        }

        // POST api/shows/ids
        [HttpPost]
        [Route("ids")]
        public async Task<IActionResult> GetShowByIds([FromBody] IEnumerable<string> imdbIds)
        {
            if (!imdbIds.Any())
            {
                return Json(new ShowLightResponse
                {
                    Shows = new List<ShowLightJson>(),
                    TotalShows = 0
                });
            }

            var hash = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    $@"type=shows&imdbIds={string.Join(',', imdbIds)}"));
            try
            {
                var cachedShow = await _cachingService.GetCache(hash);
                if (cachedShow != null)
                {
                    try
                    {
                        return Content(cachedShow, "application/json");
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
                var query = @"
                    SELECT DISTINCT
                        Show.Title, Show.Year, Rating.Percentage, Rating.Loved, Rating.Votes, Rating.Hated, Rating.Watching, Show.LastUpdated, Image.Banner, Image.Poster, Show.ImdbId, Show.TvdbId, Show.GenreNames, COUNT(*) OVER () as TotalCount
                    FROM 
                        ShowSet AS Show
                    INNER JOIN 
                        ImageShowSet AS Image
                    ON 
                        Image.Id = Show.ImagesId
                    INNER JOIN 
                        RatingSet AS Rating
                    ON 
                        Rating.Id = Show.RatingId
                    WHERE
                        Show.ImdbId IN ({@imdbIds})";

                using (var cmd = new SqlCommand(query,
                    new SqlConnection(context.Database.GetDbConnection().ConnectionString)))
                {
                    cmd.AddArrayParameters(imdbIds, "@imdbIds");
                    await cmd.Connection.OpenAsync();
                    var reader = await cmd.ExecuteReaderAsync(new CancellationToken());
                    var count = 0;
                    var shows = new List<ShowLightJson>();
                    while (await reader.ReadAsync())
                    {
                        var show = new ShowLightJson
                        {
                            Title = !await reader.IsDBNullAsync(0) ? reader.GetString(0) : string.Empty,
                            Year = !await reader.IsDBNullAsync(1) ? reader.GetInt32(1) : 0,
                            Rating = new RatingJson
                            {
                                Percentage = !await reader.IsDBNullAsync(2) ? reader.GetInt32(2) : 0,
                                Loved = !await reader.IsDBNullAsync(3) ? reader.GetInt32(3) : 0,
                                Votes = !await reader.IsDBNullAsync(4) ? reader.GetInt32(4) : 0,
                                Hated = !await reader.IsDBNullAsync(5) ? reader.GetInt32(5) : 0,
                                Watching = !await reader.IsDBNullAsync(6) ? reader.GetInt32(6) : 0
                            },
                            Images = new ImageShowJson
                            {
                                Banner = !await reader.IsDBNullAsync(8) ? reader.GetString(8) : string.Empty,
                                Poster = !await reader.IsDBNullAsync(9) ? reader.GetString(9) : string.Empty,
                            },
                            ImdbId = !await reader.IsDBNullAsync(10) ? reader.GetString(10) : string.Empty,
                            TvdbId = !await reader.IsDBNullAsync(11) ? reader.GetString(11) : string.Empty,
                            Genres = !await reader.IsDBNullAsync(12) ? reader.GetString(12) : string.Empty
                        };

                        shows.Add(show);
                        count = !await reader.IsDBNullAsync(13) ? reader.GetInt32(13) : 0;
                    }

                    var response = new ShowLightResponse
                    {
                        TotalShows = count,
                        Shows = shows
                    };

                    var json = JsonSerializer.ToJsonString(response, StandardResolver.SnakeCase);
                    await _cachingService.SetCache(hash, json, TimeSpan.FromDays(1));
                    return Content(json, "application /json");
                }
            }
        }

        // GET api/shows/tt3640424
        [HttpGet("{imdb}")]
        public async Task<IActionResult> Get(string imdb)
        {
            var hash = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(imdb));
            try
            {
                var cachedShow = await _cachingService.GetCache(hash);
                if (cachedShow != null)
                {
                    try
                    {
                        return Content(cachedShow, "application/json");
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
                var show = await context.ShowSet.Include(a => a.Rating)
                    .Include(a => a.Episodes)
                    .ThenInclude(episode => episode.Torrents)
                    .ThenInclude(torrent => torrent.Torrent0)
                    .Include(a => a.Episodes)
                    .ThenInclude(episode => episode.Torrents)
                    .ThenInclude(torrent => torrent.Torrent1080p)
                    .Include(a => a.Episodes)
                    .ThenInclude(episode => episode.Torrents)
                    .ThenInclude(torrent => torrent.Torrent480p)
                    .Include(a => a.Episodes)
                    .Include(a => a.Episodes)
                    .ThenInclude(episode => episode.Torrents)
                    .ThenInclude(torrent => torrent.Torrent720p)
                    .Include(a => a.Genres)
                    .Include(a => a.Images)
                    .Include(a => a.Similars).AsQueryable()
                    .FirstOrDefaultAsync(a => a.ImdbId == imdb);
                if (show == null) return BadRequest();

                var showJson = ConvertShowToJson(show);
                var json = JsonSerializer.ToJsonString(showJson, StandardResolver.SnakeCase);
                await _cachingService.SetCache(hash, json, TimeSpan.FromDays(1));
                return Content(json, "application/json");
            }
        }

        /// <summary>
        /// Convert a <see cref="Show"/> to a <see cref="ShowJson"/>
        /// </summary>
        /// <param name="show"></param>
        /// <returns></returns>
        private ShowJson ConvertShowToJson(Show show)
        {
            return new ShowJson
            {
                AirDay = show.AirDay,
                Rating = new RatingJson
                {
                    Hated = show.Rating?.Hated,
                    Loved = show.Rating?.Loved,
                    Percentage = show.Rating?.Percentage,
                    Votes = show.Rating?.Votes,
                    Watching = show.Rating?.Watching
                },
                Title = show.Title,
                Genres = show.Genres.Select(genre => genre.Name),
                Year = show.Year,
                ImdbId = show.ImdbId,
                Episodes = show.Episodes.Select(episode => new EpisodeShowJson
                {
                    DateBased = episode.DateBased,
                    EpisodeNumber = episode.EpisodeNumber,
                    Torrents = new TorrentShowNodeJson
                    {
                        Torrent_0 = new TorrentShowJson
                        {
                            Peers = episode.Torrents?.Torrent0?.Peers,
                            Seeds = episode.Torrents?.Torrent0?.Seeds,
                            Provider = episode.Torrents?.Torrent0?.Provider,
                            Url = episode.Torrents?.Torrent0?.Url
                        },
                        Torrent_1080p = new TorrentShowJson
                        {
                            Peers = episode.Torrents?.Torrent1080p?.Peers,
                            Seeds = episode.Torrents?.Torrent1080p?.Seeds,
                            Provider = episode.Torrents?.Torrent1080p?.Provider,
                            Url = episode.Torrents?.Torrent1080p?.Url
                        },
                        Torrent_720p = new TorrentShowJson
                        {
                            Peers = episode.Torrents?.Torrent720p?.Peers,
                            Seeds = episode.Torrents?.Torrent720p?.Seeds,
                            Provider = episode.Torrents?.Torrent720p?.Provider,
                            Url = episode.Torrents?.Torrent720p?.Url
                        },
                        Torrent_480p = new TorrentShowJson
                        {
                            Peers = episode.Torrents?.Torrent480p?.Peers,
                            Seeds = episode.Torrents?.Torrent480p?.Seeds,
                            Provider = episode.Torrents?.Torrent480p?.Provider,
                            Url = episode.Torrents?.Torrent480p?.Url
                        }
                    },
                    FirstAired = episode.FirstAired,
                    Title = episode.Title,
                    Overview = episode.Overview,
                    Season = episode.Season,
                    TvdbId = episode.TvdbId
                }).ToList(),
                TvdbId = show.TvdbId,
                AirTime = show.AirTime,
                Country = show.Country,
                Images = new ImageShowJson
                {
                    Banner = show.Images?.Banner,
                    Poster = show.Images?.Poster
                },
                LastUpdated = show.LastUpdated,
                Network = show.Network,
                NumSeasons = show.NumSeasons,
                Runtime = show.Runtime,
                Slug = show.Slug,
                Status = show.Status,
                Synopsis = show.Synopsis,
                Similar = show.Similars.Select(a => a.TmdbId).ToList()
            };
        }
    }
}