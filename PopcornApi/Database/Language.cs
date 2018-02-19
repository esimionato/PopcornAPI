using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PopcornApi.Database
{
    public partial class Language
    {
        public int Id { get; set; }
        public string SubLanguageId { get; set; }
        public string LanguageName { get; set; }
        public string Iso639 { get; set; }
        public bool OpusArchiveDownloaded { get; set; }
    }
}
