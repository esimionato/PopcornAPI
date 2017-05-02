using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PopcornApi.Models.User
{
    public class MovieHistoryJson
    {
        [JsonProperty("ImdbId")]
        public string ImdbId { get; set; }

        [JsonProperty("Seen")]
        public bool Seen { get; set; }

        [JsonProperty("Favorite")]
        public bool Favorite { get; set; }
    }
}
