using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PopcornApi.Models.User
{
    public class LanguageJson
    {
        [JsonProperty("Culture")]
        public string Culture { get; set; }
    }
}
