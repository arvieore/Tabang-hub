using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tabang_Hub.Utils
{
    public class Lists
    {
        public OrgEvents CreateEvents {  get; set; }
        public OrgSkillRequirement skillRequirement { get; set; }       
        public OrgEvents eventDetails { get; set; }
        public UserAccount userAccount { get; set; }
        public OrgInfo OrgInfo { get; set; }
        public ProfilePicture profilePic { get; set; }
        public OrgValidation orgValidation { get; set; }
        public Volunteers volunteer { get; set; }
        public VolunteerInfo volunteerInfo { get; set; }

        //Tables List
        //public List<UserAccount> userAccounts { get; set; }
        //public List<VolunteerInfo> volunteersInfo { get; set; }
        //public List<VolunteerSkill> volunteersSkill { get; set; }
        //public List<Skills> skills { get; set; }
        //public List<ProfilePicture> picture { get; set; }
        public List<vw_VolunteerAccounts> volunteerAccounts { get; set; }
        public List<OrgInfo> recentOrgAcc {  get; set; }
        public List<Skills> allSkill { get; set; }
        public List<UserAccount> pendingOrg {  get; set; }
        public List<Skills> listOfSkills { get; set; }
        public List<vw_OrganizationAccounts> organizationAccounts { get; set; }
        public List<VolunteersHistory> listofvolunteerHistory { get; set; }
        public List<OrgEventImage> detailsEventImageOne { get; set; }
        public List<OrgEventImage> detailsEventImage { get; set; }
        public List<OrgSkillRequirement> detailsSkillRequirement { get; set; }
        public List<UserAccount> userAccounts { get; set; }
        public List<VolunteerInfo> volunteersInfo { get; set; }
        public List<VolunteerSkill> volunteersSkills { get; set; }
        public List<VolunteerSkill> appliedVolunteersSkills { get; set; }
        public List<Skills> skills { get; set; }
        public List<ProfilePicture> picture { get; set; }
        public List<OrgInfo> orgInfos { get; set; }
        public List<OrgEvents> orgEvents { get; set; }
        public List<OrgEvents> pendingOrgDetails { get; set; }
        public List<Volunteers> volunteers { get; set; }
        public List<Volunteers> volunteersStatusEvent { get; set; }
        public UserDonated userDonated { get; set; }
        public List<UserDonated> listofUserDonated { get; set; }
        public List<Rating> listOfRatings { get; set; }
        public List<OrgSkillRequirement> skillRequirement1 { get; set; }
        public List<Volunteers> listOfEventVolunteers { get; set; }
        public List<OrgEvents> recentEvents { get; set; }
        public Dictionary<string, int> totalSkills { get; set; }
        public List<UserDonated> recentDonators { get; set; }
        public List<OrgEvents> orgEventHistory { get; set; }
        public List<GroupChat> groupChats { get; set; }
        public List<GroupMessages> groupMessages { get; set; }
        public List<OrgEvents> getAllOrgEvents { get; set; }
        public List<OrgInfo> getAllOrgAccounts { get; set; }
        public List<VolunteerInfo> getAllVolunteerAccounts { get; set; }
        public List<OrgEvents> getAllOrgEvent { get; set; } 
        public List<UserAccount> matchedSkills { get; set; }
        public List<UserAccount> volunteerAvail { get; set; }
        public List<ProfilePicture> profilePics { get; set; }
        public List<VolunteerRatingData> volunteerRatings { get; set; }

        public List<Rating> rating { get; set; }

        //Stored Procedure
        public List<sp_GetSkills_Result> uniqueSkill { get; set; }
        public List<sp_OtherEvent_Result> orgOtherEvent { get; set; }
        public List<sp_ListOfGc_Result> listOfGc { get; set; }
        public List<sp_matchSkill_Result> matchSkill { get; set; }
        public List<sp_VolunteerHistory_Result> volunteersHistories { get; set; }
        public List<sp_GetUserDonatedInformations_Result> userDonatedInformations { get; set; }

        //View
        public List<vw_ListOfEvent> listOfEvents { get; set; }
        public List<vw_ListOfEvent> listOfEventsOne {  get; set; }
        public List<vw_ListOfVolunteerToBeInvite> ListToBeInvite { get; set; }
        public List<vw_ListOfEvent> listOfEventsSection { get; set; }

        public decimal totalDonation { get; set; }
        public int totalVolunteer { get; set; }

        public Dictionary<int, int> eventSummary { get; set; }
        public Dictionary<int, int> allEventSummary { get; set; }
        public List<FilteredVolunteer> filteredVolunteers { get; set; }
        public List<RecruitmentResult> recruitmentResults { get; set; }


        public class VolunteerRatingData
        {
            public int VolunteerId { get; set; }
            public int Attendance { get; set; } // Process Attendance
            public string Feedback { get; set; } // Add Feedback field
            public List<SkillRating> SkillRatings { get; set; }
        }

        public class SkillRating
        {
            public int SkillId { get; set; }
            public String SkillName { get; set; }
            public int Rating { get; set; }
        }
    }
}