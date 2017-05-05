using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PopcornApi.Models.User
{
    public class UserJson
    {
        [JsonProperty("MachineGuid")]
        public Guid MachineGuid { get; set; }

        [JsonProperty("DownloadLimit")]
        public int DownloadLimit { get; set; }

        [JsonProperty("UploadLimit")]
        public int UploadLimit { get; set; }

        [JsonProperty("DefaultHdQuality")]
        public bool DefaultHdQuality { get; set; }

        [JsonProperty("DefaultSubtitleLanguage")]
        public string DefaultSubtitleLanguage { get; set; }

        [JsonProperty("Language")]
        public LanguageJson Language { get; set; }

        [JsonProperty("MovieHistory")]
        public List<MovieHistoryJson> MovieHistory { get; set; }

        [JsonProperty("ShowHistory")]
        public List<ShowHistoryJson> ShowHistory { get; set; }
    }
}
