using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tabang_Hub.Utils;

namespace Tabang_Hub.Controllers
{
    public class MessageController : BaseController
    {
        // GET: Message
        [Authorize]
        public ActionResult Message()
        {
            try
            {

                var user = _organizationManager.GetUserByUserId(UserId);
                ViewBag.UserId = UserId;
                if (user != null && user.roleId == 2)
                {
                    var orgInfo = _organizationManager.GetOrgInfoByUserId(user.userId);
                    var listOfGc = new List<sp_ListOfGc_Result>();
                    var eventImage = new List<OrgEventImage>();

                    var gc = _messageManager.GetGroupChatByUserId(user.userId);

                    foreach (var g in gc)
                    {
                        var gcItem = new sp_ListOfGc_Result
                        {
                            groupChatId = g.groupChatId,
                            userId = user.userId,
                            eventId = g.eventId,
                            eventTitle = g.OrgEvents.eventTitle,
                        };

                        var image = _messageManager.GetEventImageByEventId((int)g.eventId);

                        listOfGc.Add(gcItem);
                        eventImage.Add(image);
                    }
                    var indexModel = new Lists()
                    {
                        userAccount = user,
                        OrgInfo = orgInfo,   
                        detailsEventImage = eventImage,
                        listOfGc = listOfGc,
                    };

                    return View(indexModel);
                }
                else
                {
                    var getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();

                    var checkVolunteers = db.Volunteers.Where(m => m.userId == UserId).FirstOrDefault();

                    var getOrgEvent = checkVolunteers != null
                    ? _orgEvents.GetAll().Where(m => m.eventId == checkVolunteers.eventId).ToList()
                    : new List<OrgEvents>(); // Return an empty list if null

                    var eventImage = new List<OrgEventImage>();

                    var listOfGC = db.sp_ListOfGc(UserId).ToList();

                    foreach (var gc in listOfGC)
                    {
                        var image = _messageManager.GetEventImageByEventId((int)gc.eventId);

                        eventImage.Add(image);
                    }


                    var indexModel = new Lists()
                    {
                        picture = getProfile,
                        volunteersInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList(),
                        detailsEventImage = eventImage,
                        orgEvents = getOrgEvent,
                        listOfGc = listOfGC
                    };

                    return View(indexModel);
                }         
            }
            catch (Exception)
            {
                var indexModel = new Lists()
                {
                    picture = db.ProfilePicture.Where(m => m.userId == UserId).ToList(),
                    volunteersInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList(),
                    detailsEventImage = null,
                    orgEvents = null,
                    listOfGc = null
                };

                return View(indexModel);
            }
        }

        public JsonResult GetGroupInfo(int groupId)
        {
            try
            {
                var getEventID = db.GroupChat.Where(m => m.groupChatId == groupId).Select(m => m.eventId).FirstOrDefault();
                var getEventInfo = _orgEvents.GetAll().Where(m => m.eventId == getEventID).FirstOrDefault();

                if (getEventInfo != null)
                {
                    return Json(new
                    {
                        eventTitle = getEventInfo.eventTitle,
                        eventDescription = getEventInfo.eventDescription,
                        dateStart = getEventInfo.dateStart.Value.ToString("MMMM dd, yyyy, hh:mm tt"),
                        dateEnd = getEventInfo.dateEnd.Value.ToString("MMMM dd, yyyy, hh:mm tt"),
                        location = getEventInfo.location
                    }, JsonRequestBehavior.AllowGet);
                }
                return Json(null, JsonRequestBehavior.AllowGet);
            }
            catch (Exception)
            {
                return Json(null, JsonRequestBehavior.AllowGet);
            }
        }
        public JsonResult GetGroupMembers(int groupId)
        {
            try
            {
                var getEventID = db.GroupChat.Where(m => m.groupChatId == groupId).Select(m => m.eventId).FirstOrDefault();

                if (getEventID != null)
                {
                    var getMemberIDs = _volunteers.GetAll().Where(m => m.eventId == getEventID && m.Status == 1).Select(m => m.userId).ToList();

                    if (getMemberIDs.Any())
                    {
                        var members = db.VolunteerInfo.Where(m => getMemberIDs.Contains(m.userId)).Select(m => new
                        {
                            userId = m.userId,
                            fName = m.fName,
                            lName = m.lName,
                            appliedAt = db.Volunteers.Where(a => a.userId == m.userId && a.appliedAt != null).Select(a => a.appliedAt).FirstOrDefault(),
                            profilePath = db.ProfilePicture.Where(p => p.userId == m.userId).Select(p => p.profilePath).FirstOrDefault()
                        }).ToList();

                        var formattedMembers = members.Select(m => new
                        {
                            userId = m.userId,
                            fName = m.fName,
                            lName = m.lName,
                            appliedAt = m.appliedAt != null ? m.appliedAt.Value.ToString("MMMM dd, yyyy") : "N/A",
                            profilePath = m.profilePath ?? "default.jpg"
                        }).ToList();

                        return Json(formattedMembers, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(null, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }
        [HttpPost]
        public JsonResult KickVolunteer(int userId, string reason, int gc)
        {
            try
            {
                var geteventId = db.GroupChat.Where(m => m.groupChatId == gc).Select(m => m.eventId).FirstOrDefault();
                var checkVol = _volunteers.GetAll().Where(m => m.userId == userId && m.eventId == geteventId).FirstOrDefault();
                if (checkVol != null)
                {
                    db.sp_RemoveVolunteer(userId, checkVol.eventId);
                    return Json(new { success = true, message = "Volunteer successfully kicked." });
                }

                return Json(new { success = false, message = "An error occurred while kicking the volunteer." });
            }
            catch (Exception)
            {
                return Json(new { success = false, message = "An error occurred while kicking the volunteer." });
            }
        }
    }
}