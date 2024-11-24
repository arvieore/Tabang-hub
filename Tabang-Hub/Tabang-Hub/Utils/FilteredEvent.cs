using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tabang_Hub.Utils
{
    public class FilteredEvent
    {
        public int EventID { get; set; }
        public string EventDescription { get; set; }
        public double similarityScore { get; set; }
        public List<int> RequiredSkills { get; set; }
        public List<int> MatchedSkills { get; set; }
    }
}