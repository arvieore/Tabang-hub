using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tabang_Hub.Utils;
using Tabang_Hub.Repository;

namespace Tabang_Hub.Controllers
{
    public class CalendarController : BaseController
    {
        // GET: Calendar
        public ActionResult Calendar()
        {

            var getInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList();
            var getVolunteerSkills = db.VolunteerSkill.Where(m => m.userId == UserId).ToList();
            var getSkills = _skills.GetAll().ToList();
            var getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();
            var getOrgInfo = _orgInfo.GetAll().ToList();
            var getVolunteers = _volunteers.GetAll().ToList();
            var orgEventsSelectId = _orgEvents.GetAll().Where(m => m.dateEnd >= DateTime.Now).Select(m => m.eventId).ToList();
            List<Volunteers> pendings = _volunteerManager.GetVolunteersEventPendingByUserId(UserId);
            List<Volunteers> accepted = _volunteerManager.GetVolunteersEventParticipateByUserId(UserId);

            var getParticipate = _calendarManager.GetEventParticipate(UserId);

            var indexModel = new Lists()
            {
                orgEvents = accepted.OrderByDescending(m => m.applyVolunteerId).Select(e => _orgEvents.GetAll().FirstOrDefault(o => o.eventId == e.eventId)).ToList(),
                detailsEventImage = _eventImages.GetAll().ToList(),
                volunteersHistories = db.sp_VolunteerHistory(UserId).ToList(),
                pendingOrgDetails = pendings.OrderByDescending(m => m.applyVolunteerId).Select(e => _pendingOrgDetails.GetAll().FirstOrDefault(p => p.eventId == e.eventId)).ToList(),
                rating = db.Rating.Where(m => m.userId == UserId).ToList(),
                ongoingEvents = getParticipate.ongoingEvents,
                pendingEvents = getParticipate.pendingEvents,
                eventHistory = getParticipate.eventHistory,

                volunteersInfo = getInfo,
                volunteersSkills = getVolunteerSkills,
                skills = getSkills,
                picture = getProfile,
                listOfEvents = db.vw_ListOfEvent.OrderByDescending(m => m.Event_Id).ToList(),
                volunteers = getVolunteers,
                orgInfos = getOrgInfo,
                listOfEventsSection = db.vw_ListOfEvent.Where(m => m.End_Date >= DateTime.Now).ToList()
            };
            return View(indexModel);
        }
    }
}