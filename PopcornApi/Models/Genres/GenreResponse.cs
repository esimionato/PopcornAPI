using System.Collections.Generic;
using Newtonsoft.Json;

namespace PopcornApi.Models.Genres
{
    public class Genre
    {
        [JsonProperty("EnglishName")]
        public string EnglishName { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }
    }

    public class GenreResponse
    {
        [JsonProperty("genres")]
        public IEnumerable<Genre> Genres { get; set; }
    }
}
