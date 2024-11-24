using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Tabang_Hub.Repository;
using Tabang_Hub.Utils;
using System.Text;
using Microsoft.AspNet.SignalR.Hubs;
using System.Threading.Tasks;
using Tabang_Hub.Hubs;
using static Tabang_Hub.Utils.Lists;

namespace Tabang_Hub.Controllers
{
    public class OrganizationController : BaseController
    {
        // GET: Organization
        [Authorize]
        public ActionResult Index()
        {
            var orgInfo = _organizationManager.GetOrgInfoByUserId(UserId);
            var totalVolunteer = _organizationManager.GetTotalVolunteerByUserId(UserId);
            var totalDonation = _organizationManager.GetTotalDonationByUserId(UserId);
            var totalEvents = _organizationManager.GetOrgEventsByUserId(UserId);
            
            //var profile = _organizationManager.GetProfileByProfileId(orgInfo.profileId);

            var pendingVol = new List<Volunteers>();
            var donated = new List<UserDonated>();

            foreach (var events in totalEvents)
            {
                var volunteers = _organizationManager.GetPendingVolunteersByEventId(events.eventId);
                var userDonated = _organizationManager.ListOfUserDonated(events.eventId);

                foreach (var volDonated in userDonated)
                { 
                    donated.Add(volDonated);
                }
                foreach (var vol in volunteers)
                {
                    if (!pendingVol.Any(v => v.userId == vol.userId && v.eventId == vol.eventId))
                    {
                        pendingVol.Add(vol);
                    }
                }
            }

            var recentDonations = donated
               .OrderByDescending(d => d.donatedAt)
               .Take(7)
               .ToList();

            var indexModel = new Lists()
            {
                OrgInfo = orgInfo,
                totalVolunteer = totalVolunteer,
                totalDonation = totalDonation,
                orgEvents = totalEvents,
                volunteers = pendingVol,
                listofUserDonated = recentDonations
                //profilePic = profile,
            };
            return View(indexModel);
        }
        [Authorize]
        public ActionResult OrgProfile()
        {
            var orgInfo = _organizationManager.GetOrgInfoByUserId(UserId);
            var orgEvents = _organizationManager.GetOrgEventsByUserId(UserId);
            var orgImage = new List<OrgEventImage>();
            foreach (var image in orgEvents)
            {
                var orgEvenImage = _organizationManager.GetEventImageByEventId(image.eventId);

                orgImage.Add(orgEvenImage);
            }
            
            //var profile = _organizationManager.GetProfileByProfileId(orgInfo.profileId);

            var indexModdel = new Lists()
            {
                OrgInfo = orgInfo,
                getAllOrgEvent = orgEvents,
                detailsEventImage = orgImage,
                //profilePic = profile,
            };
            return View(indexModdel);
        }
        [HttpPost]
        public JsonResult EditOrgProfile(Lists orgProfile, HttpPostedFileBase profilePic, HttpPostedFileBase coverPic)
        {
            string errMsg = string.Empty;

            // Ensure the email cannot be changed by reassigning the original email from the database
            var existingOrgInfo = _organizationManager.GetOrgInfoByUserId(UserId); // Retrieve existing organization info by UserId
            if (existingOrgInfo != null)
            {
                orgProfile.OrgInfo.orgEmail = existingOrgInfo.orgEmail; // Maintain the original email
            }
            else
            {
                return Json(new { success = false, message = "Organization information not found." });
            }

            // Handle profile picture upload
            if (profilePic != null && profilePic.ContentLength > 0)
            {
                var inputFileName = Path.GetFileName(profilePic.FileName);
                var serverSavePath = Path.Combine(Server.MapPath("~/Content/IdPicture/"), inputFileName);

                if (!Directory.Exists(Server.MapPath("~/Content/IdPicture/")))
                    Directory.CreateDirectory(Server.MapPath("~/Content/IdPicture/"));

                profilePic.SaveAs(serverSavePath);
                orgProfile.OrgInfo.profilePath = "~/Content/IdPicture/" + inputFileName;
            }

            // Handle cover photo upload
            if (coverPic != null && coverPic.ContentLength > 0)
            {
                var coverFileName = Path.GetFileName(coverPic.FileName);
                var coverSavePath = Path.Combine(Server.MapPath("~/Content/CoverPhotos/"), coverFileName);

                if (!Directory.Exists(Server.MapPath("~/Content/CoverPhotos/")))
                    Directory.CreateDirectory(Server.MapPath("~/Content/CoverPhotos/"));

                coverPic.SaveAs(coverSavePath);
                orgProfile.OrgInfo.coverPhoto = "~/Content/CoverPhotos/" + coverFileName;
            }

            // Update organization profile info
            var result = _organizationManager.EditOrgInfo(orgProfile.OrgInfo, UserId, ref errMsg);

            if (result == ErrorCode.Success)
            {
                return Json(new { success = true, message = "Profile updated successfully." });
            }
            else
            {
                return Json(new { success = false, message = errMsg });
            }
        }
        [Authorize]
        public ActionResult EventsList()
        {
            var lists = _organizationManager.ListOfEvents1(UserId);
            var listOfSkill = _organizationManager.ListOfSkills();
            var listofUserDonated = _organizationManager.ListOfUserDonated(UserId);
            var orgInfo = _organizationManager.GetOrgInfoByUserId(UserId);
            //var profile = _organizationManager.GetProfileByProfileId(orgInfo.profileId);

            var indexModel = new Lists()
            {
                listOfEvents = lists,
                listOfSkills = listOfSkill,
                listofUserDonated = listofUserDonated,
                OrgInfo = orgInfo,
                //profilePic = profile,
            };

            return View(indexModel);
        }
        [HttpPost]
        public async Task<ActionResult> CreateEvents(Lists events, List<string> skills, HttpPostedFileBase[] images)
        {
            // Sanitize skill names
            var sanitizedSkills = new List<string>();
            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    var sanitizedSkillName = skill.Replace(" x", "").Trim();
                    sanitizedSkills.Add(sanitizedSkillName);
                }
            }

            events.CreateEvents.userId = UserId;
            string errMsg = string.Empty;
            List<string> uploadedFiles = new List<string>();

            // Server-side validation
            if (string.IsNullOrWhiteSpace(events.CreateEvents.eventTitle))
            {
                ModelState.AddModelError("CreateEvents.eventTitle", "Event Title is required.");
            }
            if (string.IsNullOrWhiteSpace(events.CreateEvents.eventDescription))
            {
                ModelState.AddModelError("CreateEvents.eventDescription", "Event Description is required.");
            }
            if (events.CreateEvents.maxVolunteer <= 0)
            {
                ModelState.AddModelError("CreateEvents.maxVolunteer", "Maximum Volunteers must be greater than 0.");
            }
            if (events.CreateEvents.dateStart == null || events.CreateEvents.dateEnd == null)
            {
                ModelState.AddModelError("CreateEvents.dateStart", "Start Date and End Date are required.");
            }
            if (events.CreateEvents.dateStart < DateTime.Now)
            { 
                ModelState.AddModelError("CreateEvents.dateStart", "Start date and time cannot be before the current date and time.");
            }
            if (events.CreateEvents.dateEnd <= events.CreateEvents.dateStart)
            {
                ModelState.AddModelError("CreateEvents.dateEnd", "End date and time cannot be before or the same as the start date and time.");
            }
            if (string.IsNullOrWhiteSpace(events.CreateEvents.location))
            {
                ModelState.AddModelError("CreateEvents.location", "Location is required.");
            }
            if (sanitizedSkills == null || sanitizedSkills.Count == 0)
            {
                ModelState.AddModelError("CreateEvents.skills", "At least one skill is required.");
            }
            if (images == null || images.Length == 0)
            {
                ModelState.AddModelError("CreateEvents.images", "At least one image is required.");
            }

            // Image processing
            if (images != null && images.Length > 0)
            {
                var imagePath = Server.MapPath("~/Content/Events");
                Directory.CreateDirectory(imagePath); // Create directory if it doesn't exist

                foreach (var image in images)
                {
                    if (image != null && image.ContentLength > 0)
                    {
                        var fileName = Path.GetFileName(image.FileName);
                        var filePath = Path.Combine(imagePath, fileName);
                        image.SaveAs(filePath);
                        uploadedFiles.Add(fileName);
                    }
                }
            }

            // Store the event and associated skill requirements
            if (_organizationManager.CreateEvents(events.CreateEvents, uploadedFiles, sanitizedSkills, ref errMsg) != ErrorCode.Success)
            {
                ModelState.AddModelError(string.Empty, errMsg);
                return RedirectToAction("EventsList");
            }

            var filtered = await _organizationManager.GetMatchedVolunteers(events.CreateEvents.eventId);
            var user = _organizationManager.GetOrgInfoByUserId(UserId);

            foreach(var fltr in filtered.SortedByRating)
            {
                if (_organizationManager.SentNotif(fltr.userId, UserId, events.CreateEvents.eventId, "Create Event", $"{user.orgName} create a new event that matched your skills!", 0, ref ErrorMessage) != ErrorCode.Success)
                {
                    TempData["Error Sending Notification"] = true;
                    return RedirectToAction("EventsList");
                }
            }

            TempData["Success"] = true;
            return RedirectToAction("EventsList");
        }
        [Authorize]
        public async Task<ActionResult> Details(int id)
        {
            var events = _organizationManager.GetEventById(id);
            var listofImage = _organizationManager.listOfEventImage(id);
            var listOfSkills = _organizationManager.listOfSkillRequirement(id);
            var listofUserDonated = _organizationManager.ListOfUserDonated(id);
            var orgInfo = _organizationManager.GetOrgInfoByUserId(UserId);
            var listOfEventVolunteers = _organizationManager.ListOfEventVolunteers(id);
            var listofEventVolunteerHistory = _organizationManager.ListOfEventVolunteersHistory(id);
            var skillReq = _organizationManager.listOfSkillRequirement(events.eventId);
            var volunteerSkills = _organizationManager.ListOfEventVolunteerSkills();

            //var matchedSkill = _organizationManager.GetMatchedVolunteers(id);
            var matchedSkill = await _organizationManager.GetMatchedVolunteers(id);
            //var profile = _organizationManager.GetProfileByProfileId(orgInfo.profileId);
            var listOfSkill = _organizationManager.ListOfSkills();
            var pendingVol = new List<Volunteers>();
            var rating = new List<Rating>(); 
            
            //na a anhi ang users nga nag based sa Rating
            //var getRate = new List<FilteredVolunteer>();
            //var usrRank = new List<UserAccount>();
            //foreach (var dataByRating in matchedSkill.SortedByRating)
            //{
            //    var usrs = _userAcc.GetAll().Where(m => m.userId == dataByRating.userId).ToList();
            //    usrRank.AddRange(usrs);

            //    getRate.AddRange(matchedSkill.SortedByRating.Where(m => m.userId == dataByRating.userId).ToList());
            //}
            ////na a daari ang mga users nga based sa availability
            //var userAvail = new List<UserAccount>();
            //foreach (var dataAvailability in matchedSkill.SortedByAvailability)
            //{
            //    var usrs = _userAcc.GetAll().Where(m => m.userId == dataAvailability.userId).ToList();
            //    userAvail.AddRange(usrs);

            //    if(!matchedSkill.SortedByAvailability.Any(m => getRate.Select(n => n.userId).Contains(m.userId)))
            //    {
            //        getRate.AddRange(matchedSkill.SortedByAvailability.Where(m => m.userId == dataAvailability.userId).ToList());
            //    }
            //}

            foreach (var data in listOfEventVolunteers)
            {
                if (!pendingVol.Any(v => v.userId == data.userId))
                {
                    var toAppend = new Volunteers()
                    {
                        applyVolunteerId = data.applyVolunteerId,
                        userId = data.userId,
                        eventId = data.eventId,
                        Status = data.Status,
                        appliedAt = data.appliedAt,

                        OrgEvents = data.OrgEvents,
                        UserAccount = data.UserAccount,
                    };

                    pendingVol.Add(toAppend);

                    var rate = _organizationManager.GetVolunteerRatingsByUserId((int)data.userId);

                    foreach (var r in rate)
                    {
                        var rateToAppend = new Rating()
                        {
                            ratingId = r.ratingId,
                            eventId = r.eventId,
                            userId = r.userId,
                            skillId = r.skillId,
                            rate = r.rate,
                            ratedAt = r.ratedAt,
                        };

                        rating.Add(rateToAppend);
                    }
                }
            }

            var indexModel = new Lists()
            {
                OrgInfo = orgInfo,
                eventDetails = events,
                listOfSkills = listOfSkill,
                detailsEventImage = listofImage,
                detailsSkillRequirement = listOfSkills,
                listOfEventVolunteers = pendingVol,
                volunteersSkills = volunteerSkills,
                listofvolunteerHistory = listofEventVolunteerHistory,
                listofUserDonated = listofUserDonated,
                listOfRatings = rating,
                //matchedSkills = usrRank,
                //volunteerAvail = userAvail,
                volunteersInfo = _volunteerInfo.GetAll().ToList(),
                skillRequirement1 = skillReq,
                //filteredVolunteers = getRate,
                ListToBeInvite = db.vw_ListOfVolunteerToBeInvite.ToList(),
                //matchedSkills = matchedSkill,
                //profilePic = profile,
            };

            if (events != null)
            {
                return View(indexModel);
            }
            return RedirectToAction("EventsManagement");
        }

        [HttpPost]
        public JsonResult InviteVolunteer(List<int> selectedVolunteers, int eventId)
        {
            string errMsg = string.Empty;
            var events = _organizationManager.GetEventByEventId(eventId);
            List<int> alreadyJoinedUsers = new List<int>();
            List<int> alreadyInvitedUsers = new List<int>();
            List<int> newlyInvitedUsers = new List<int>();

            foreach (var userId in selectedVolunteers)
            {
                // Check if the user is already joined
                var volunteer = _organizationManager.GetVolunteerById(userId, eventId);
                if (volunteer != null)
                {
                    if (volunteer.Status != 3)
                    {
                        alreadyJoinedUsers.Add(userId);
                        continue; // Skip to next user if they are already joined
                    }
                    else if (volunteer.Status == 3)
                    {
                        alreadyInvitedUsers.Add(userId);
                        // Optionally, you can choose to skip inviting again
                        // continue;
                    }
                }
                else
                {
                    // Process the invitation for users who are not yet joined
                    if (_organizationManager.InviteVolunteer(userId, eventId, ref errMsg) != ErrorCode.Success)
                    {
                        return Json(new { success = false, message = errMsg });
                    }
                    newlyInvitedUsers.Add(userId);
                }

                // Send notification
                var notification = new Notification
                {
                    userId = userId,
                    senderUserId = UserId, // Ensure UserId is properly set
                    relatedId = events.eventId,
                    type = "Invitation",
                    content = $"You have been invited to join the {events.eventTitle} event.",
                    broadcast = 0,
                    status = 0,
                    createdAt = DateTime.Now,
                    readAt = null
                };

                db.Notification.Add(notification);
                db.SaveChanges();
            }

            // Return JSON indicating success, listing already joined, already invited, and newly invited users
            return Json(new
            {
                success = true,
                redirectUrl = Url.Action("Details", "Organization", new { id = eventId }),
                alreadyJoinedUsers = alreadyJoinedUsers,
                alreadyInvitedUsers = alreadyInvitedUsers,
                newlyInvitedUsers = newlyInvitedUsers
            });
        }
        [HttpGet]
        public JsonResult GetAllVolunteers()
        {
            try
            {
                // Fetch all volunteers and their skills in memory
                var allVolunteers = db.vw_AllVolunteers
                    .AsEnumerable() // Materialize the query to bring data into memory
                    .Select(v => new
                    {
                        v.userId,
                        v.FullName,
                        v.OverallRating,
                        SkillIds = db.VolunteerSkill
                            .Where(s => s.userId == v.userId)
                            .Select(s => s.skillId)
                            .ToList() // Process the subquery in memory
                    })
                    .ToList();

                return Json(new
                {
                    success = true,
                    message = "All volunteers retrieved successfully.",
                    volunteers = allVolunteers
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while retrieving volunteers: " + ex.Message
                }, JsonRequestBehavior.AllowGet);
            }
        }

        private static List<int> SelectedSkills = new List<int>();
        [HttpPost]
        public async Task<JsonResult> FilterSkill(List<int> skillId, int eventId)
        {
            try
            {
                // Check if no skills are selected
                if (skillId == null || !skillId.Any())
                {
                    List<VolunteerInvite> volunteer = new List<VolunteerInvite>();

                    var vol = await _organizationManager.ConvertFeedbackToSentiment();


                    return Json(new
                    {
                        success = true,
                        message = "All volunteers retrieved successfully.",
                        volunteers = vol
                    });
                }
                else
                {
                    // Fetch volunteers matching the selected skills
                    var filteredVolunteers = await _organizationManager.GetVolunteerFilteredSkill(eventId, skillId);

                    var formattedVolunteers = filteredVolunteers
                        .Select(v => new
                        {
                            FullName = v.FullName,
                            OverallRating = v.OverallRating,
                            UserId = v.UserId,
                            Feedback = v.Feedback
                        }).ToList();

                    return Json(new
                    {
                        success = true,
                        message = "Filtered volunteers retrieved successfully.",
                        volunteers = formattedVolunteers
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = "An error occurred while retrieving volunteers: " + ex.Message
                });
            }
        }
        [HttpPost]
        public async Task<JsonResult> FilterRate()
        {
            var filteredVolunteers = await _organizationManager.GetVolunteersByRating();

            var formattedVolunteers = filteredVolunteers
                        .Select(v => new
                        {
                            FullName = v.FullName,
                            OverallRating = v.OverallRating,
                            UserId = v.UserId,
                            Feedback = v.Feedback
                        }).ToList();

            return Json(new { success = true, volunteers = formattedVolunteers });
        }

        [HttpPost]
        public async Task<JsonResult> FilterBySkillsAndRatings(List<int> skillId, int eventId)
        {
            var filteredVolunteers = await _organizationManager.GetVolunteersBySkillsAndRatings(skillId, eventId);

            var formattedVolunteers = filteredVolunteers
                        .Select(v => new
                        {
                            FullName = v.FullName,
                            OverallRating = v.OverallRating,
                            UserId = v.UserId,
                            Feedback = v.Feedback
                        }).ToList();

            return Json(new { success = true, volunteers = formattedVolunteers });
        }

        [HttpPost]
        public JsonResult Invite(List<int> userId)
        {
            return Json( new { });
        }
        [HttpPost]
        public JsonResult EditEvent(Lists events, List<string> skills, string[] skillsToRemove, HttpPostedFileBase[] images, int eventId)
        {
            string errMsg = string.Empty;
            var allowedExtensions = new List<string> { ".jpg", ".jpeg", ".png", ".gif" };

            List<string> uploadedFiles = new List<string>();

            // Handle image uploads
            if (images != null && images.Length > 0)
            {
                foreach (var image in images)
                {
                    if (image != null && image.ContentLength > 0)
                    {
                        var extension = Path.GetExtension(image.FileName).ToLower();

                        if (!allowedExtensions.Contains(extension))
                        {
                            return Json(new { success = false, message = "Invalid file type. Only JPG, JPEG, PNG, and GIF files are allowed." });
                        }

                        var inputFileName = Path.GetFileName(image.FileName);
                        var serverSavePath = Path.Combine(Server.MapPath("~/Content/Events/"), inputFileName);

                        if (!Directory.Exists(Server.MapPath("~/Content/Events/")))
                            Directory.CreateDirectory(Server.MapPath("~/Content/Events/"));

                        try
                        {
                            // Save the image (you can include resizing logic here)
                            image.SaveAs(serverSavePath);
                            uploadedFiles.Add(inputFileName);
                        }
                        catch (Exception ex)
                        {
                            return Json(new { success = false, message = $"Error processing file {inputFileName}: {ex.Message}" });
                        }
                    }
                }
            }

            if (Request.Form["events.CreateEvents.targetAmount"] != null)
            {
                var targetAmountString = Request.Form["events.CreateEvents.targetAmount"];
                if (string.IsNullOrWhiteSpace(targetAmountString))
                {
                    events.CreateEvents.targetAmount = null; // Set to null if empty
                }
                else
                {
                    // Validate and parse the target amount
                    if (decimal.TryParse(targetAmountString, out decimal targetAmountValue) && targetAmountValue > 0)
                    {
                        events.CreateEvents.targetAmount = targetAmountValue;
                    }
                    else
                    {
                        return Json(new { success = false, message = "Please enter a valid target amount." });
                    }
                }
            }
            else
            {
                // If the field is not present, set targetAmount to null
                events.CreateEvents.targetAmount = null;
            }

            // Fetch the current event from the database
            var currentEvent = _organizationManager.GetEventByEventId(eventId);
            if (currentEvent == null)
            {
                return Json(new { success = false, message = "Event not found." });
            }

            // Compare the event details to detect changes
            bool hasChanges = false;
            bool skillsChanged = false;
            bool imagesChanged = false;

            if (events.CreateEvents.eventTitle != currentEvent.eventTitle)
            {
                hasChanges = true;
            }
            if (events.CreateEvents.eventDescription != currentEvent.eventDescription)
            {
                hasChanges = true;
            }
            if (events.CreateEvents.dateStart != currentEvent.dateStart)
            {
                hasChanges = true;
            }
            if (events.CreateEvents.dateEnd != currentEvent.dateEnd)
            {
                hasChanges = true;
            }
            if (events.CreateEvents.location != currentEvent.location)
            {
                hasChanges = true;
            }
            if (events.CreateEvents.targetAmount != currentEvent.targetAmount)
            {
                hasChanges = true;
            }
            // Add comparisons for other fields as necessary

            // Check for skills changes
            var currentSkills = _organizationManager.listOfSkillRequirement(eventId);
            var currentSkillsSet = new HashSet<string>(currentSkills.Select(s => s.Skills.skillName));
            var newSkillsSet = new HashSet<string>(skills);

            if (!currentSkillsSet.SetEquals(newSkillsSet))
            {
                skillsChanged = true;
                hasChanges = true;
            }
            // Check if skills are to be removed
            if (skillsToRemove != null && skillsToRemove.Length > 0)
            {
                skillsChanged = true;
                hasChanges = true;
            }

            // Check for image changes
            if (uploadedFiles.Count > 0)
            {
                imagesChanged = true;
                hasChanges = true;
            }
            // Note: If you also handle image deletions, include that logic here

            // Handle no changes
            if (!hasChanges)
            {
                return Json(new { success = false, message = "No changes were made to the event." });
            }

            // Proceed to update the event details in the database
            if (_organizationManager.EditEvent(events.CreateEvents, skills, skillsToRemove, uploadedFiles, eventId, ref errMsg) != ErrorCode.Success)
            {
                return Json(new { success = false, message = errMsg });
            }

            // Send notifications to volunteers regardless of the changes
            var volunteers = _organizationManager.GetVolunteersByEventId(eventId);

            foreach (var vol in volunteers)
            {
                var notification = new Notification
                {
                    userId = vol.userId,
                    senderUserId = UserId,
                    relatedId = currentEvent.eventId,
                    type = "Event Update",
                    content = $"{events.CreateEvents.eventTitle} Event has been edited.",
                    broadcast = 0,
                    status = 0,
                    createdAt = DateTime.Now,
                    readAt = null
                };

                db.Notification.Add(notification);
            }
            db.SaveChanges(); // Save the notifications

            return Json(new { success = true, message = "Event edited successfully.", redirectUrl = Url.Action("Details", new { id = eventId }) });
        }

        [HttpPost]
        public ActionResult Delete(int eventId)
        {
            var deleteEvent = _organizationManager.DeleteEvent(eventId);
            var volunteer = _organizationManager.GetVolunteersByEventId(eventId);
            var events = _organizationManager.GetEventByEventId(eventId);

            if (deleteEvent != ErrorCode.Success)
            {
                // You may want to set a TempData or ViewBag message to inform the user of the error
                TempData["ErrorMessage"] = "There was an error deleting the event. Please try again.";
                return RedirectToAction("EventsList");
            }

            foreach (var vol in volunteer)
            {
                var notification = new Notification
                {
                    userId = vol.userId,
                    senderUserId = UserId,
                    relatedId = eventId,
                    type = "Donation",
                    content = $"{events.eventTitle} Event has been deleted.",
                    broadcast = 0,
                    status = 0,
                    createdAt = DateTime.Now,
                    readAt = null
                };


                db.Notification.Add(notification);
                db.SaveChanges(); // Save the notification
            }
           


            // You may want to set a TempData or ViewBag message to inform the user of the success
            TempData["SuccessMessage"] = "Event deleted successfully.";
            return RedirectToAction("EventsList");
        }
        [HttpPost]
        public JsonResult ConfirmApplicants(int id, int eventId)
        {
            string errMsg = string.Empty;

            if (_organizationManager.ConfirmApplicants(id, eventId, ref errMsg) == ErrorCode.Success)
            {
                // Create an instance of your NotificationHub and call SendNotification
                var notificationHub = new NotificationHub();

                notificationHub.SendNotification(
                    userId: id, // The volunteer's user ID
                    senderUserId: UserId, // The organization ID or admin ID who is sending the notification
                    relatedId: eventId,
                    type: "Acceptance",
                    content: "You have been accepted to participate in the event!"
                );

                // Return JSON response indicating success with redirect URL to event details page
                return Json(new { success = true, message = "Volunteer confirmed successfully.", redirectUrl = Url.Action("Details", "Organization", new { id = eventId }) });
            }
            else
            {
                // Return JSON response indicating failure with an error message
                return Json(new { success = false, message = errMsg });
            }
        }
        [HttpPost]
        public JsonResult DeclineApplicants(int id, int eventId)
        {
            string errMsg = string.Empty;

            if (_organizationManager.DeclineApplicant(id, eventId) == ErrorCode.Success)
            {
                // Create an instance of your NotificationHub and call SendNotification
                var notificationHub = new NotificationHub();

                notificationHub.SendNotification(
                    userId: id, // The volunteer's user ID
                    senderUserId: UserId, // The organization ID or admin ID who is sending the notification
                    relatedId: eventId,
                    type: "Declined",
                    content: "You have been declined to participate in the event!"
                );

                // Return JSON response indicating success with redirect URL to event details page
                return Json(new { success = true, message = "Volunteer declined successfully.", redirectUrl = Url.Action("Details", "Organization", new { id = eventId }) });
            }
            else
            {
                // Return JSON response indicating failure with an error message
                return Json(new { success = false, message = errMsg });
            }
        }
        [Authorize]
        public ActionResult VolunteerDetails(int userId)
        {
            var getUserAccount = db.UserAccount.Where(m => m.userId == userId).ToList();
            var getVolunteerInfo = db.VolunteerInfo.Where(m => m.userId == userId).FirstOrDefault();
            var getVolunteerSkills = db.VolunteerSkill.Where(m => m.userId == userId).ToList();
            var getProfile = db.ProfilePicture.Where(m => m.userId == userId).ToList();
            var orgInfo = _organizationManager.GetOrgInfoByUserId(UserId);

            var getUniqueSkill = db.sp_GetSkills(userId).ToList();
            if (getProfile.Count() <= 0)
            {
                var defaultPicture = new ProfilePicture
                {
                    userId = UserId,
                    profilePath = "default.jpg"
                };
                _profilePic.Create(defaultPicture);

                getProfile = db.ProfilePicture.Where(m => m.userId == userId).ToList();
            }
            var listModel = new Lists()
            {
                OrgInfo = orgInfo,
                userAccounts = getUserAccount,
                volunteerInfo = getVolunteerInfo,
                volunteersSkills = getVolunteerSkills,
                uniqueSkill = getUniqueSkill,
                picture = getProfile,
                skills = _skills.GetAll().ToList(),
                volunteersHistories = _volunteerManager.GetVolunteersHistoryByUserId(userId),
                rating = db.Rating.Where(m => m.userId == userId).ToList(),
                orgEventHistory = db.OrgEvents.Where(m => m.userId == userId && m.status == 2).ToList(),
                detailsEventImage = _eventImages.GetAll().ToList()
            };

            return View(listModel);
        }
        [Authorize]
        public ActionResult Reports()
        {
            var orgInfo = _organizationManager.GetOrgInfoByUserId(UserId);
            var profile = _organizationManager.GetProfileByProfileId(UserId);
            var events = _organizationManager.ListOfEvents(UserId);
            var totalDonation = _organizationManager.GetTotalDonationByUserId(UserId);
            var totalVolunteer = _organizationManager.GetTotalVolunteerByUserId(UserId);
            var eventSummary = _organizationManager.GetEventsByUserId(UserId);
            var recentEvents = _organizationManager.GetRecentOngoingEventsByUserId(UserId);
            var totalSkills = _organizationManager.GetAllVolunteerSkills(UserId);
            var userDonated = _organizationManager.GetRecentUserDonationsByUserId(UserId);

            var indexModdel = new Lists()
            {
                OrgInfo = orgInfo,
                listOfEvents = events,
                totalDonation = totalDonation,
                totalVolunteer = totalVolunteer,
                eventSummary = eventSummary,
                recentEvents = recentEvents,
                totalSkills = totalSkills,
                recentDonators = userDonated,
                //profilePic = profile,
            };
            return View(indexModdel);
        }
        [Authorize]
        public ActionResult History()
        {
            var orgInfo = _organizationManager.GetOrgInfoByUserId(UserId);
            //var profile = _organizationManager.GetProfileByProfileId(orgInfo.profileId);
            var eventHistory = _organizationManager.GetEventHistoryByUserId(UserId);


            var indexModdel = new Lists()
            {
                OrgInfo = orgInfo,
                //profilePic = profile,
                orgEventHistory = eventHistory,
            };
            return View(indexModdel);
        }
       
        [HttpPost]
        public ActionResult ExportData()
        {
            var csv = new StringBuilder();

            var events = _organizationManager.ListOfEvents(UserId);
            csv.AppendLine("Events Data");
            csv.AppendLine("Event ID,User ID,Event Title,Event Description,Target Amount,Max Volunteers,Start Date,End Date,location");
            foreach (var evt in events)
            {
                csv.AppendLine($"{evt.Event_Id},{evt.User_Id},{evt.Event_Name},{evt.Description},{evt.Target_Amount},{evt.Maximum_Volunteer},{evt.Start_Date},{evt.End_Date},{evt.location}");
            }
            csv.AppendLine();

            csv.AppendLine("Skill Requirements Data");
            csv.AppendLine("Skill Requirement ID,Event ID,Skill ID,Total Needed");
            foreach (var evt in events)
            {
                var skillRequirements = _organizationManager.listOfSkillRequirement(evt.Event_Id);                
                foreach (var skillReq in skillRequirements)
                {
                    csv.AppendLine($"{skillReq.skillRequirementId},{skillReq.eventId},{skillReq.skillId},{skillReq.totalNeeded}");
                }               
            }

            csv.AppendLine();

            csv.AppendLine("User Donations Data");
            csv.AppendLine("Donation ID,Event ID,User ID,Amount,Donated At");
            foreach (var evt in events)
            {
                var donations = _organizationManager.ListOfUserDonated(evt.Event_Id);
                foreach (var userDonated in donations)
                {
                    csv.AppendLine($"{userDonated.orgUserDonatedId},{userDonated.eventId},{userDonated.userId},{userDonated.amount},{userDonated.donatedAt}");
                }
            }

            csv.AppendLine();

            csv.AppendLine("Volunteers");
            csv.AppendLine("Applicants ID,User ID,Event ID,Status,Skill ID,Applied At");
            foreach (var evt in events)
            {
                var volunteer = _organizationManager.GetVolunteersByEventId(evt.Event_Id);
                foreach (var vol in volunteer)
                {
                    csv.AppendLine($"{vol.applyVolunteerId},{vol.userId},{vol.eventId},{vol.Status},{vol.appliedAt}");
                }
            }

            byte[] fileBytes = Encoding.UTF8.GetBytes(csv.ToString());

            return File(fileBytes, "text/csv", "OrganizationDataExport.csv");
        }
        [HttpGet]
        public JsonResult GetUnreadNotifications()
        {
            int organizationId = UserId;
            var unreadNotifications = db.Notification
                .Where(n => n.userId == organizationId && n.status == 0)
                .OrderByDescending(n => n.createdAt)
                .ToList();

            return Json(unreadNotifications, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult SubmitRatings(int eventId, List<VolunteerRatingData> volunteerRatings)
        {
            string errMsg = string.Empty;

            if (volunteerRatings == null || volunteerRatings.Count == 0)
            {
                return Json(new { success = false, message = "Invalid or incomplete data received." });
            }

            foreach (var ratingData in volunteerRatings)
            {
                var volunteerId = ratingData.VolunteerId;
                var attendanceStatus = ratingData.Attendance; // Attendance is now passed and processed

                foreach (var skillRating in ratingData.SkillRatings)
                {
                    int skillId = skillRating.SkillId;
                    int rating = skillRating.Rating;

                    // Save the rating and attendance
                    var result = _organizationManager.SaveRating(eventId, attendanceStatus, volunteerId, skillId, rating, ref errMsg);
                    if (result != ErrorCode.Success)
                    {
                        return Json(new { success = false, message = "Error saving rating: " + errMsg });
                    }
                }

                var events = _organizationManager.GetEventByEventId(eventId);

                var notification = new Notification
                {
                    userId = volunteerId,
                    senderUserId = UserId,
                    relatedId = eventId,
                    type = "Event",
                    content = $"{events.eventTitle} has ended",
                    broadcast = 0,
                    status = 0,
                    createdAt = DateTime.Now,
                    readAt = null
                };

                db.Notification.Add(notification);
                db.SaveChanges();
            }
            
            var historyResult = _organizationManager.TrasferToHisotry1(eventId, volunteerRatings, ref errMsg);
            if (historyResult != ErrorCode.Success)
            {
                return Json(new { success = false, message = "Error saving to history: " + errMsg });
            }

            return Json(new { success = true, message = "All ratings and attendance submitted successfully." });
        }

        [HttpPost]
        public JsonResult OpenNotification(int notificationId)
        {
            try
            {
                // Fetch the notification from the database
                var notification = db.Notification.FirstOrDefault(n => n.notificationId == notificationId);

                if (notification != null)
                {
                    // Mark the notification as read
                    notification.status = 1;
                    db.SaveChanges();

                    // Optionally, get the URL to redirect the user
                    string redirectUrl = GetRedirectUrlForNotification(notification);

                    return Json(new { success = true, redirectUrl = redirectUrl });
                }
                else
                {
                    return Json(new { success = false, message = "Notification not found." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                // Return an error response
                return Json(new { success = false, message = ex.Message });
            }
        }
        private string GetRedirectUrlForNotification(Notification notification)
        {
            // Logic to determine redirect URL based on notification type or content
            // For example:
            if (notification.type == "New Application")
            {
                return Url.Action("Details", "Organization", new { id = notification.relatedId });
            }
            else if (notification.type == "Donation")
            {
                return Url.Action("Details", "Organization", new { id = notification.relatedId });
            }
            else if (notification.type == "Cancel Application")
            {
                return Url.Action("Details", "Organization", new { id = notification.relatedId });
            }
            else if (notification.type == "Accept Invitation")
            {
                return Url.Action("Details", "Organization", new { id = notification.relatedId });
            }
            else if (notification.type == "Leave Event")
            {
                return Url.Action("Details", "Organization", new { id = notification.relatedId });
            }
            // Default to null if no redirection is needed
            return null;
        }
    }
}