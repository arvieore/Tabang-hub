using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Tabang_Hub.Contracts;
using Tabang_Hub.Controllers;
using Tabang_Hub.Utils;


namespace Tabang_Hub.Repository
{
    public class VolunteerManager
    {
        public BaseRepository<Skills> _skills;
        public BaseRepository<UserDonated> _userDonated;
        public BaseRepository<Volunteers> _volunteers;
        public BaseRepository<VolunteersHistory> _volunteersHistory;
        public BaseRepository<OrgEvents> _orgEvents;
        public BaseRepository<Notification> _notification;
        public BaseRepository<VolunteerInfo> _volunteerInfo;
        public BaseRepository<Donated> _donated;
        public BaseRepository<Donates> _donates;

        public BaseRepository<vw_ListOfEvent> _vw_listOfEvent;

        public BaseRepository<sp_VolunteerHistory_Result> _sp_VolunteerHistory;

        public TabangHubEntities db = new TabangHubEntities();
        public OrganizationManager OrganizationManager;
        public VolunteerManager() 
        {
            _skills = new BaseRepository<Skills>();
            _userDonated = new BaseRepository<UserDonated>();
            _volunteers = new BaseRepository<Volunteers>();
            _orgEvents = new BaseRepository<OrgEvents>();
            _notification = new BaseRepository<Notification>();
            _volunteerInfo = new BaseRepository<VolunteerInfo>();
            _donated = new BaseRepository<Donated>();
            _donates = new BaseRepository<Donates>();

            _vw_listOfEvent = new BaseRepository<vw_ListOfEvent>();

            _sp_VolunteerHistory = new BaseRepository<sp_VolunteerHistory_Result>();

            OrganizationManager = new OrganizationManager();
            db = new TabangHubEntities();
        }

        public ErrorCode CreateDonation(UserDonated userDonated, ref String errMsg)
        {
            if (_userDonated.Create(userDonated, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            return ErrorCode.Success;
        }
        public ErrorCode SubmitDonation(List<Donated> donate, int donationEventId, string refNum, int userId, ref String errMsg)
        {

            // Define Philippine time zone
            TimeZoneInfo philippineTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");

            // Get current Philippine time
            DateTime philippineTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, philippineTimeZone);

            var toSave = new Donates()
            {
                eventType = 2,
                referenceNum = refNum,
                userId = userId,
                eventId = donationEventId,
                donatedAt = philippineTime,
                status = 1,
            };

            if (_donates.Create(toSave, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            foreach (var donatedItem in donate)
            {

                donatedItem.donatesId = toSave.donatesId;
                donatedItem.status = 0;

                if (_donated.Create(donatedItem, out errMsg) != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }
           
            return ErrorCode.Success;
        }
        public ErrorCode SubmitDonation1(Donated donate, int donationEventId, string refNum, int userId, int donationType, ref String errMsg)
        {

            // Define Philippine time zone
            TimeZoneInfo philippineTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");

            // Get current Philippine time
            DateTime philippineTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, philippineTimeZone);

            var toSave = new Donates()
            {
                eventType = donationType,
                referenceNum = refNum,
                userId = userId,
                eventId = donationEventId,
                donatedAt = philippineTime,
                status = 1,
            };

            if (_donates.Create(toSave, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            donate.donatesId = toSave.donatesId;
            donate.status = 0;

            if (_donated.Create(donate, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
        public ErrorCode SaveDonation(Donated donate, int donationEventId, string refNum, int userId, ref String errMsg)
        {
            var exist = DonatesIsExist(refNum);

            donate.donatesId = exist.donatesId;

            if (_donated.Create(donate, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
        public Donates GetDonatedByUserIdAndDonationEventId1(int donatesId)
        {
            return _donates._table.Where(m => m.donatesId == donatesId).FirstOrDefault();
        }
        public Donates GetDonatedByUserIdAndDonationEventId(string refNum)
        {
            return _donates._table.Where(m => m.referenceNum == refNum).FirstOrDefault();
        }
        public List<Donates> GetDonatesByEventIdAndUserId(int donationEventId, int userId)
        { 
            return _donates._table.Where(m => m.eventId == donationEventId && m.userId == userId).ToList();
        }
        public List<Donated> MyDonation(int donatesId)
        {
            return _donated._table.Where(m => m.donatesId == donatesId).ToList();
        }
        public Volunteers GetVolunteerByUserId(int userId, int eventId)
        {
            return _volunteers._table.Where(m => m.userId == userId && m.eventId == eventId).FirstOrDefault();
        }
        public List<Volunteers> GetVolunteersEventPendingByUserId(int userId)
        {
            var pendingEvents = _volunteers.GetAll().Where(m => m.userId == userId && m.Status == 0).ToList();
            List<Volunteers> pendings = new List<Volunteers>();
            foreach (var pending in pendingEvents)
            {
                var getPending = _volunteers.GetAll().Where(m => m.userId == userId && m.eventId == pending.eventId && m.Status == 0).OrderByDescending(m => m.applyVolunteerId).FirstOrDefault();

                if (!pendings.Contains(getPending))
                {
                    pendings.Add(getPending);
                }
            }
            return pendings;
        }
        public Volunteers GetVolunteerByUserIdAndEventId(int userId, int eventId)
        {
            return _volunteers._table.Where(m => m.userId == userId && m.eventId == eventId && m.Status == 0).FirstOrDefault();
        }
        public VolunteerInfo GetVolunteerInfoByUserId(int userId)
        {
            return _volunteerInfo._table.Where(m => m.userId == userId).FirstOrDefault();
        }
        public Donates DonatesExist(int userId, int eventId)
        {
            return _donates._table.Where(m => m.userId == userId && m.eventId == eventId).FirstOrDefault();
        }

        public Donates DonatesIsExist(string refNum)
        {
            return _donates._table.Where(m => m.referenceNum == refNum).FirstOrDefault();
        }

        public List<Volunteers> GetVolunteersEventParticipateByUserId(int userId)
        {
            var acceptedEvents = _volunteers.GetAll().Where(m => m.userId == userId && m.Status == 1).ToList();
            List<Volunteers> participate = new List<Volunteers>();
            foreach (var accept in acceptedEvents)
            {
                var getParticipate = _volunteers.GetAll().Where(m => m.userId == userId && m.eventId == accept.eventId && m.Status == 1).OrderByDescending(m => m.applyVolunteerId).FirstOrDefault();

                if (!participate.Contains(getParticipate))
                {
                    participate.Add(getParticipate);
                }
            }
            return participate;
        }
        public List<Notification> GetNotificationByUserIdAndEventId(int userId, int relatedId)
        {
            return _notification._table.Where(m => m.senderUserId == userId && m.relatedId == relatedId).ToList();
        }
        public ErrorCode DeleteNotification(int userId, int evenId, ref String errMsg)
        {
            var notification = GetNotificationByUserIdAndEventId(userId, evenId);

            foreach (var item in notification)
            {
                if (_notification.Delete(item.notificationId) != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }
            return ErrorCode.Success;
        }
        public void CheckVolunteerEventEndByUserId(int userId)
        {

            // Define Philippine time zone
            TimeZoneInfo philippineTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");

            // Get current Philippine time
            DateTime philippineTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, philippineTimeZone);

            var getEvents = _orgEvents.GetAll().ToList();
            var endedEvent = getEvents.Where(m => m.dateEnd < philippineTime && m.status == 1).ToList();

            foreach (var evt in endedEvent)
            {
                var getVolEvent = _volunteers.GetAll().Where(m => m.eventId == evt.eventId && m.userId == userId).ToList();
                // Move each volunteer record to VolunteersHistory
                foreach (var volunteer in getVolEvent.Where(m => m.userId == userId))
                {
                    var volunteerHistory = new VolunteersHistory
                    {
                        eventId = volunteer.eventId,
                        userId = volunteer.userId,
                        appliedAt = volunteer.appliedAt,
                        attended = 0
                    };

                    db.VolunteersHistory.Add(volunteerHistory);
                    db.SaveChanges();
                }
                foreach (var volunteer in getVolEvent)
                {
                    db.sp_RemoveEvent(volunteer.eventId);
                }
            }
        }
        public void RemoveVolunteerFromVolunteerByUserIdAndEventId(int userId, int? eventId)
        {
            db.sp_RemoveVolunteer(userId, eventId);
        }
        public List<sp_VolunteerHistory_Result> GetVolunteersHistoryByUserId(int userId)
        {
            List<sp_VolunteerHistory_Result> userEventHistory = new List<sp_VolunteerHistory_Result>();
            var checkVol = db.sp_VolunteerHistory(userId).ToList();
            if (!checkVol.Equals(0))
            {
                foreach (var volHistory in checkVol)
                {
                    userEventHistory.Add(volHistory);
                }
                return userEventHistory;
            }
            return userEventHistory;
        }

         public async Task<List<FilteredEvent>> RunRecommendation(int UserId)
         {

            // Define Philippine time zone
            TimeZoneInfo philippineTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");

            // Get current Philippine time
            DateTime philippineTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, philippineTimeZone);

            // Prepare the data to pass to Flask
            var datas = new
            {
                user_skills = db.VolunteerSkill.Where(m => m.userId == UserId).Select(m => new { userId = m.userId, skillId = m.skillId }).ToList(),
                event_data = _orgEvents.GetAll().Where(m => m.dateEnd >= philippineTime && m.status != 3).Select(m => new { eventId = m.eventId, eventDescription = m.eventDescription }).ToList(),
                event_skills = db.OrgSkillRequirement.Select(es => new { eventId = es.eventId, skillId = es.skillId }).ToList()
                //volunteer_history = db.VolunteersHistory.Where(vh => vh.userId == UserId).Select(vh => new { eventId = vh.eventId, attended = vh.attended }).ToList()
            };

            //string flaskApiUrl = "https://tabangapi.as.r.appspot.com/predict"; // Flask API URL
            //string flaskApiUrl = "http://127.0.0.1:5000/predictMatchOneSkillOrMore"; // Flask API URL
            string flaskApiUrl = "https://tabangapi.as.r.appspot.com/predictMatchOneSkillOrMore";

            List<FilteredEvent> recommendedEvents = new List<FilteredEvent>();

            using (var client = new HttpClient())
            {
                // Step 1: Send POST request to Flask API with the requestData
                var response = await client.PostAsJsonAsync(flaskApiUrl, datas);
                var jsonResponse = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception("Error calling the Python API: " + response.ReasonPhrase);
                }
                else
                {
                    // Step 2: Deserialize Flask API response to a list of recommended events
                    recommendedEvents = JsonConvert.DeserializeObject<List<FilteredEvent>>(jsonResponse);
                }
            }

            return recommendedEvents; // Return the list of recommended events
        }
    }
}