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
        [Authorize]
        public ActionResult Calendar(string section = "ongoingEventsSection")
        {

            var getInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList();
            var getVolunteerSkills = db.VolunteerSkill.Where(m => m.userId == UserId).ToList();
            var getSkills = _skills.GetAll().ToList();
            var getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();
            var getOrgInfo = _orgInfo.GetAll().ToList();
            var getVolunteers = _volunteers.GetAll().ToList();
            var orgEventsSelectId = _orgEvents.GetAll().Where(m => m.dateEnd >= DateTime.Now && m.status != 3).Select(m => m.eventId).ToList();
            List<Volunteers> pendings = _volunteerManager.GetVolunteersEventPendingByUserId(UserId);
            List<Volunteers> accepted = _volunteerManager.GetVolunteersEventParticipateByUserId(UserId);

            var getParticipate = _calendarManager.GetEventParticipate(UserId);

            var indexModel = new Lists()
            {
                orgEvents = accepted.OrderByDescending(m => m.applyVolunteerId).Select(e => _orgEvents.GetAll().Where(m => m.status != 3).FirstOrDefault(o => o.eventId == e.eventId)).ToList(),
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
                listOfEvents = db.vw_ListOfEvent.Where(m => m.status != 3).OrderByDescending(m => m.Event_Id).ToList(),
                volunteers = getVolunteers,
                orgInfos = getOrgInfo,
                listOfEventsSection = db.vw_ListOfEvent.Where(m => m.End_Date >= DateTime.Now && m.status != 3).ToList()
            };

            ViewBag.SectionToShow = section;
            return View(indexModel);
        }

        [HttpPost]
        public ActionResult CancelRequest(int eventId)
        {
            try
            {
                var evnt = _organizationManager.GetEventByEventId(eventId);
                var vol = _organizationManager.GetUserByUserId(UserId);

                if (evnt != null)
                {
                    var orga = _organizationManager.GetUserByUserId((int)evnt.userId);

                    db.sp_CancelRequest(eventId, UserId);

                    if (_volunteerManager.DeleteNotification(UserId, eventId, ref ErrorMessage) != ErrorCode.Success)
                    {
                        return Json(new { success = false, message = "Error Deleting notificaiton" });
                    }

                    if (_organizationManager.SentNotif(orga.userId, UserId, eventId, "Cancel Application", $"Volunteer {vol.email} canceled his application og event {evnt.eventTitle}!", 0, ref ErrorMessage) != ErrorCode.Success)
                    {
                        return Json(new { success = false, message = "Request cancel failed" });
                    }

                    return Json(new { success = true, message = "Request Cancelled" });
                }

                return Json(new { success = true, message = "Request Cancelled" });

            }
            catch (Exception)
            {
                return Json(new { success = false, message = "error message" });
            }
        }
        [HttpPost]
        public ActionResult LeaveEvent(int eventId)
        {
            try
            {
                var updateVol = _volunteers.GetAll().Where(m => m.userId == UserId && m.eventId == eventId).FirstOrDefault();
                var events = _organizationManager.GetEventByEventId(eventId);
                var volInfo = _volunteerManager.GetVolunteerInfoByUserId((int)updateVol.userId);

                if (updateVol != null)
                {
                    db.sp_LeaveEvent(updateVol.eventId, updateVol.userId);

                    // Save the notification for the organization
                    var notification = new Notification
                    {
                        userId = events.userId, // Notify the organization
                        senderUserId = UserId, // The user who donated
                        relatedId = eventId,
                        type = "Leave Event",
                        content = $"{volInfo.lName + ", " + volInfo.lName} Left {events.eventTitle} Event.",
                        broadcast = 0, // Not a broadcast
                        status = 0, // Assuming 1 is the status for a new notification
                        createdAt = DateTime.Now,
                        readAt = null // Initially unread
                    };

                    db.Notification.Add(notification);
                    db.SaveChanges(); // Save the notification

                    return Json(new { success = true, message = "Left Successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = "Error" });
                }
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "error message" });
            }
        }
    }
}