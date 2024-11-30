using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tabang_Hub.Utils
{
    public class View_VolunteerInfo
    {
        public string FullName { get; set; }
        public string BirthDay { get; set; }
        public string PhoneNum { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string Province { get; set; }
        public string Zip { get; set; }
        public string Availability { get; set; }
        public string About { get; set; }
        public string ProfilePic { get; set; }
        public List<string> Skills { get; set; }
        public double AverageRate { get; set; }
    }
}