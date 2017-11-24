using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PopcornApi.Models.Image
{
    public class ImageShowJson
    {
        [DataMember(Name = "poster")]
        public string Poster { get; set; }

        [DataMember(Name = "banner")]
        public string Banner { get; set; }
    }
}
