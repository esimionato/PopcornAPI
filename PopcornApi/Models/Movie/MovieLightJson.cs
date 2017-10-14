using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PopcornApi.Models.Cast;
using PopcornApi.Models.Torrent.Movie;

namespace PopcornApi.Models.Movie
{
    public class MovieLightJson
    {
        [DataMember(Name = "imdb_code")]
        public string ImdbCode { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "year")]
        public int Year { get; set; }

        [DataMember(Name = "rating")]
        public double Rating { get; set; }

        [DataMember(Name = "genres")]
        public string Genres { get; set; }

        [DataMember(Name = "poster_image")]
        public string PosterImage { get; set; }
    }
}
