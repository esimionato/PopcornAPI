namespace PopcornApi.Database
{
    using System;
    using System.Collections.Generic;

    public partial class Subtitle
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Subtitle()
        {
        }

        public int Id { get; set; }
        public string OsdbSubtitleId { get; set; }
        public string SubtitleFileName { get; set; }
        public string ImdbId { get; set; }
        public string LanguageId { get; set; }
        public string LanguageName { get; set; }
        public double Rating { get; set; }
        public double Bad { get; set; }
        public string SubtitleDownloadLink { get; set; }
        public string Iso639 { get; set; }
    }
}
