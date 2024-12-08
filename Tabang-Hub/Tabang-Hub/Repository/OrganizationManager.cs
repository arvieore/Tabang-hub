using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Tabang_Hub.Utils;
using static Tabang_Hub.Utils.Lists;

namespace Tabang_Hub.Repository
{
    public class OrganizationManager
    {
        private readonly TabangHubEntities db;

        private BaseRepository<OrgEvents> _orgEvents;
        private BaseRepository<OrgSkillRequirement> _orgSkillRequirements;
        private BaseRepository<OrgEventImage> _orgEventsImage;
        private BaseRepository<vw_ListOfEvent> _listOfEvents;
        private BaseRepository<OrgInfo> _orgInfo;
        private BaseRepository<ProfilePicture> _profilePic;
        private BaseRepository<UserDonated> _userDonated;
        private BaseRepository<Volunteers> _eventVolunteers;
        private BaseRepository<VolunteerSkill> _volunteerSkills;
        private BaseRepository<UserAccount> _userAccount;
        private BaseRepository<Skills> _skills;
        private BaseRepository<VolunteersHistory> _volunteersHistory;
        private BaseRepository<GroupChat> _groupChat;
        private BaseRepository<GroupMessages> _groupMessages;
        private BaseRepository<Rating> _ratings;
        private BaseRepository<vw_VolunteerSkills> _vwVollunterSkills;
        private BaseRepository<VolunteerInfo> _volunteerInfo;
        private BaseRepository<ProfilePicture> _profile;
        private BaseRepository<Notification> _notification;
        private BaseRepository<Feedback> _feedback;
        private BaseRepository<DonationEvent> _donationEvent;
        private BaseRepository<DonationImage> _donationImage;
        private BaseRepository<Donated> _donated;
        private BaseRepository<Donates> _donates;
        public OrganizationManager()
        {
            db = new TabangHubEntities();

            _orgEvents = new BaseRepository<OrgEvents>();
            _orgSkillRequirements = new BaseRepository<OrgSkillRequirement>();
            _orgEventsImage = new BaseRepository<OrgEventImage>();
            _listOfEvents = new BaseRepository<vw_ListOfEvent>();
            _orgInfo = new BaseRepository<OrgInfo>();
            _profilePic = new BaseRepository<ProfilePicture>();
            _userDonated = new BaseRepository<UserDonated>();
            _eventVolunteers = new BaseRepository<Volunteers>();
            _volunteerSkills = new BaseRepository<VolunteerSkill>();
            _userAccount = new BaseRepository<UserAccount>();
            _skills = new BaseRepository<Skills>();
            _volunteersHistory = new BaseRepository<VolunteersHistory>();
            _groupChat = new BaseRepository<GroupChat>();
            _groupMessages = new BaseRepository<GroupMessages>();
            _ratings = new BaseRepository<Rating>();
            _vwVollunterSkills = new BaseRepository<vw_VolunteerSkills>();
            _volunteerInfo = new BaseRepository<VolunteerInfo>();
            _profile = new BaseRepository<ProfilePicture>();
            _notification = new BaseRepository<Notification>();
            _feedback = new BaseRepository<Feedback>();
            _donationEvent = new BaseRepository<DonationEvent>();
            _donationImage = new BaseRepository<DonationImage>();
            _donated = new BaseRepository<Donated>();
            _donates = new BaseRepository<Donates>();
        }

        public ErrorCode CreateEvents(OrgEvents orgEvents, List<string> imageFileNames, List<SkillDto> skills, ref string errMsg)
        {
            // Create the event
            orgEvents.status = 1;
            if (_orgEvents.Create(orgEvents, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            // Get the eventId of the newly created event
            int eventId = orgEvents.eventId;
            int orgInfoId = db.OrgInfo.Where(m => m.userId == orgEvents.userId).Select(m => m.orgInfoId).FirstOrDefault();

            var gc = new GroupChat
            {
                eventId = eventId,
                orgInfoId = orgInfoId
            };
            if (_groupChat.Create(gc, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            // Add each skill associated with the eventId            
            foreach (var skill in skills)
            {
                var skillId = GetSkillIdBySkillName(skill.Name);
                var skillRequirement = new OrgSkillRequirement
                {
                    eventId = eventId,
                    skillId = skillId.skillId,
                };

                if (_orgSkillRequirements.Create(skillRequirement, out errMsg) != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }

            // Add each image associated with the eventId
            foreach (var fileName in imageFileNames)
            {
                var orgEventImage = new OrgEventImage
                {
                    eventId = eventId,
                    eventImage = fileName
                };

                if (_orgEventsImage.Create(orgEventImage, out errMsg) != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }
            return ErrorCode.Success;
        }
        public DonationEvent GetDonationEventByDonationEventId(int donationEventId)
        {
            return _donationEvent._table.Where(m => m.donationEventId == donationEventId).FirstOrDefault();
        }
        public List<DonationImage> GetDonationEventImageByDonationEventId(int donationEventId)
        {
            return _donationImage._table.Where(m => m.donationEventId == donationEventId).ToList();
        }
        //public List<Donated> ListOfDonatedByDonationEventId(int donationEventId)
        //{
        //    return _donated._table.Where(m => m.donationEventId == donationEventId).ToList();
        //}
        
        public List<Donates> GetDonatedByDonationEventId(int donatedId)
        {
            return _donates._table.Where(m => m.eventId == donatedId && m.eventType == 2).ToList();
        }
        public Donated GetDonateByDonateId(int donateId)
        {
            return _donated._table.Where(m => m.donateId == donateId).FirstOrDefault();
        }
        public Donates GetDonatesByDonateId(int donatesId)
        { 
            return _donates._table.Where(m => m.donatesId == donatesId).FirstOrDefault();
        }
        public ErrorCode MarkAsReceived(int donateId, ref string errMsg)
        {
            var donate = GetDonatesByDonateId(donateId);
            donate.status = 1;

            if (_donates.Update(donate.donatesId, donate, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }

        public ErrorCode EditOrgInfo(OrgInfo orgInformation, int id, ref string errMsg)
        {
            if (orgInformation == null)
            {
                errMsg = "Organization information is required.";
                return ErrorCode.Error;
            }
            var currentOrgInfo = GetOrgInfoByUserId(id);

            // Check if there are changes, otherwise keep the existing values
            currentOrgInfo.userId = id;
            currentOrgInfo.orgName = orgInformation.orgName ?? currentOrgInfo.orgName;
            currentOrgInfo.orgEmail = orgInformation.orgEmail ?? currentOrgInfo.orgEmail;
            currentOrgInfo.orgType = orgInformation.orgType ?? currentOrgInfo.orgType;
            currentOrgInfo.orgDescription = orgInformation.orgDescription ?? currentOrgInfo.orgDescription;
            currentOrgInfo.phoneNum = orgInformation.phoneNum ?? currentOrgInfo.phoneNum;
            currentOrgInfo.street = orgInformation.street ?? currentOrgInfo.street;
            currentOrgInfo.city = orgInformation.city ?? currentOrgInfo.city;
            currentOrgInfo.province = orgInformation.province ?? currentOrgInfo.province;
            currentOrgInfo.profilePath = orgInformation.profilePath ?? currentOrgInfo.profilePath;
            currentOrgInfo.coverPhoto = orgInformation.coverPhoto ?? currentOrgInfo.coverPhoto;

            // Proceed to update if there are changes
            if (_orgInfo.Update(currentOrgInfo.orgInfoId, currentOrgInfo, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
        public ErrorCode EditEvent(OrgEvents updatedEvent, List<SkillDto> skills, List<string> skillsToRemove, List<string> imageFileNames, int eventId, ref string errMsg)
        {
            var events = GetEventByEventId(updatedEvent.eventId);

            events.eventTitle = updatedEvent.eventTitle;
            events.eventDescription = updatedEvent.eventDescription;
            events.donationIsAllowed = updatedEvent.donationIsAllowed;
            events.maxVolunteer = updatedEvent.maxVolunteer;
            events.dateStart = updatedEvent.dateStart;
            events.dateEnd = updatedEvent.dateEnd;
            events.location = updatedEvent.location;
            events.status = events.status;

            // Remove unwanted skills
            var currentSkills = listOfSkillRequirement(events.eventId);

            // Add new skills if not already present
            foreach (var newSkill in skills)
            {
                if (!currentSkills.Any(s => s.Skills.skillName == newSkill.Name))
                {
                    var skillId = GetSkillIdBySkillName(newSkill.Name);
                    var newSkll = new OrgSkillRequirement()
                    {
                        eventId = events.eventId,
                        skillId = skillId.skillId,
                    };
                    if (_orgSkillRequirements.Create(newSkll, out errMsg) != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                }
            }

            
            foreach (var currentSkill in currentSkills)
            {
                if (skillsToRemove.Contains(currentSkill.Skills.skillName))
                {
                    if (_orgSkillRequirements.Delete(currentSkill.skillRequirementId) != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                }
            }

            foreach (var iamges in imageFileNames)
            {
                var imageToAdd = new OrgEventImage()
                { 
                    eventId = events.eventId,
                    eventImage = iamges
                };

                if (_orgEventsImage.Create(imageToAdd, out errMsg) != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }

            if (_orgEvents.Update(events.eventId, events, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
        public List<vw_ListOfEvent> ListOfEvents(int userId)
        {
            return _listOfEvents.GetAll()
                                .Where(m => m.User_Id == userId)
                                .OrderByDescending(m => m.Start_Date) // Replace `Event_Date` with the appropriate property for ordering
                                .ToList();
        }

        public List<vw_ListOfEvent> ListOfEvents1(int userId)
        {
            return _listOfEvents.GetAll()
                                .Where(m => m.User_Id == userId && m.status == 1)
                                .OrderByDescending(m => m.Start_Date) // Replace `Event_Date` with the appropriate property for ordering
                                .ToList();
        }

        public List<OrgEvents> ListOfEvents2(int userId)
        {
            return _orgEvents.GetAll()
                                .Where(m => m.userId == userId && m.status == 1)
                                .OrderByDescending(m => m.dateStart) // Replace `Event_Date` with the appropriate property for ordering
                                .ToList();
        }

        public decimal GetTotalDonationByUserId(int userId)
        {
            var events = ListOfEvents(userId);

            decimal? totalDonation = 0;

            foreach (var totalEvent in events)
            {
                var donations = GetTotalDonationByEventId(totalEvent.Event_Id);

                foreach (var donation in donations)
                {
                    totalDonation += donation.amount;
                }
            }

            return (decimal)totalDonation;
        }
        public int GetTotalVolunteerByUserId(int userId)
        {
            var events = ListOfEvents(userId);

            int totalVolunteer = 0;

            foreach (var totalEvent in events)
            {
                var volunteers = GetTotalVolunteerByEventId(totalEvent.Event_Id);

                totalVolunteer += volunteers.Count();

            }

            return totalVolunteer;
        }
        public Dictionary<int, int> GetEventsByUserId(int userId)
        {
            // Events from the main table
            var eventSummary = _orgEvents._table
                .Where(m => m.userId == userId && m.dateStart.HasValue && m.dateEnd.HasValue && m.status == 2)
                .GroupBy(m => m.dateStart.Value.Month)
                .ToDictionary(
                    group => group.Key,
                    group => group.Count()
                );

            return eventSummary;
        }

        public List<OrgEvents> GetRecentOngoingEventsByUserId(int userId)
        {
            var recentEvents = _orgEvents._table
                .Where(m => m.userId == userId && m.dateStart.HasValue && m.dateEnd.HasValue)
                // Events that have started but not yet ended
                .OrderByDescending(m => m.eventId) // Order by dateStart, most recent first
                .Take(5) // Get the top 5 most recent events
                .ToList();

            return recentEvents;
        }
        public List<UserDonated> GetRecentUserDonationsByUserId(int userId)
        {
            // Step 1: Get the list of events created by the organization
            var events = ListOfEvents(userId); // Assuming ListOfEvents(userId) returns a list of events for the user

            // Step 2: Extract the event IDs from the event list
            var eventIds = events.Select(e => e.Event_Id).ToList();

            // Step 3: Get donations related to those event IDs, sorted by the most recent donation date
            var recentDonations = _userDonated._table
                .Where(d => eventIds.Contains((int)d.eventId)) // This will work if eventIds is a list of integers
                .OrderByDescending(d => d.donatedAt) // Sort by donation date
                .Take(5) // Get the top 5 most recent donations
                .ToList();

            return recentDonations;
        }
        public Dictionary<string, int> GetAllVolunteerSkills(int userId)
        {
            // Step 1: Get the list of events created by the user (Organization or Event Creator)
            var events = ListOfEvents(userId); // Assuming ListOfEvents(userId) returns a list of events
            // Step 2: Initialize a dictionary to count the frequency of each skill by name
            Dictionary<string, int> skillFrequency = new Dictionary<string, int>();

            // Step 3: For each event, retrieve the volunteers and their associated skills
            foreach (var eventItem in events)
            {
                // Assuming you have a method GetVolunteersByEventId that retrieves all volunteers for an event
                var volunteers = GetVolunteersByEventId(eventItem.Event_Id);

                // For each volunteer, get their skills
                foreach (var volunteer in volunteers)
                {
                    // Assuming you have a method GetVolunteerSkillsByUserId to get the skills of a volunteer
                    var skills = GetVolunteerSkillsByUserId(volunteer.userId);

                    // Count the occurrence of each skill by its name
                    foreach (var skill in skills)
                    {
                        var req = listOfSkillRequirement(eventItem.Event_Id);

                        foreach (var require in req)
                        {
                            if (skill.skillId == require.skillId && volunteer.Status == 1)
                            {
                                if (skillFrequency.ContainsKey(skill.Skills.skillName)) // Assuming SkillName is a string representing the skill's name
                                {
                                    skillFrequency[skill.Skills.skillName]++; // Increment count if skill already exists
                                }
                                else
                                {
                                    skillFrequency[skill.Skills.skillName] = 1; // Initialize with count 1 if it doesn't exist
                                }
                            }
                        }
                    }
                }
            }

            // Step 4: Return the dictionary containing the skills and their counts
            return skillFrequency;
        }

        public List<Volunteers> GetVolunteersByEventId(int eventId)
        {
            return _eventVolunteers._table.Where(m => m.eventId == eventId).ToList();
        }
        public List<ProfilePicture> GetProfileByUserId(int userId)
        {
            return _profile._table.Where(m => m.userId == userId).ToList();
        }
        public ErrorCode SentNotif(int userId, int senderId, int relatedId, string type, string content, int broadcast, ref string errMsg)
        {
            var notif = new Notification()
            {
                userId = userId,
                senderUserId = senderId,
                relatedId = relatedId,
                type = type,
                content = content,
                broadcast = broadcast,
                status = 0,
                createdAt = DateTime.Now,

            };
            try
            {
                if (_notification.Create(notif, out errMsg) != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }
            catch (Exception ex)
            {
                errMsg = ex.Message;
            }

            return ErrorCode.Success;
        }
        public List<VolunteerSkill> GetVolunteerSkillByUserId(int userId)
        {
            return _volunteerSkills._table.Where(m => m.userId == userId).ToList();
        }
        public List<VolunteerSkill> GetVolunteerSkillsByUserId(int? userId)
        {
            return _volunteerSkills._table.Where(m => m.userId == userId).ToList();
        }
        public List<UserDonated> GetTotalDonationByEventId(int eventId)
        {
            return _userDonated._table.Where(m => m.eventId == eventId).ToList();
        }
        public Skills GetSkillIdBySkillName(string skillName)
        {
            return _skills._table.Where(m => m.skillName == skillName).FirstOrDefault();
        }
        public List<Volunteers> GetTotalVolunteerByEventId(int eventId)
        {
            return _eventVolunteers._table.Where(m => m.eventId == eventId).ToList();
        }
        public List<Volunteers> GetPendingVolunteersByEventId(int eventId)
        {
            return _eventVolunteers._table.Where(m => m.eventId == eventId && m.Status == 0).ToList();
        }
        public List<UserAccount> GetListOfVolunteer()
        {
            return _userAccount._table.Where(m => m.roleId == 1 && m.status == 1).ToList();
        }
        public List<VolunteersHistory> GetTotalVolunteerHistoryByEventId(int eventId)
        {
            return _volunteersHistory._table.Where(v => v.eventId == eventId).ToList();
        }
        public OrgInfo GetProfileByProfileId(int? id)
        {
            return _orgInfo._table.Where(m => m.userId == id).FirstOrDefault();
        }
        public List<Skills> ListOfSkills()
        {
            return _skills.GetAll().ToList();
        }
        public Volunteers GetVolunteerById(int id, int eventId)
        {
            return _eventVolunteers._table.Where(m => m.userId == id && m.eventId == eventId).FirstOrDefault();
        }
        public OrgInfo GetOrgInfoByUserId(int? id)
        {
            return _orgInfo._table.Where(m => m.userId == id).FirstOrDefault();
        }
        public OrgInfo GetOrgInfoByUserId(int id)
        {
            return _orgInfo._table.Where(m => m.userId == id).FirstOrDefault();
        }
        public OrgEvents GetEventById(int id)
        {
            return _orgEvents._table.Where(m => m.eventId == id).FirstOrDefault();
        }
        public OrgEvents GetEventsByEventId(int id)
        {
            return _orgEvents._table.Where(m => m.eventId == id).FirstOrDefault();
        }
        public OrgEventImage GetEventImageByEventId(int eventId)
        {
            return _orgEventsImage._table.Where(m => m.eventId == eventId).FirstOrDefault();
        }
        public List<OrgEvents> GetOrgEventsByUserId(int userId)
        {
            return _orgEvents._table.Where(m => m.userId == userId).ToList();
        }
        public List<OrgEventImage> listOfEventImage(int id)
        {
            return _orgEventsImage.GetAll().Where(m => m.eventId == id).ToList();
        }

        public List<OrgSkillRequirement> listOfSkillRequirement(int id)
        {
            return _orgSkillRequirements.GetAll().Where(m => m.eventId == id).ToList();
        }
        public List<UserDonated> ListOfUserDonated(int id)
        {
            return _userDonated.GetAll().Where(m => m.eventId == id).ToList();
        }
        public List<Volunteers> ListOfEventVolunteers(int eventId)
        {
            return _eventVolunteers.GetAll().Where(m => m.eventId == eventId).ToList();
        }
        public List<VolunteersHistory> ListOfEventVolunteersHistory(int eventId)
        {
            return _volunteersHistory.GetAll().Where(m => m.eventId == eventId).ToList();
        }
        public List<VolunteerSkill> ListOfEventVolunteerSkills()
        {
            return _volunteerSkills.GetAll();
        }
        public List<Rating> GetVolunteerRatingsByUserId(int userId)
        {
            return _ratings._table.Where(m => m.userId == userId).ToList();
        }
        public List<VolunteerSkill> GetListOfVolunteerSkillByUserId(int userId)
        {
            return _volunteerSkills.GetAll().Where(m => m.userId == userId).ToList();
        }
        public UserAccount GetUserByUserId(int userId)
        {
            return _userAccount._table.Where(m => m.userId == userId).FirstOrDefault();
        }
        public VolunteerInfo GetVolunteerInfoByUserId(int userId)
        {
            return _volunteerInfo._table.Where(m => m.userId == userId).FirstOrDefault();
        }
        public List<Donated> GetDonatedByDonatesId(int donatesId)
        {
            return _donated._table.Where(m => m.donatesId == donatesId).ToList();
        }
        public List<OrgEvents> GetEventHistoryByUserId(int userId)
        {
            return _orgEvents._table.Where(m => m.userId == userId && m.status == 2).ToList();
        }
        public List<VolunteersHistory> GetVolunteerHistoryByEventId(int eventId)
        {
            return _volunteersHistory._table.Where(m => m.eventId == eventId).ToList();
        }
        public Feedback GetFeedbackByEventIdAndUserId(int userId, int eventId)
        {
            return _feedback._table.Where(m => m.userId == userId && m.eventId == eventId).FirstOrDefault();
        }
        public ProfilePicture GetProfileByUserId1(int userId)
        {
            return _profile._table.Where(m => m.userId == userId).FirstOrDefault();
        }
        public List<Rating> GetEventVolunteerRatingByUserIdAndEventId(int userId, int eventId)
        {
            return _ratings._table.Where(m => m.userId == userId && m.eventId == eventId).ToList();
        }
        public GroupChat GetGroupChatByEventId(int eventId)
        {
            return _groupChat._table.Where(m => m.eventId == eventId).FirstOrDefault();
        }
        public List<GroupMessages> GetGroupMessagesByGroupChatId(int groupChatId)
        {
            return _groupMessages._table.Where(m => m.groupChatId == groupChatId).ToList();
        }
        public VolunteersHistory GetSkillIdByEventIdAndUserId(int eventId, int userId)
        {
            return _volunteersHistory._table.Where(m => m.userId == userId && m.eventId == eventId).FirstOrDefault();
        }
        public Volunteers GetVolunteerByUserIdAndEventId(int userId, int eventId)
        {
            return _eventVolunteers._table.Where(m => m.userId == userId && m.eventId == eventId).FirstOrDefault();
        }
        public OrgEvents GetEventByEventId(int eventId)
        {
            return _orgEvents._table.Where(m => m.eventId == eventId).FirstOrDefault();
        }
        public List<DonationEvent> GetListOfDonationEventByUserId(int userId)
        { 
            return _donationEvent._table.Where(m => m.userId == userId).ToList();
        }
        public ErrorCode DeleteEvent(int eventId, ref string errMsg)
        {
            var skillsRequirement = listOfSkillRequirement(eventId);
            var eventImage = listOfEventImage(eventId);
            var eventVolunteers = ListOfEventVolunteers(eventId);
            var userDonated = ListOfUserDonated(eventId);
            var groupChat = GetGroupChatByEventId(eventId);


            //if (groupChat != null)
            //{
            //    var groupMessage = GetGroupMessagesByGroupChatId(groupChat.groupChatId);

            //    foreach (var message in groupMessage)
            //    {
            //        if (_groupMessages.Delete(message.messageId) != ErrorCode.Success)
            //        {
            //            return ErrorCode.Error;
            //        }
            //    }

            //    if (_groupChat.Delete(groupChat.groupChatId) != ErrorCode.Success)
            //    {
            //        return ErrorCode.Error;
            //    }
            //}

            //if (skillsRequirement != null)
            //{
            //    foreach (var skillRequirement in skillsRequirement)
            //    {
            //        var result = _orgSkillRequirements.Delete(skillRequirement.skillRequirementId);
            //        if (result != ErrorCode.Success)
            //        {
            //            return ErrorCode.Error;
            //        }

            //        var deletedSkillReq = new DeletedOrgSkillRequirement()
            //        { 
            //            eventId = skillRequirement.eventId,
            //            skillId = skillRequirement.skillId,
            //            totalNeeded = skillRequirement.totalNeeded,
            //        };

            //        if (_deletedOrgSkillReq.Create(deletedSkillReq, out errMsg) != ErrorCode.Success)
            //        {
            //            return ErrorCode.Error;
            //        }
            //    }
            //}

            //if (eventImage != null)
            //{
            //    foreach (var eventImages in eventImage)
            //    {
            //        var result = _orgEventsImage.Delete(eventImages.eventImageId);
            //        if (result != ErrorCode.Success)
            //        {
            //            return ErrorCode.Error;
            //        }

            //        var deletedEventImage = new DeletedEventImage()
            //        { 
            //            eventId= eventImages.eventId,
            //            eventImage = eventImages.eventImage,
            //        };

            //        if (_deletedEventImage.Create(deletedEventImage, out errMsg) != ErrorCode.Success)
            //        {
            //            return ErrorCode.Error;
            //        }
            //    }
            //}

            //if (userDonated != null)
            //{
            //    foreach (var donated in userDonated)
            //    {
            //        var deletedUserDonated = new DeletedUserDonated()
            //        {
            //            referenceNum = donated.referenceNum,
            //            eventId = donated.eventId,
            //            userId = donated.userId,
            //            amount = donated.amount,
            //            Status = donated.Status,
            //            donatedAt = donated.donatedAt,
            //        };

            //        if (_deletedUserDonated.Create(deletedUserDonated, out errMsg) != ErrorCode.Success)
            //        {
            //            return ErrorCode.Error;
            //        }
            //    }
            //}

            //if (eventVolunteers != null)
            //{
            //    foreach (var eventVolunteer in eventVolunteers)
            //    {
            //        var result = _eventVolunteers.Delete(eventVolunteer.applyVolunteerId);
            //        if (result != ErrorCode.Success)
            //        {
            //            return ErrorCode.Error;
            //        }
            //    }
            //}

            var evnt = GetEventByEventId(eventId);

            if (evnt.dateStart >= DateTime.Now && evnt.dateEnd <= DateTime.Now)
            {
                errMsg = "Event is ongoing";
                return ErrorCode.Error;
            }

            //if (eventImage != null)
            //{
            //    var deletedEvent = new DeletedEvent()
            //    {
            //        userId = evnt.userId,
            //        eventTitle = evnt.eventTitle,
            //        eventDescription = evnt.eventDescription,
            //        targetAmount = evnt.targetAmount,
            //        maxVolunteer = evnt.maxVolunteer,
            //        dateStart = evnt.dateStart,
            //        dateEnd = evnt.dateEnd,
            //        location = evnt.location,
            //        eventImage = evnt.eventImage,
            //    };

            //    if (_deletedEvent.Create(deletedEvent, out errMsg) != ErrorCode.Success)
            //    {
            //        return ErrorCode.Error;
            //    }
            //}

            evnt.status = 3;
            //var deleteEventResult = _orgEvents.Delete(eventId);
            //if (deleteEventResult != ErrorCode.Success)
            //{
            //    return ErrorCode.Error;
            //}

            if (_orgEvents.Update(evnt.eventId, evnt, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
        public ErrorCode DeleteDonation(int eventId, ref string errMsg)
        { 
            var eventDoination = GetDonationEventByDonationEventId(eventId);

            eventDoination.status = 3;

            if (_donationEvent.Update(eventDoination.donationEventId, eventDoination, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            return ErrorCode.Success;
        }
        public ErrorCode ConfirmApplicants(int id, int eventId, ref string errMsg)
        {
            var user = GetVolunteerById(id, eventId);

            user.Status = 1;

            if (_eventVolunteers.Update(user.applyVolunteerId, user, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
        public ErrorCode DeclineApplicant(int id, int eventId)
        {
            var user = GetVolunteerById(id, eventId);


            if (_eventVolunteers.Delete(user.applyVolunteerId) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }

        public async Task<RecruitmentResult> GetMatchedVolunteers(int eventId)
        {
            string flaskApiUrl = "https://tabangapi.as.r.appspot.com/recruitSortVolunteer"; // Flask API URL
            //string flaskApiUrl = "http://127.0.0.1:5000/recruitSortVolunteer"; // Flask API URL

            RecruitmentResult recruitmentResult = new RecruitmentResult();
            string errorMessage = null;

            // Get the target event's date range
            var targetEvent = _orgEvents.GetAll().FirstOrDefault(m => m.eventId == eventId);
            if (targetEvent == null)
            {
                throw new Exception("Event not found");
            }
            DateTime targetStartDate = targetEvent.dateStart ?? DateTime.MinValue;
            DateTime targetEndDate = targetEvent.dateEnd ?? DateTime.MaxValue;

            // Get volunteers who do not have conflicting events
            var availableVolunteers = _vwVollunterSkills.GetAll()
                .Where(vol => !db.Volunteers.ToList().Any(e =>
                    e.userId == vol.userId &&
                    (e.Status == 0 || e.Status == 1) && // Check for pending or ongoing events
                    _orgEvents.GetAll().Any(ev =>
                        ev.eventId == e.eventId &&
                        (ev.dateStart <= targetEndDate && ev.dateEnd >= targetStartDate) // Exclude if date overlap
                    )
                ))
                .ToList();

            // Prepare the data to pass to Flask
            var datas = new
            {
                user_info = _volunteerInfo.GetAll().Select(m => new { userId = m.userId, availability = m.availability }).ToList(),
                // Retrieve only relevant user skills for volunteers
                user_skills = availableVolunteers.Select(m => new { userId = m.userId, skillId = m.skillId, rating = m.rate }).ToList(),

                // Pass only the specific event's data
                event_data = _orgEvents.GetAll().Where(m => m.eventId == eventId).Select(m => new { eventId = m.eventId, eventDescription = m.eventDescription }).ToList(),

                // Only pass skills required for the specific event
                event_skills = db.OrgSkillRequirement.Where(es => es.eventId == eventId).Select(es => new { eventId = es.eventId, skillId = es.skillId }).ToList()
            };

            using (var client = new HttpClient())
            {
                try
                {
                    // Step 1: Send POST request to Flask API with the requestData
                    var response = await client.PostAsJsonAsync(flaskApiUrl, datas);

                    if (response.IsSuccessStatusCode)
                    {
                        // Step 2: Deserialize Flask API response to a list of recommended volunteers
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        recruitmentResult = JsonConvert.DeserializeObject<RecruitmentResult>(jsonResponse);
                    }
                    else
                    {
                        errorMessage = "Error calling the Python API: " + response.ReasonPhrase;
                    }
                }
                catch (Exception ex)
                {
                    errorMessage = "An error occurred: " + ex.Message;
                }
            }

            return recruitmentResult; // Return the list of recommended volunteers and any error message
        }
        public async Task<List<VolunteerInvite>> ConvertFeedbackToSentiment(int eventId)
        {
            //string flaskApiUrl = "http://127.0.0.1:5000/classify_users_feedback";
            string flaskApiUrl = "https://tabangapi.as.r.appspot.com/classify_users_feedback";

            var datas = new
            {
                user_info = (from volunteer in _volunteerInfo.GetAll()
                             join rating in db.vw_ListOfVolunteerToBeInvite
                             on volunteer.userId equals rating.userId
                             select new
                             {
                                 userId = volunteer.userId,
                                 fname = volunteer.fName,
                                 lname = volunteer.lName,
                                 overallRating = rating.OverallRating ?? 0,
                                 feedback = rating.Feedback,
                                 availability = volunteer.availability
                             }).ToList(),

                // Collect volunteer skills
                user_skills = db.VolunteerSkill
                    .Select(m => new { userId = m.userId, skillId = m.skillId })
                    .ToList(),

                // Pass event data (e.g., ID and description)
                event_data = _orgEvents.GetAll()
                    .Where(m => m.eventId == eventId)
                    .Select(m => new { eventId = m.eventId, eventDescription = m.eventDescription })
                    .ToList(),

                event_skills = db.OrgSkillRequirement.Where(es => es.eventId == eventId).Select(es => new { eventId = es.eventId, skillId = es.skillId }).ToList()
            };

            using (var client = new HttpClient())
            {
                try
                {
                    Console.WriteLine("Data being sent to Flask API: " + JsonConvert.SerializeObject(datas));
                    var response = await client.PostAsJsonAsync(flaskApiUrl, datas);

                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception("Error calling Flask API: " + response.ReasonPhrase);
                    }

                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Response from Flask API: " + jsonResponse);

                    // Deserialize the API response
                    var apiResponse = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                    if (apiResponse.status == "success")
                    {
                        var classifiedFeedback = JsonConvert.DeserializeObject<List<VolunteerInvite>>(
                            apiResponse.classified_feedback.ToString()
                        );

                        foreach (var invite in classifiedFeedback)
                        {
                            Console.WriteLine($"UserId: {invite.UserId}, FullName: {invite.FullName}, Rating: {invite.OverallRating}, Sentiment: {invite.Feedback}");
                        }

                        return classifiedFeedback;
                    }
                    else
                    {
                        throw new Exception("Flask API returned an error: " + apiResponse.message);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error during sentiment conversion: " + ex.Message);
                    throw;
                }
            }
        }



        public async Task<List<VolunteerInvite>> GetVolunteerFilteredSkill(int eventId, List<int> skillIds)
        {
            // Prepare the data to pass to Flask
            var datas = new
            {
                // Collect volunteer information with OverallRating
                user_info = (from volunteer in _volunteerInfo.GetAll()
                             join rating in db.vw_ListOfVolunteerToBeInvite
                             on volunteer.userId equals rating.userId
                             select new
                             {
                                 userId = volunteer.userId,
                                 fname = volunteer.fName,
                                 lname = volunteer.lName,
                                 overallRating = rating.OverallRating,
                                 feedback = rating.Feedback,
                                 availability = volunteer.availability
                             }).ToList(),

                // Collect volunteer skills
                user_skills = db.VolunteerSkill
                    .Select(m => new { userId = m.userId, skillId = m.skillId })
                    .ToList(),

                // Pass event data (e.g., ID and description)
                event_data = _orgEvents.GetAll()
                    .Where(m => m.eventId == eventId)
                    .Select(m => new { eventId = m.eventId, eventDescription = m.eventDescription })
                    .ToList(),

                // Selected skills for the event
                event_skills = skillIds.Select(skillId => new { eventId = eventId, skillId = skillId }).ToList()
            };
            string flaskApiUrl = "https://tabangapi.as.r.appspot.com/recruit"; // Flask API URL
            //string flaskApiUrl = "http://127.0.0.1:5000/recruit"; // Flask API URL

            using (var client = new HttpClient())
            {
                // Send the POST request to Flask with the data
                var response = await client.PostAsJsonAsync(flaskApiUrl, datas);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error calling the Python API: " + response.ReasonPhrase);
                }

                // Read and deserialize the response into the VolunteerInfo class
                var jsonResponse = await response.Content.ReadAsStringAsync();

                return JsonConvert.DeserializeObject<List<VolunteerInvite>>(jsonResponse);
            }
        }
        public async Task<List<VolunteerInvite>> GetVolunteersByRating(int eventId)
        {
            string flaskApiUrl = "https://tabangapi.as.r.appspot.com/filter_rate";
            //string flaskApiUrl = "http://127.0.0.1:5000/filter_rate";

            var datas = new
            {
                user_info = (from volunteer in _volunteerInfo.GetAll()
                             join rating in db.vw_ListOfVolunteerToBeInvite
                             on volunteer.userId equals rating.userId
                             select new
                             {
                                 userId = volunteer.userId,
                                 fname = volunteer.fName,
                                 lname = volunteer.lName,
                                 overallRating = rating.OverallRating,
                                 feedback = rating.Feedback,
                                 availability = volunteer.availability
                             }).ToList(),
                user_skills = db.VolunteerSkill.Select(m => new { userId = m.userId, skillId = m.skillId }).ToList(),

                // Pass event data (e.g., ID and description)
                event_data = _orgEvents.GetAll()
                    .Where(m => m.eventId == eventId)
                    .Select(m => new { eventId = m.eventId, eventDescription = m.eventDescription })
                    .ToList(),

                event_skills = db.OrgSkillRequirement.Where(es => es.eventId == eventId).Select(es => new { eventId = es.eventId, skillId = es.skillId }).ToList()
            };

            using (var client = new HttpClient())
            {
                var response = await client.PostAsJsonAsync(flaskApiUrl, datas);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error calling Flask API: " + response.ReasonPhrase);
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserialize the outer object
                var responseObject = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                // Extract the volunteers array
                var volunteersJson = responseObject.volunteers.ToString();

                // Deserialize volunteers into the expected type
                return JsonConvert.DeserializeObject<List<VolunteerInvite>>(volunteersJson);
            }
        }

        public async Task<List<VolunteerInvite>> GetVolunteersBySkillsAndRatings(List<int> skillIds, int eventId)
        {
            string flaskApiUrl = "https://tabangapi.as.r.appspot.com/filter_skills_and_ratings";
            //string flaskApiUrl = "http://127.0.0.1:5000/filter_skills_and_ratings";

            var datas = new
            {
                user_info = (from volunteer in _volunteerInfo.GetAll()
                             join rating in db.vw_ListOfVolunteerToBeInvite
                             on volunteer.userId equals rating.userId
                             select new
                             {
                                 userId = volunteer.userId,
                                 fname = volunteer.fName,
                                 lname = volunteer.lName,
                                 overallRating = rating.OverallRating,
                                 feedback = rating.Feedback,
                                 availability = volunteer.availability
                             }).ToList(),
                user_skills = db.VolunteerSkill.Select(m => new { userId = m.userId, skillId = m.skillId }).ToList(),
                event_skills = skillIds
            };

            using (var client = new HttpClient())
            {
                var response = await client.PostAsJsonAsync(flaskApiUrl, datas);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error calling Flask API: " + response.ReasonPhrase);
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<List<VolunteerInvite>>(jsonResponse);
            }
        }

        public async Task<List<VolunteerInvite>> GetFilteredVolunteerBasedOnAvailability(List<int> skillIds, int eventId, string availability)
        {
            string flaskApiUrl = "https://tabangapi.as.r.appspot.com/filter_by_availability";
            //string flaskApiUrl = "http://127.0.0.1:5000/filter_by_availability";

            var datas = new
            {
                user_info = (from volunteer in _volunteerInfo.GetAll()
                             join rating in db.vw_ListOfVolunteerToBeInvite
                             on volunteer.userId equals rating.userId
                             select new
                             {
                                 userId = volunteer.userId,
                                 fname = volunteer.fName,
                                 lname = volunteer.lName,
                                 overallRating = rating.OverallRating,
                                 feedback = rating.Feedback,
                                 availability = volunteer.availability
                             }).ToList(),
                user_skills = db.VolunteerSkill.Select(m => new { userId = m.userId, skillId = m.skillId }).ToList(),
                event_skills_selected = skillIds ?? new List<int>(), // Default to empty list if null

                event_data = _orgEvents.GetAll()
                    .Where(m => m.eventId == eventId)
                    .Select(m => new { eventId = m.eventId, eventDescription = m.eventDescription })
                    .ToList(),

                event_skills_required = db.OrgSkillRequirement.Where(es => es.eventId == eventId).Select(es => new { eventId = es.eventId, skillId = es.skillId }).ToList(),

                sortBy = availability
            };

            using (var client = new HttpClient())
            {
                var response = await client.PostAsJsonAsync(flaskApiUrl, datas);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error calling Flask API: " + response.ReasonPhrase);
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserialize the outer object
                var responseObject = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                // Extract the volunteers array
                var volunteersJson = responseObject.volunteers.ToString();

                // Deserialize volunteers into the expected type
                return JsonConvert.DeserializeObject<List<VolunteerInvite>>(volunteersJson);
            }
        }



        public async Task<List<VolunteerInvite>> GetFilteredByAvailabilityWithSkill(List<int> skillIds, int eventId, string availability)
        {
            string flaskApiUrl = "https://tabangapi.as.r.appspot.com/filter_by_availability_with_skills";
            //string flaskApiUrl = "http://127.0.0.1:5000/filter_by_availability_with_skills";

            var datas = new
            {
                user_info = (from volunteer in _volunteerInfo.GetAll()
                             join rating in db.vw_ListOfVolunteerToBeInvite
                             on volunteer.userId equals rating.userId
                             select new
                             {
                                 userId = volunteer.userId,
                                 fname = volunteer.fName,
                                 lname = volunteer.lName,
                                 overallRating = rating.OverallRating,
                                 feedback = rating.Feedback,
                                 availability = volunteer.availability
                             }).ToList(),
                user_skills = db.VolunteerSkill.Select(m => new { userId = m.userId, skillId = m.skillId }).ToList(),
                event_skills = skillIds ?? new List<int>(), // Default to empty list if null
                event_data = _orgEvents.GetAll()
                    .Where(m => m.eventId == eventId)
                    .Select(m => new { eventId = m.eventId, eventDescription = m.eventDescription })
                    .ToList(),

                event_skills_required = db.OrgSkillRequirement.Where(es => es.eventId == eventId).Select(es => new { eventId = es.eventId, skillId = es.skillId }).ToList(),

                sortBy = availability
            };

            using (var client = new HttpClient())
            {
                var response = await client.PostAsJsonAsync(flaskApiUrl, datas);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error calling Flask API: " + response.ReasonPhrase);
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserialize the outer object
                var responseObject = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                // Extract the volunteers array
                var volunteersJson = responseObject.volunteers.ToString();

                // Deserialize volunteers into the expected type
                return JsonConvert.DeserializeObject<List<VolunteerInvite>>(volunteersJson);
            }
        }


        public async Task<List<VolunteerInvite>> GetFilterByRatingsWithAvailability(List<int> skillIds, int eventId, string availability)
        {
            string flaskApiUrl = "https://tabangapi.as.r.appspot.com/filter_by_availability_with_skills";
            //string flaskApiUrl = "http://127.0.0.1:5000/filter_by_ratings_with_availability";

            var datas = new
            {
                user_info = (from volunteer in _volunteerInfo.GetAll()
                             join rating in db.vw_ListOfVolunteerToBeInvite
                             on volunteer.userId equals rating.userId
                             select new
                             {
                                 userId = volunteer.userId,
                                 fname = volunteer.fName,
                                 lname = volunteer.lName,
                                 overallRating = rating.OverallRating,
                                 feedback = rating.Feedback,
                                 availability = volunteer.availability
                             }).ToList(),
                user_skills = db.VolunteerSkill
                .Select(m => new { userId = m.userId, skillId = m.skillId })
                .ToList(),
                event_skills_required = db.OrgSkillRequirement
                .Where(es => es.eventId == eventId)
                .Select(es => new { eventId = es.eventId, skillId = es.skillId })
                .ToList(),
                sortBy = availability
            };


            using (var client = new HttpClient())
            {
                var response = await client.PostAsJsonAsync(flaskApiUrl, datas);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error calling Flask API: " + response.ReasonPhrase);
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserialize the response into a list of VolunteerInvite objects
                var responseObject = JsonConvert.DeserializeObject<dynamic>(jsonResponse);
                var volunteersJson = responseObject.volunteers.ToString();
                return JsonConvert.DeserializeObject<List<VolunteerInvite>>(volunteersJson);
            }
        }


        public async Task<List<VolunteerInvite>> GetFilterByRateWithAvailabilityAndSkills(List<int> skillIds, int eventId, string availability)
        {
            string flaskApiUrl = "https://tabangapi.as.r.appspot.com/filter_by_ratings_with_availability";
            //string flaskApiUrl = "http://127.0.0.1:5000/filter_by_rate_availability_skills";

            var datas = new
            {
                user_info = (from volunteer in _volunteerInfo.GetAll()
                             join rating in db.vw_ListOfVolunteerToBeInvite
                             on volunteer.userId equals rating.userId
                             select new
                             {
                                 userId = volunteer.userId,
                                 fname = volunteer.fName,
                                 lname = volunteer.lName,
                                 overallRating = rating.OverallRating,
                                 feedback = rating.Feedback,
                                 availability = volunteer.availability
                             }).ToList(),
                user_skills = db.VolunteerSkill.Select(m => new { userId = m.userId, skillId = m.skillId }).ToList(),
                event_skills = skillIds ?? new List<int>(), // Default to empty list if null
                event_skills_required = db.OrgSkillRequirement
                    .Where(es => es.eventId == eventId)
                    .Select(es => new { eventId = es.eventId, skillId = es.skillId })
                    .ToList(),
                sortBy = availability
            };

            using (var client = new HttpClient())
            {
                var response = await client.PostAsJsonAsync(flaskApiUrl, datas);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error calling Flask API: " + response.ReasonPhrase);
                }

                var jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserialize the outer object
                var responseObject = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

                // Extract the volunteers array
                var volunteersJson = responseObject.volunteers.ToString();

                // Deserialize volunteers into the expected type
                return JsonConvert.DeserializeObject<List<VolunteerInvite>>(volunteersJson);
            }
        }


        public ErrorCode TrasferToHisotry1(int eventId, List<VolunteerRatingData> volunteerRatings, ref string errMsg)
        {
            var orgEvent = GetEventByEventId(eventId);
            var volunteers = ListOfEventVolunteers(eventId);

            foreach (var volunteer in volunteers)
            {
                foreach (var rate in volunteerRatings)
                {
                    if (volunteer.userId == rate.VolunteerId)
                    {
                        var volunteersHistory = new VolunteersHistory()
                        {

                            userId = volunteer?.userId,
                            eventId = volunteer.eventId,
                            appliedAt = volunteer?.appliedAt,
                            attended = rate.Attendance,
                        };

                        if (_volunteersHistory.Create(volunteersHistory, out errMsg) != ErrorCode.Success)
                        {
                            return ErrorCode.Error;
                        }
                    }
                }
               
            }       

            orgEvent.status = 2;

            if (_orgEvents.Update(orgEvent.eventId, orgEvent, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            return ErrorCode.Success;
        }
        public ErrorCode SaveRating(int eventId, int attended, string feedback, int userId, int skillId, int rating, ref string errMsg)
        {
            var skillId1 = GetSkillIdByEventIdAndUserId(eventId, userId);
            skillId1.attended = attended;
            var ratings = new Rating()
            {
                eventId = eventId,
                userId = userId,
                rate = rating,
                skillId = skillId,             
                ratedAt = DateTime.Now,
            };

            var feedbck = new Feedback()
            { 
                userId = userId,
                eventId = eventId,
                feedback1 = feedback,
            };

            if (_volunteersHistory.Update(skillId1.applyVolunteerId, skillId1, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            if (_ratings.Create(ratings, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            if (_feedback.Create(feedbck, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
        public ErrorCode RemoveVolunteer(int userId, int eventId, ref string errMsg)
        {
            var volunteer = GetVolunteerByUserIdAndEventId(userId, eventId);

            if (_eventVolunteers.Delete(volunteer.applyVolunteerId) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            return ErrorCode.Success;
        }
        public ErrorCode InviteVolunteer(int userId, int eventId, ref string errMsg)
        {
            var volunteer = new Volunteers()
            {
                userId = userId,
                eventId = eventId,
                Status = 2,
            };

            if (_eventVolunteers.Create(volunteer, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
        public View_VolunteerInfo GetUserInfo(int userId, int eventId)
        {
            try
            {
                // Retrieve volunteer data
                var volunteer = _volunteerInfo.GetAll()
                    .Where(m => m.userId == userId)
                    .Select(m => new
                    {
                        FullName = $"{m.fName} {m.lName}",
                        BirthDay = m.bDay?.ToString("yyyy-MM-dd"),
                        PhoneNum = m.phoneNum,
                        Street = m.street,
                        City = m.city,
                        Province = m.province,
                        Zip = m.zipCode,
                        Availability = m.availability,
                        About = m.aboutMe
                    })
                    .FirstOrDefault();

                if (volunteer == null)
                    return null;

                var volProfile = _profilePic.GetAll()
                    .Where(m => m.userId == userId)
                    .Select(m => m.profilePath)
                    .FirstOrDefault();

                var volSkill = _volunteerSkills.GetAll()
                    .Where(m => m.userId == userId)
                    .Select(m => m.skillId)
                    .ToList();

                var getMatchSkill = _orgSkillRequirements.GetAll()
                    .Where(m => volSkill.Contains(m.skillId) && m.eventId == eventId)
                    .Select(m => m.skillId)
                    .ToList();

                var skills = _skills.GetAll()
                    .Where(s => getMatchSkill.Contains(s.skillId))
                    .Select(s => s.skillName)
                    .ToList();

                double totalRate = 0.0;
                int rateCount = 0;

                foreach (var skillId in volSkill)
                {
                    var getRate = _ratings.GetAll()
                        .Where(r => r.skillId == skillId && r.userId == userId)
                        .Select(r => r.rate);

                    totalRate += getRate.Sum(rate => rate ?? 0);
                    rateCount += getRate.Count();
                }

                double averageRate = rateCount > 0 ? totalRate / rateCount : 0.0;

                return new View_VolunteerInfo
                {
                    FullName = volunteer.FullName,
                    BirthDay = volunteer.BirthDay,
                    PhoneNum = volunteer.PhoneNum,
                    Street = volunteer.Street,
                    City = volunteer.City,
                    Province = volunteer.Province,
                    Zip = volunteer.Zip,
                    Availability = volunteer.Availability,
                    About = volunteer.About,
                    ProfilePic = volProfile,
                    Skills = skills,
                    AverageRate = averageRate
                };
            }
            catch
            {
                return null; // Handle exceptions gracefully
            }
        }

        public ErrorCode CreateDonation(DonationEvent donation, List<String> images, ref string errMsg) 
        {
            donation.status = 1;

            if (donation != null)
            {
                if (images != null)
                {
                    if (_donationEvent.Create(donation, out errMsg) != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }

                    foreach (var image in images)
                    {
                        var dntnImage = new DonationImage
                        {
                            donationEventId = donation.donationEventId,
                            imagePath = image,
                        };

                        if (_donationImage.Create(dntnImage, out errMsg) != ErrorCode.Success)
                        {
                            return ErrorCode.Error;
                        }
                    };
                }
                else
                {
                    errMsg = "Please Select Image!";
                }
            }
            else 
            {
                errMsg = "Please fill in the form!";
            }

            return ErrorCode.Success;
        }
    }
}