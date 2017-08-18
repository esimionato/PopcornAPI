using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PopcornApi.Models.Cast;
using PopcornApi.Models.Torrent.Movie;

namespace PopcornApi.Models.Movie
{
    public class MovieLightJson
    {
        [JsonProperty("imdb_code")]
        public string ImdbCode { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("rating")]
        public double Rating { get; set; }

        [JsonProperty("genres")]
        public string Genres { get; set; }

        [JsonProperty("poster_image")]
        public string PosterImage { get; set; }
    }
}
