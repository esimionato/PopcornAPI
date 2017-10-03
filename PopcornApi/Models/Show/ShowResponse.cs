using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PopcornApi.Models.Show
{
    public class ShowResponse
    {
        [DataMember(Name = "totalShows")]
        public long TotalShows { get; set; }

        [DataMember(Name = "shows")]
        public IEnumerable<ShowJson> Shows { get; set; }
    }
}