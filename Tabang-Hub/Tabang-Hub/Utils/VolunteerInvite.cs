using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tabang_Hub.Utils
{
    public class VolunteerInvite
    {
        public int? UserId { get; set; }
        public string FullName { get; set; }
        public double OverallRating { get; set; }
        public List<int> SkillIds { get; set; }
        public List<VolunteerSkill> skills { get; set; }
        public double SimilarityScore { get; set; }
        public string Feedback { get; set; }
        public string Sentiment { get; set; }
        public string Availability {  get; set; }
    }
}