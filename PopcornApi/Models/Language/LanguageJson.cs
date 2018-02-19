using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace PopcornApi.Models.Language
{
    public class LanguageJson
    {
        [DataMember(Name = "languageName")]
        public string LanguageName { get; set; }

        [DataMember(Name = "subLanguageId")]
        public string SubLanguageId { get; set; }

        [DataMember(Name = "iso639")]
        public string Iso639 { get; set; }
    }
}
