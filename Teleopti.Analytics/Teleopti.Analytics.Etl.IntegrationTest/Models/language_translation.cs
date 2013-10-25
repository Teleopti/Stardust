using System;
using System.Collections.Generic;

namespace Teleopti.Analytics.Etl.IntegrationTest.Models
{
    public partial class language_translation
    {
        public string Culture { get; set; }
        public int language_id { get; set; }
        public string ResourceKey { get; set; }
        public string term_language { get; set; }
        public string term_english { get; set; }
    }
}
