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
        public DonationEvent DonationEvent { get; set; }

        //Tables List
        //public List<UserAccount> userAccounts { get; set; }
        //public List<VolunteerInfo> volunteersInfo { get; set; }
        //public List<VolunteerSkill> volunteersSkill { get; set; }
        //public List<Skills> skills { get; set; }
        //public List<ProfilePicture> picture { get; set; }
        public List<OrgEvents> listOfOrgEvents { get; set; }
        public List<vw_VolunteerAccounts> volunteerAccounts { get; set; }
        public List<DonationImage> DonationImages { get; set; }
        public List<OrgInfo> recentOrgAcc {  get; set; }
        public List<Skills> allSkill { get; set; }
        public List<UserAccount> pendingOrg {  get; set; }
        public List<Skills> listOfSkills { get; set; }
        public List<vw_OrganizationAccounts> organizationAccounts { get; set; }
        public List<VolunteersHistory> listofvolunteerHistory { get; set; }
        public List<OrgEventImage> detailsEventImageOne { get; set; }
        public List<OrgEventImage> detailsEventImage { get; set; }
        public List<OrgSkillRequirement> detailsSkillRequirement { get; set; }
        public List<Donated> MyDonations { get; set; }
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
        public List<DonationEvent> listOfDonationEvent { get; set; }
        public List<Donated> listOfDonated { get; set; }
        public List<Donates> listOfDonates{ get; set; }
        public Donates donates { get; set; }

        public List<Rating> rating { get; set; }

        //Stored Procedure
        public List<sp_GetSkills_Result> uniqueSkill { get; set; }
        public List<sp_OtherEvent_Result> orgOtherEvent { get; set; }
        public List<sp_ListOfGc_Result> listOfGc { get; set; }
        public List<sp_matchSkill_Result> matchSkill { get; set; }
        public List<sp_checkMatchByUserId_Result> matchSkillByUserId { get; set; }
        public List<sp_VolunteerHistory_Result> volunteersHistories { get; set; }
        public List<sp_GetUserDonatedInformations_Result> userDonatedInformations { get; set; }
        public List<sp_GetUserDonated_Result> sp_GetUserDonated { get; set; }
        public List<sp_UserListEvent_Result> sp_userListEvent { get; set; }

        //View
        public List<vw_ListOfEvent> listOfEvents { get; set; }
        public List<vw_ListOfEvent> listOfEventsOne {  get; set; }
        public List<vw_ListOfVolunteerToBeInvite> ListToBeInvite { get; set; }
        public List<vw_ListOfEvent> listOfEventsSection { get; set; }

        public decimal totalDonation { get; set; }
        public int totalVolunteer { get; set; }

        public Dictionary<int, int> eventSummary { get; set; }
        public Dictionary<int, int> donationSummary { get; set; }
        public Dictionary<int, int> allEventSummary { get; set; }
        public List<FilteredVolunteer> filteredVolunteers { get; set; }
        public List<RecruitmentResult> recruitmentResults { get; set; }

        public List<Event> events { get; set; }
        public List<Event> ongoingEvents {  get; set; }
        public List<Event> pendingEvents { get; set; }
        public List<Event> eventHistory { get; set; }


        public List<Donated> donateds { get; set; }
        public List<DonationImage> donationImages { get; set; }
        public List<DonationEvent> donationEvents { get; set; }
        public DonationEvent donationEvent { get; set; }
        public List<DonationType> donationTypes { get; set; }


        public class VolunteerRatingData
        {
            public int VolunteerId { get; set; }
            public int Attendance { get; set; } // Process Attendance
            public string Feedback { get; set; }
            public List<SkillRating> SkillRatings { get; set; }
        }

        public class SkillRating
        {
            public int SkillId { get; set; }
            public String SkillName { get; set; }
            public int Rating { get; set; }
        }

        public List<TopVolunteer> topVolunteers { get; set; }

        public class TopVolunteer
        {
            public int VolunteerId { get; set; }
            public List<int> EventIds { get; set; } = new List<int>(); // Store multiple event IDs
            public List<string> EventImages { get; set; } = new List<string>(); // Store multiple event images
            public string Name { get; set; }
            public int TotalEventsParticipated { get; set; }
        }

        public List<TopDonators> topDonators { get; set; }

        public class TopDonators
        {
            public int donatorsId { get; set; }
            public string Name { get; set; }
            public decimal totalAmountDonated { get; set; }
            public List<EventDonation> EventDonations { get; set; } = new List<EventDonation>(); // Store donations for each event
        }

        public class EventDonation
        {
            public int EventId { get; set; }
            public string EventName { get; set; }
            public string EventImage { get; set; }
            public decimal AmountDonated { get; set; }
        }

        public List<Donation> ListOfDonationEvents { get; set; }

        public class Donation
        { 
            public List<DonationEvent> Event { get; set; }
            public List<DonationImage> Image { get; set; }
        }

        public List<Donators> donators { get; set; }
        public class Donators
        { 
            public int donatesId { get; set; }
            public int userId { get; set; }
            public string referenceNum { get; set; }
            public int donationEventId { get; set; }
            public string donorName { get; set; }
            public int donationQuantity { get; set; }
            public int status { get; set; }
        }

        public class SkillDto
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public List<DonationHistory> listOfDonationsHisotry { get; set; }
        public class DonationHistory
        { 
            public Donates donates { get; set; }
            public DonationEvent donationEvent { get; set; }
            public OrgEvents orgEvents { get; set; }
        }
    }
}