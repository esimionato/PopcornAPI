using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PopcornApi.Models.Movie
{
    public class MovieResponse
    {
        [DataMember(Name = "totalMovies")]
        public int TotalMovies { get; set; }

        [DataMember(Name = "movies")]
        public IEnumerable<MovieJson> Movies { get; set; }
    }
}