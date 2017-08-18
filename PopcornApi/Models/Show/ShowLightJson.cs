using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PopcornApi.Models.Episode;
using PopcornApi.Models.Image;
using PopcornApi.Models.Rating;

namespace PopcornApi.Models.Show
{
    public class ShowLightJson
    {
        [JsonProperty("imdb_id")]
        public string ImdbId { get; set; }

        [JsonProperty("tvdb_id")]
        public string TvdbId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("year")]
        public int Year { get; set; }

        [JsonProperty("genres")]
        public string Genres { get; set; }

        [JsonProperty("images")]
        public ImageShowJson Images { get; set; }

        [JsonProperty("rating")]
        public RatingJson Rating { get; set; }
    }
}
