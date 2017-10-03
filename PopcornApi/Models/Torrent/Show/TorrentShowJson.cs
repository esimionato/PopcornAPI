using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PopcornApi.Models.Torrent.Show
{
    public class TorrentShowJson
    {
        [DataMember(Name = "provider")]
        public string Provider { get; set; }

        [DataMember(Name = "peers")]
        public int? Peers { get; set; }

        [DataMember(Name = "seeds")]
        public int? Seeds { get; set; }

        [DataMember(Name = "url")]
        public string Url { get; set; }
    }
}
