using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tabang_Hub.Utils
{
    public class FilteredVolunteer
    {
        public double Rating { get; set; }
        public int userId { get; set; }
        public List<int> matchedSkills { get; set; }
        public double rating { get; set; }
        public double similarityScore { get; set; }
        public string Availability { get; set; }
    }
}