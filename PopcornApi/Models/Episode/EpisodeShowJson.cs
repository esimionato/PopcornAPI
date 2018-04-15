using System.Collections.Generic;
using System.Runtime.Serialization;
using PopcornApi.Models.Torrent.Show;

namespace PopcornApi.Models.Episode
{
    public class EpisodeShowJson
    {
        [DataMember(Name = "torrents")]
        public TorrentShowNodeJson Torrents { get; set; }

        [DataMember(Name = "first_aired")]
        public long FirstAired { get; set; }

        [DataMember(Name = "date_based")]
        public bool DateBased { get; set; }

        [DataMember(Name = "overview")]
        public string Overview { get; set; }

        [DataMember(Name = "title")]
        public string Title { get; set; }

        [DataMember(Name = "episode")]
        public int EpisodeNumber { get; set; }

        [DataMember(Name = "season")]
        public int Season { get; set; }

        [DataMember(Name = "tvdb_id")]
        public int? TvdbId { get; set; }
    }
}
