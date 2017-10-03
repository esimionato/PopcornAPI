using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using PopcornApi.Attributes;
using PopcornApi.Database;
using PopcornApi.Models.Cast;
using PopcornApi.Models.Movie;
using PopcornApi.Models.Torrent.Movie;
using PopcornApi.Services.Caching;
using PopcornApi.Services.Logging;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Text;
using System.Threading;
using PopcornApi.Extensions;
using PopcornApi.Helpers;
using JsonSerializer = Utf8Json.JsonSerializer;

namespace PopcornApi.Controllers
{
    [Route("api/[controller]")]
    public class MoviesController : Controller
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
        public MoviesController(ILoggingService loggingService, ICachingService cachingService)
        {
            _loggingService = loggingService;
            _cachingService = cachingService;

        }

        // GET api/movies
        [HttpGet]
        public async Task<IActionResult> Get([RequiredFromQuery] int page, [FromQuery] int limit,
            [FromQuery] int minimum_rating, [FromQuery] string query_term,
            [FromQuery] string genre, [FromQuery] string sort_by)
        {
            var nbMoviesPerPage = 20;
            if (limit >= 20 && limit <= 50)
                nbMoviesPerPage = limit;

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
                    $@"type=movies&page={page}&limit={limit}&minimum_rating={minimum_rating}&query_term={
                            query_term
                        }&genre={genre}&sort_by={sort_by}"));
            try
            {
                var cachedMovies = await _cachingService.GetCache(hash);
                if (cachedMovies != null)
                {
                    try
                    {
                        return Json(JsonSerializer.Deserialize<MovieLightResponse>(cachedMovies));
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
                var skipParameter = new SqlParameter("@skip", (currentPage - 1) * nbMoviesPerPage);
                var takeParameter = new SqlParameter("@take", nbMoviesPerPage);
                var ratingParameter = new SqlParameter("@rating", minimum_rating);
                var queryParameter = new SqlParameter("@Keywords", queryTerm);
                var genreParameter = new SqlParameter("@genre", genreFilter);
                var query = @"
                    SELECT 
                        Movie.Title, Movie.Year, Movie.Rating, Movie.PosterImage, Movie.ImdbCode, Movie.GenreNames, Torrent.Peers, Torrent.Seeds, COUNT(*) OVER () as TotalCount
                    FROM 
                        MovieSet AS Movie
                    INNER JOIN
                        TorrentMovieSet AS Torrent
                    ON 
                        Torrent.MovieId = Movie.Id
                    AND 
                        Torrent.Quality = '720p'
                    WHERE
                        1 = 1";

                if (minimum_rating > 0 && minimum_rating < 10)
                {
                    query += @" AND
                        Rating >= @rating";
                }

                if (!string.IsNullOrWhiteSpace(query_term))
                {
                    query += @" AND
                        FREETEXT(Title, @Keywords)";
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
                            query += " ORDER BY Movie.Title ASC";
                            break;
                        case "year":
                            query += " ORDER BY Movie.Year DESC";
                            break;
                        case "rating":
                            query += " ORDER BY Movie.Rating DESC";
                            break;
                        case "peers":
                            query += " ORDER BY Torrent.Peers DESC";
                            break;
                        case "seeds":
                            query += " ORDER BY Torrent.Seeds DESC";
                            break;
                        case "download_count":
                            query += " ORDER BY Movie.DownloadCount DESC";
                            break;
                        case "like_count":
                            query += " ORDER BY Movie.LikeCount DESC";
                            break;
                        case "date_added":
                            query += " ORDER BY Movie.DateUploadedUnix DESC";
                            break;
                        default:
                            query += " ORDER BY Movie.DateUploadedUnix DESC";
                            break;
                    }
                }
                else
                {
                    query += " ORDER BY Movie.DateUploadedUnix DESC";
                }

                query += @" OFFSET @skip ROWS 
                    FETCH NEXT @take ROWS ONLY";

                var moviesQuery = await context.Database.ExecuteSqlQueryAsync(query, new CancellationToken(),
                    skipParameter, takeParameter,
                    ratingParameter, queryParameter,
                    genreParameter);
                var reader = moviesQuery.DbDataReader;
                var count = 0;
                var movies = new List<MovieLightJson>();
                while (await reader.ReadAsync())
                {
                    var movie = new MovieLightJson
                    {
                        Title = reader[0].GetType() != typeof(DBNull) ? (string) reader[0] : string.Empty,
                        Year = reader[1].GetType() != typeof(DBNull) ? (int) reader[1] : 0,
                        Rating = reader[2].GetType() != typeof(DBNull) ? (double) reader[2] : 0d,
                        PosterImage = reader[3].GetType() != typeof(DBNull) ? (string) reader[3] : string.Empty,
                        ImdbCode = reader[4].GetType() != typeof(DBNull) ? (string) reader[4] : string.Empty,
                        Genres = reader[5].GetType() != typeof(DBNull) ? (string) reader[5] : string.Empty
                    };
                    movies.Add(movie);
                    count = reader[8].GetType() != typeof(DBNull) ? (int) reader[8] : 0;
                }

                var response = new MovieLightResponse
                {
                    TotalMovies = count,
                    Movies = movies
                };

                await _cachingService.SetCache(hash, JsonSerializer.ToJsonString(response), TimeSpan.FromDays(1));
                return
                    Json(response);
            }
        }

        // GET api/similar
        [HttpPost]
        [Route("similar")]
        public async Task<IActionResult> GetSimilar([FromBody] IEnumerable<string> imdbIds, [RequiredFromQuery] int page, [FromQuery] int limit)
        {
            if (!imdbIds.Any())
            {
                return Json(new MovieLightResponse
                {
                    Movies = new List<MovieLightJson>(),
                    TotalMovies = 0
                });
            }

            var nbMoviesPerPage = 20;
            if (limit >= 20 && limit <= 50)
                nbMoviesPerPage = limit;

            var currentPage = 1;
            if (page >= 1)
            {
                currentPage = page;
            }

            var hash = Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    $@"type=movies&page={page}&limit={limit}&imdbId={string.Join(',', imdbIds)}"));
            try
            {
                var cachedMovies = await _cachingService.GetCache(hash);
                if (cachedMovies != null)
                {
                    try
                    {
                        return Json(JsonSerializer.Deserialize<MovieLightResponse>(cachedMovies));
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
                var skipParameter = new SqlParameter("@skip", (currentPage - 1) * nbMoviesPerPage);
                var takeParameter = new SqlParameter("@take", nbMoviesPerPage);
                var query = @"
                    SELECT DISTINCT
                        Movie.Title, Movie.Year, Movie.Rating, Movie.PosterImage, Movie.ImdbCode, Movie.GenreNames, Torrent.Peers, Torrent.Seeds, COUNT(*) OVER () as TotalCount
                    FROM 
                        MovieSet AS Movie
                    INNER JOIN
                        TorrentMovieSet AS Torrent
                    ON 
                        Torrent.MovieId = Movie.Id
                    INNER JOIN
                        Similar
                    ON 
                        Similar.MovieId = Movie.Id
                    AND 
                        Torrent.Quality = '720p'
                    WHERE
                        Similar.TmdbId IN ({@imdbIds})
                    ORDER BY Movie.Rating DESC";

                query += @" OFFSET @skip ROWS 
                    FETCH NEXT @take ROWS ONLY";

                using (var cmd = new SqlCommand(query, new SqlConnection(context.Database.GetDbConnection().ConnectionString)))
                {
                    cmd.AddArrayParameters(imdbIds, "@imdbIds");
                    cmd.Parameters.Add(skipParameter);
                    cmd.Parameters.Add(takeParameter);
                    await cmd.Connection.OpenAsync();
                    var reader = await cmd.ExecuteReaderAsync(new CancellationToken());
                    var count = 0;
                    var movies = new List<MovieLightJson>();
                    while (await reader.ReadAsync())
                    {
                        var movie = new MovieLightJson
                        {
                            Title = reader[0].GetType() != typeof(DBNull) ? (string) reader[0] : string.Empty,
                            Year = reader[1].GetType() != typeof(DBNull) ? (int) reader[1] : 0,
                            Rating = reader[2].GetType() != typeof(DBNull) ? (double) reader[2] : 0d,
                            PosterImage = reader[3].GetType() != typeof(DBNull) ? (string) reader[3] : string.Empty,
                            ImdbCode = reader[4].GetType() != typeof(DBNull) ? (string) reader[4] : string.Empty,
                            Genres = reader[5].GetType() != typeof(DBNull) ? (string) reader[5] : string.Empty
                        };
                        movies.Add(movie);
                        count = reader[8].GetType() != typeof(DBNull) ? (int) reader[8] : 0;
                    }

                    var response = new MovieLightResponse
                    {
                        TotalMovies = count,
                        Movies = movies
                    };
                    await _cachingService.SetCache(hash, JsonSerializer.ToJsonString(response), TimeSpan.FromDays(1));
                    return
                        Json(response);
                }
            }
        }

        // GET api/movies/light/tt3640424
        [HttpGet("light/{imdb}")]
        public async Task<IActionResult> GetLight(string imdb)
        {
            var hash = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"light:{imdb}"));
            try
            {
                var cachedMovie = await _cachingService.GetCache(hash);
                if (cachedMovie != null)
                {
                    try
                    {
                        return Json(JsonSerializer.Deserialize<MovieLightJson>(cachedMovie));
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
                var imdbParameter = new SqlParameter("@imdbCode", imdb);
                var query = @"
                    SELECT 
                        Movie.Title, Movie.Year, Movie.Rating, Movie.PosterImage, Movie.ImdbCode, Movie.GenreNames
                    FROM 
                        MovieSet AS Movie
                    WHERE
                        Movie.ImdbCode = @imdbCode";
                var movieQuery =
                    await context.Database.ExecuteSqlQueryAsync(query, new CancellationToken(), imdbParameter);
                var reader = movieQuery.DbDataReader;
                var movie = new MovieLightJson();
                while (await reader.ReadAsync())
                {
                    movie.Title = reader[0].GetType() != typeof(DBNull) ? (string) reader[0] : string.Empty;
                    movie.Year = reader[1].GetType() != typeof(DBNull) ? (int) reader[1] : 0;
                    movie.Rating = reader[2].GetType() != typeof(DBNull) ? (double) reader[2] : 0d;
                    movie.PosterImage = reader[3].GetType() != typeof(DBNull) ? (string) reader[3] : string.Empty;
                    movie.ImdbCode = reader[4].GetType() != typeof(DBNull) ? (string) reader[4] : string.Empty;
                    movie.Genres = reader[5].GetType() != typeof(DBNull) ? (string) reader[5] : string.Empty;
                }

                if (string.IsNullOrEmpty(movie.ImdbCode))
                    return BadRequest();

                await _cachingService.SetCache(hash, JsonSerializer.ToJsonString(movie));
                return Json(movie);
            }
        }

        // GET api/movies/tt3640424
        [HttpGet("{imdb}")]
        public async Task<IActionResult> Get(string imdb)
        {
            var hash = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"full:{imdb}"));
            try
            {
                var cachedMovie = await _cachingService.GetCache(hash);
                if (cachedMovie != null)
                {
                    try
                    {
                        return Json(JsonSerializer.Deserialize<MovieJson>(cachedMovie));
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
                var movie =
                    await context.MovieSet.Include(a => a.Torrents)
                        .Include(a => a.Cast)
                        .Include(a => a.Similars)
                        .Include(a => a.Genres).AsQueryable()
                        .FirstOrDefaultAsync(
                            document => document.ImdbCode == imdb);
                if (movie == null) return BadRequest();

                var movieJson = ConvertMovieToJson(movie);
                await _cachingService.SetCache(hash, JsonSerializer.ToJsonString(movieJson));
                return Json(movieJson);
            }
        }

        /// <summary>
        /// Convert a <see cref="Movie"/> to a <see cref="MovieJson"/>
        /// </summary>
        /// <param name="movie"></param>
        /// <returns></returns>
        private MovieJson ConvertMovieToJson(Movie movie)
        {
            return new MovieJson
            {
                Rating = movie.Rating,
                Torrents = movie.Torrents.Select(torrent => new TorrentMovieJson
                {
                    DateUploadedUnix = torrent.DateUploadedUnix,
                    Peers = torrent.Peers,
                    Seeds = torrent.Seeds,
                    Quality = torrent.Quality,
                    Url = torrent.Url,
                    DateUploaded = torrent.DateUploaded,
                    Hash = torrent.Hash,
                    Size = torrent.Size,
                    SizeBytes = torrent.SizeBytes
                }).ToList(),
                Title = movie.Title,
                DateUploadedUnix = movie.DateUploadedUnix,
                Genres = movie.Genres.Select(genre => genre.Name).ToList(),
                Cast = movie.Cast.Select(cast => new CastJson
                {
                    CharacterName = cast.CharacterName,
                    Name = cast.Name,
                    ImdbCode = cast.ImdbCode,
                    SmallImage = cast.SmallImage
                }).ToList(),
                Runtime = movie.Runtime,
                Url = movie.Url,
                Year = movie.Year,
                Slug = movie.Slug,
                LikeCount = movie.LikeCount,
                DownloadCount = movie.DownloadCount,
                ImdbCode = movie.ImdbCode,
                DateUploaded = movie.DateUploaded,
                BackdropImage = movie.BackdropImage,
                BackgroundImage = movie.BackgroundImage,
                DescriptionFull = movie.DescriptionFull,
                DescriptionIntro = movie.DescriptionIntro,
                Language = movie.Language,
                LargeCoverImage = movie.LargeCoverImage,
                LargeScreenshotImage1 = movie.LargeScreenshotImage1,
                LargeScreenshotImage2 = movie.LargeScreenshotImage2,
                LargeScreenshotImage3 = movie.LargeScreenshotImage3,
                MediumCoverImage = movie.MediumCoverImage,
                MediumScreenshotImage1 = movie.MediumScreenshotImage1,
                MediumScreenshotImage2 = movie.MediumScreenshotImage2,
                MediumScreenshotImage3 = movie.MediumScreenshotImage3,
                MpaRating = movie.MpaRating,
                PosterImage = movie.PosterImage,
                SmallCoverImage = movie.SmallCoverImage,
                TitleLong = movie.TitleLong,
                YtTrailerCode = movie.YtTrailerCode,
                Similar = movie.Similars.Select(a => a.TmdbId).ToList()
            };
        }
    }
}