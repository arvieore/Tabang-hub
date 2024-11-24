using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tabang_Hub.Utils
{
    public class RecruitmentResult
    {
        public List<FilteredVolunteer> SortedByRating { get; set; }
        public List<FilteredVolunteer> SortedByAvailability { get; set; }
    }
}