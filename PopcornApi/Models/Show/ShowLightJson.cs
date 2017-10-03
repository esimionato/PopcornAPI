using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PopcornApi.Models.Episode;
using PopcornApi.Models.Image;
using PopcornApi.Models.Rating;

namespace PopcornApi.Models.Show
{
    public class ShowLightJson
    {
        [DataMember(Name = "imdb_id")]
        public string ImdbId { get; set; }

        [DataMember(Name = "tvdb_id")]
        public string TvdbId { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "year")]
        public int Year { get; set; }

        [DataMember(Name = "genres")]
        public string Genres { get; set; }

        [DataMember(Name = "images")]
        public ImageShowJson Images { get; set; }

        [DataMember(Name = "rating")]
        public RatingJson Rating { get; set; }
    }
}
