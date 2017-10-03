using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PopcornApi.Models.Show
{
    public class ShowLightResponse
    {
        [DataMember(Name = "totalShows")]
        public long TotalShows { get; set; }

        [DataMember(Name = "shows")]
        public IEnumerable<ShowLightJson> Shows { get; set; }
    }
}
