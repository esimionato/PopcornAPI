using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace PopcornApi.Models.Subtitle
{
    public class SubtitleJson
    {
        [DataMember(Name = "osdbSubtitleId")]
        public string OsdbSubtitleId { get; set; }

        [DataMember(Name = "subtitleFileName")]
        public string SubtitleFileName { get; set; }

        [DataMember(Name = "imdbId")]
        public string ImdbId { get; set; }

        [DataMember(Name = "languageId")]
        public string LanguageId { get; set; }

        [DataMember(Name = "languageName")]
        public string LanguageName { get; set; }

        [DataMember(Name = "rating")]
        public double Rating { get; set; }

        [DataMember(Name = "bad")]
        public double Bad { get; set; }

        [DataMember(Name = "subtitleDownloadLink")]
        public string SubtitleDownloadLink { get; set; }

        [DataMember(Name = "iso639")]
        public string Iso639 { get; set; }
    }
}
