
//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------


namespace Tabang_Hub
{

using System;
    using System.Collections.Generic;
    
public partial class VolunteerInfo
{

    public int volunteerId { get; set; }

    public Nullable<int> userId { get; set; }

    public string fName { get; set; }

    public string lName { get; set; }

    public Nullable<System.DateTime> bDay { get; set; }

    public string gender { get; set; }

    public string phoneNum { get; set; }

    public string street { get; set; }

    public string city { get; set; }

    public string province { get; set; }

    public string zipCode { get; set; }

    public Nullable<int> profilePath { get; set; }

    public string availability { get; set; }

    public string aboutMe { get; set; }



    public virtual ProfilePicture ProfilePicture { get; set; }

    public virtual UserAccount UserAccount { get; set; }

}

}
