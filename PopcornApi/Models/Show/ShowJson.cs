﻿using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using PopcornApi.Models.Episode;
using PopcornApi.Models.Image;
using PopcornApi.Models.Rating;
using PopcornApi.Models.Subtitle;

namespace PopcornApi.Models.Show
{
    public class ShowJson
    {
        [DataMember(Name = "imdb_id")]
        public string ImdbId { get; set; }

        [DataMember(Name = "tvdb_id")]
        public string TvdbId { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "year")]
        public int Year { get; set; }

        [DataMember(Name = "slug")]
        public string Slug { get; set; }

        [DataMember(Name = "synopsis")]
        public string Synopsis { get; set; }

        [DataMember(Name = "runtime")]
        public string Runtime { get; set; }

        [DataMember(Name = "country")]
        public string Country { get; set; }

        [DataMember(Name = "network")]
        public string Network { get; set; }

        [DataMember(Name = "air_day")]
        public string AirDay { get; set; }

        [DataMember(Name = "air_time")]
        public string AirTime { get; set; }

        [DataMember(Name = "status")]
        public string Status { get; set; }

        [DataMember(Name = "num_seasons")]
        public int NumSeasons { get; set; }

        [DataMember(Name = "last_updated")]
        public long LastUpdated { get; set; }

        [DataMember(Name = "episodes")]
        public List<EpisodeShowJson> Episodes { get; set; }

        [DataMember(Name = "genres")]
        public IEnumerable<string> Genres { get; set; }

        [DataMember(Name = "images")]
        public ImageShowJson Images { get; set; }

        [DataMember(Name = "rating")]
        public RatingJson Rating { get; set; }

        [DataMember(Name = "similar")]
        public List<string> Similar { get; set; }

        [DataMember(Name = "subtitles")]
        public List<SubtitleJson> Subtitles { get; set; }
    }
}
