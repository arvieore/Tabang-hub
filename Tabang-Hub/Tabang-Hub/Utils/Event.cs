using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tabang_Hub.Utils
{
    public class Event
    {
        public int eventId { get; set; }
        public string Title { get; set; }
        public string Details { get; set; }
        public string Start_Date { get; set; }
        public string End_Date { get; set; }
        public int? Status { get; set; }
        public string Image { get; set; }
        public string Location { get; set; }
        public List<string> SkillName { get; set; }
        public List<int?> Rating { get; set; }
    }
}