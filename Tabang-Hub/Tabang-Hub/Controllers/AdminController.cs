using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Tabang_Hub.Repository;
using Tabang_Hub.Utils;
using static Tabang_Hub.Utils.Lists;

namespace Tabang_Hub.Controllers
{
    public class AdminController : BaseController
    {
        // GET: Admin
        [Authorize]
        public ActionResult Index()
        {
            var organization = _adminManager.GetOrganizationAccount();
            var volunteers = _adminManager.GetVolunteerAccounts();
            var donated = _adminManager.GetAllDonators();
            var pending = _adminManager.GetPendingOrg();
            var recentDonate = _adminManager.GetRecentDonated();

            var indexModel = new Lists()
            {
                recentOrgAcc = organization,
                volunteerAccounts = volunteers,
                listofUserDonated = donated,
                pendingOrg = pending,
                recentDonators = recentDonate,

            };
            return View(indexModel);
        }
        [Authorize]
        public ActionResult VolunteerAccounts()
        {

            var volunteerAccount = _adminManager.GetVolunteerAccounts();

            var indexModel = new Lists()
            {
                volunteerAccounts = volunteerAccount,
            };
            return View(indexModel);
        }
        [Authorize]
        public ActionResult History(int? organizationId = null)
        {
            var organizations = _adminManager.GetOrganizationAccount();

            if (organizationId != null && organizationId != 0)
            {
                var orgEvents = _adminManager.GetEventsByUserId((int)organizationId);

                var indexModel = new Lists()
                {
                    getAllOrgAccounts = organizations,
                    getAllOrgEvent = orgEvents,
                };

                return View(indexModel);
            }

            // If no specific organization is selected, display all events
            var allEvents = _adminManager.GetAllEvents();

            var allEventsModel = new Lists()
            {
                getAllOrgAccounts = organizations,
                getAllOrgEvent = allEvents,
            };

            return View(allEventsModel);
        }
        [Authorize]
        public ActionResult VolunteerDetails(int userId)
        {
            var getUserAccount = db.UserAccount.Where(m => m.userId == userId).ToList();
            var getVolunteerInfo = db.VolunteerInfo.Where(m => m.userId == userId).FirstOrDefault();
            var getVolunteerSkills = db.VolunteerSkill.Where(m => m.userId == userId).ToList();
            var getProfile = db.ProfilePicture.Where(m => m.userId == userId).ToList();

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
        public ActionResult OrgProfile(int userId)
        {
            var userProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();
            var getVolunteerInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList();

            var getOrgUserId = db.OrgEvents.Where(m => m.eventId == userId).Select(m => m.userId).FirstOrDefault();
            var getOrgInfo = _organizationManager.GetOrgInfoByUserId(userId);

            var orgEvents = _organizationManager.GetOrgEventsByUserId(userId);
            var orgImage = new List<OrgEventImage>();
            foreach (var image in orgEvents)
            {
                var orgEvenImage = _organizationManager.GetEventImageByEventId(image.eventId);

                orgImage.Add(orgEvenImage);
            }

            var filteredEvent = new List<vw_ListOfEvent>();

            var indexModel = new Lists()
            {
                picture = userProfile,
                volunteersInfo = getVolunteerInfo,
                OrgInfo = getOrgInfo,
                detailsEventImage = orgImage,
                getAllOrgEvent = orgEvents,
                listOfEvents = filteredEvent.OrderByDescending(m => m.Event_Id).ToList(),
            };

            return View(indexModel);
        }

        [HttpGet]
        public JsonResult GetUnreadNotifications()
        {
            int organizationId = UserId;
            var notifications = db.Notification
               .Where(n => n.userId == organizationId)
               .OrderBy(n => n.status) // Unread (status = 0) first
               .ThenByDescending(n => n.createdAt)
               .Select(n => new
               {
                   n.notificationId,
                   n.content,
                   n.status, // 0 for unread, 1 for read
                   n.createdAt
               })
               .ToList();

            return Json(notifications, JsonRequestBehavior.AllowGet);
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
        [Authorize]
        public ActionResult OrganizationAccounts()
        {

            var organizationAccount = _adminManager.GetOrganizationAccounts();

            var indexModel = new Lists()
            {
                organizationAccounts = organizationAccount,
            };
            return View(indexModel);
        }
        [Authorize]
        public ActionResult UserDetails(int userId)
        {
            var getUserAccount = db.UserAccount.Where(m => m.userId == userId).ToList();
            var getVolunteerInfo = db.VolunteerInfo.Where(m => m.userId == userId).ToList();
            var getVolunteerSkills = db.VolunteerSkill.Where(m => m.userId == userId).ToList();
            var getProfile = db.ProfilePicture.Where(m => m.userId == userId).ToList();

            var getUniqueSkill = db.sp_GetSkills(UserId).ToList();

            var listModel = new Lists()
            {
                userAccounts = getUserAccount,
                volunteersInfo = getVolunteerInfo,
                volunteersSkills = getVolunteerSkills,
                uniqueSkill = getUniqueSkill,
                picture = getProfile
            };

            return View(listModel);
        }
        [HttpPost]
        public ActionResult Register(UserAccount u, VolunteerInfo v, UserRoles r, String ConfirmPass)
        {
            u.status = 0;
            u.roleId = 1;

            try
            {
                if (!u.password.Equals(ConfirmPass))
                {
                    ModelState.AddModelError(String.Empty, "Password not match");
                    return RedirectToAction("VolunteerAccounts");
                }

                if (_userManager.Register(u, v, r, ref ErrorMessage) != ErrorCode.Success)
                {
                    ModelState.AddModelError(String.Empty, ErrorMessage);

                    return RedirectToAction("VolunteerAccounts");
                }
            }
            catch (Exception ex)
            {
                TempData["msg"] = $"Error! " + ex.Message;
                return RedirectToAction("VolunteerAccounts");
            }

            return RedirectToAction("VolunteerAccounts");
        }
        [HttpPost]
        public ActionResult RegisterOrg(UserAccount u, OrgInfo o, OrgValidation ov, UserRoles r, HttpPostedFileBase picture1, HttpPostedFileBase picture2)
        {
            if (picture1 != null && picture1.ContentLength > 0)
            {
                var inputFileName = Path.GetFileName(picture1.FileName);
                var serverSavePath = Path.Combine(Server.MapPath("~/Content/IdPicture/"), inputFileName);

                if (!Directory.Exists(Server.MapPath("~/UploadedFiles/")))
                    Directory.CreateDirectory(Server.MapPath("~/Content/IdPicture/"));

                picture1.SaveAs(serverSavePath);

                ov.idPicture1 = inputFileName;
            }
            if (picture2 != null && picture2.ContentLength > 0)
            {
                var inputFileName = Path.GetFileName(picture2.FileName);
                var serverSavePath = Path.Combine(Server.MapPath("~/Content/IdPicture/"), inputFileName);

                if (!Directory.Exists(Server.MapPath("~/UploadedFiles/")))
                    Directory.CreateDirectory(Server.MapPath("~/Content/IdPicture/"));

                picture2.SaveAs(serverSavePath);

                ov.idPicture2 = inputFileName;
            }
            if (_userManager.OrgRegister(u, o, ov, r, ref ErrorMessage) != ErrorCode.Success)
            {
                ModelState.AddModelError(String.Empty, ErrorMessage);
            }
            return RedirectToAction("OrganizationAccounts");
        }
        [HttpPost]
        public ActionResult DeleteUser(int userId)
        {
            try
            {
                var user = _adminManager.DeleteUser(userId, ref ErrorMessage);
                if (user != ErrorCode.Success)
                {
                    return Json(new { success = false, message = "User not found" });

                }
                else
                {

                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [HttpPost]
        public ActionResult DeleteOrg(int userId)
        {
            try
            {
                var user = _adminManager.DeleteOrganization(userId);
                if (user != ErrorCode.Success)
                {
                    return Json(new { success = false, message = "User not found" });

                }
                else
                {

                    return Json(new { success = true });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
        [Authorize]
        public ActionResult ManageSkill()
        {
            var skills = _adminManager.GetSkills();

            var indexModel = new Lists()
            {
                allSkill = skills,
            };
            return View(indexModel);
        }
        [HttpPost]
        public JsonResult AddSkills(Skills skill, HttpPostedFileBase skillImage)
        {
            // Check if the skill already exists in the database
            var existingSkill = _adminManager.GetSkillByName(skill.skillName);
            if (existingSkill != null)
            {
                return Json(new { success = false, message = "Skill already exists." });
            }

            // Allowed image types: PNG and JPEG
            var allowedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
    {
        "image/png",
        "image/jpeg"
    };

            const int targetWidth = 800;  // Desired width
            const int targetHeight = 600; // Desired height

            if (skillImage != null && skillImage.ContentLength > 0)
            {
                if (!allowedTypes.Contains(skillImage.ContentType))
                {
                    return Json(new { success = false, message = "Only PNG and JPEG images are allowed." });
                }

                var directoryPath = Server.MapPath("~/Content/SkillImages");
                Directory.CreateDirectory(directoryPath);

                var fileName = Path.GetFileName(skillImage.FileName);
                var path = Path.Combine(directoryPath, fileName);

                // Resize and save the image
                using (var originalImage = System.Drawing.Image.FromStream(skillImage.InputStream))
                {
                    using (var resizedImage = new Bitmap(targetWidth, targetHeight))
                    {
                        using (var graphics = Graphics.FromImage(resizedImage))
                        {
                            graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                            graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                            // Draw resized image
                            graphics.DrawImage(originalImage, 0, 0, targetWidth, targetHeight);
                        }

                        if (skillImage.ContentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase))
                        {
                            // Compress and save as JPEG
                            var qualityParam = new System.Drawing.Imaging.EncoderParameter(
                                System.Drawing.Imaging.Encoder.Quality, 75L); // Adjust compression as needed

                            var jpegCodec = System.Drawing.Imaging.ImageCodecInfo
                                .GetImageDecoders()
                                .FirstOrDefault(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);

                            if (jpegCodec != null)
                            {
                                var encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
                                encoderParams.Param[0] = qualityParam;
                                resizedImage.Save(path, jpegCodec, encoderParams);
                            }
                            else
                            {
                                // Fallback if JPEG encoder not found
                                resizedImage.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
                            }
                        }
                        else
                        {
                            // Save as PNG
                            resizedImage.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                        }
                    }
                }

                skill.skillImage = fileName;

                string errMsg = string.Empty;
                if (_adminManager.AddSkills(skill, ref errMsg) == ErrorCode.Success)
                {
                    var users = _adminManager.GetAllUser();
                    var skll = _adminManager.GetSkillById(skill.skillId);

                    foreach (var usr in users)
                    {
                        var str = $"New skill has been created named {skll.skillName}!";
                        if (_organizationManager.SentNotif(usr.userId, UserId, UserId, "Add Skill", str, 0, ref errMsg) != ErrorCode.Success)
                        {
                            return Json(new { success = false, message = "Failed to send notifications." });
                        }
                    }

                    return Json(new { success = true, message = "Skill added successfully!" });
                }
                else
                {
                    return Json(new { success = false, message = errMsg });
                }
            }

            return Json(new { success = false, message = "An error occurred while adding the skill." });
        }

        [HttpPost]
        public JsonResult EditSkill(int skillId, string skillName, HttpPostedFileBase skillImage)
        {
            try
            {
                // Validate inputs
                if (string.IsNullOrWhiteSpace(skillName))
                {
                    return Json(new { success = false, message = "Skill name cannot be empty." });
                }

                var orgEvents = _adminManager.GetAllEvents();

                foreach (var evenItems in orgEvents)
                {
                    var orgSkillReq = _organizationManager.listOfSkillRequirement(evenItems.eventId);

                    foreach (var reqItem in orgSkillReq)
                    {
                        if (reqItem.skillId == skillId)
                        {
                            return Json(new { success = false, message = "You cannot edit a skill that is currently in use." });
                        }
                    }
                }

                // Fetch the current skill details from the database
                var existingSkill = _adminManager.GetSkillById(skillId);
                if (existingSkill == null)
                {
                    return Json(new { success = false, message = "Skill not found." });
                }

                // Allowed image types: PNG and JPEG
                var allowedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "image/png",
            "image/jpeg"
        };

                const int targetWidth = 800;  // Desired width
                const int targetHeight = 600; // Desired height
                string imagePath = existingSkill.skillImage; // Keep the current image by default

                // Handle image update if a new image is uploaded
                if (skillImage != null && skillImage.ContentLength > 0)
                {
                    if (!allowedTypes.Contains(skillImage.ContentType))
                    {
                        return Json(new { success = false, message = "Only PNG and JPEG images are allowed." });
                    }

                    var directoryPath = Server.MapPath("~/Content/SkillImages/");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    var fileName = Path.GetFileName(skillImage.FileName);
                    var path = Path.Combine(directoryPath, fileName);

                    // Resize and save the image
                    using (var originalImage = System.Drawing.Image.FromStream(skillImage.InputStream))
                    {
                        using (var resizedImage = new Bitmap(targetWidth, targetHeight))
                        {
                            using (var graphics = Graphics.FromImage(resizedImage))
                            {
                                graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                                graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

                                // Draw resized image
                                graphics.DrawImage(originalImage, 0, 0, targetWidth, targetHeight);
                            }

                            if (skillImage.ContentType.Equals("image/jpeg", StringComparison.OrdinalIgnoreCase))
                            {
                                // Compress and save as JPEG
                                var qualityParam = new System.Drawing.Imaging.EncoderParameter(
                                    System.Drawing.Imaging.Encoder.Quality, 75L);

                                var jpegCodec = System.Drawing.Imaging.ImageCodecInfo
                                    .GetImageDecoders()
                                    .FirstOrDefault(c => c.FormatID == System.Drawing.Imaging.ImageFormat.Jpeg.Guid);

                                if (jpegCodec != null)
                                {
                                    var encoderParams = new System.Drawing.Imaging.EncoderParameters(1);
                                    encoderParams.Param[0] = qualityParam;
                                    resizedImage.Save(path, jpegCodec, encoderParams);
                                }
                                else
                                {
                                    // Fallback if JPEG encoder not found
                                    resizedImage.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
                                }
                            }
                            else
                            {
                                // Save as PNG
                                resizedImage.Save(path, System.Drawing.Imaging.ImageFormat.Png);
                            }
                        }
                    }

                    imagePath = fileName; // Update imagePath to the new file name
                }

                // Update skill in the database
                string errMsg = string.Empty;
                if (_adminManager.EditSkill(skillId, skillName, imagePath, ref errMsg) == ErrorCode.Success)
                {
                    var volunteerSkill = _adminManager.GetVolunteerSkillBySkillId(skillId);

                    foreach (var vol in volunteerSkill)
                    {
                        _organizationManager.SentNotif((int)vol.userId, UserId, (int)vol.skillId, "Edit Skill", $"The {vol.Skills.skillName} skill has been updated and removed from your skill set.", 0, ref errMsg);
                    }

                    return Json(new { success = true, message = "Skill updated successfully." });
                }
                else
                {
                    return Json(new { success = false, message = errMsg });
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging purposes
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = "An error occurred while updating the skill. Please try again later." });
            }
        }
        // Action to handle the deletion of a skill
        [HttpPost]
        public JsonResult DeleteSkill(int skillId)
        {
            string errMsg = string.Empty;
            var skill = _adminManager.GetSkillById(skillId);

            var orgEvents = _adminManager.GetAllEvents();

            foreach (var evenItems in orgEvents)
            {
                var orgSkillReq = _organizationManager.listOfSkillRequirement(evenItems.eventId);

                foreach (var reqItem in orgSkillReq)
                {
                    if (reqItem.skillId == skillId)
                    {
                        return Json(new { success = false, message = "You cannot delete a skill that is currently in use." });
                    }
                }
            }

            if (_adminManager.DeleteSkill(skillId) == ErrorCode.Success)
            {
                var users = _adminManager.GetAllUser();


                foreach (var usr in users)
                {
                    var str = $"The {skill.skillName} Skill has been deleted!";
                    if (_organizationManager.SentNotif(usr.userId, UserId, UserId, "Delete Skill", str, 0, ref ErrorMessage) != ErrorCode.Success)
                    {
                        return Json(new { success = false, message = errMsg });
                    }
                }
                return Json(new { success = true, message = "Skill deleted successfully." });
            }

            return Json(new { success = false, message = errMsg });
        }
        [HttpPost]
        public ActionResult Deactivate(int userId)
        {
            string errMsg = string.Empty;
            if (_adminManager.DeactivateAccount(userId, ref errMsg) != ErrorCode.Success)
            {
                return Json(new { success = false, message = errMsg });
            }
            return Json(new { success = true });
        }
        [HttpPost]
        public JsonResult Reactivate(int userId)
        {
            string errMsg = string.Empty;
            if (_adminManager.ReactivateAccount(userId, ref errMsg) != ErrorCode.Success)
            {
                return Json(new { success = false, message = errMsg });
            }
            return Json(new { success = true });
        }
        [Authorize]
        public ActionResult Reports(int? organizationId = null)
        {
            if (organizationId != null && organizationId != 0)
            {
                var orgInfo = _organizationManager.GetOrgInfoByUserId(organizationId);
                var profile = _organizationManager.GetProfileByProfileId(organizationId);
                var events = _organizationManager.ListOfEvents((int)organizationId);
                var totalDonation = _organizationManager.GetTotalDonationByUserId((int)organizationId);
                var totalVolunteer = _organizationManager.GetTotalVolunteerByUserId((int)organizationId);
                var eventSummary = _organizationManager.GetEventsByUserId((int)organizationId);
                var donationSummary = _organizationManager.GetDonationEventSummaryByUserId((int)organizationId);
                var recentEvents1 = _organizationManager.GetRecentOngoingEventsByUserId((int)organizationId);
                var totalSkills1 = _organizationManager.GetAllVolunteerSkills((int)organizationId);
                var userDonated = _organizationManager.GetRecentUserDonationsByUserId((int)organizationId);
                var eventHistory = _organizationManager.GetEventHistoryByUserId((int)organizationId);
                var listofUserDonated = _organizationManager.ListOfUserDonated((int)organizationId);
                var allOrgAcc1 = _adminManager.GetOrganizationAccount();
                var donationList = _organizationManager.GetListOfDonationEventByUserId((int)organizationId);
                var listOfDoneEvents = _organizationManager.ListOfDoneEvents((int)organizationId);
                var listOfvlntr = new List<Volunteers>();

                // Dictionary to accumulate volunteer participation stats
                var volunteerStats = new Dictionary<int, TopVolunteer>();
                var volunteerStats1 = new Dictionary<int, TopVolunteer>();
                var donatorStats = new Dictionary<int, TopDonators>();

                foreach (var evnt in events)
                {
                    var volunteers = _organizationManager.GetVolunteersByEventId(evnt.Event_Id);
                    var evntImg = _organizationManager.GetEventImageByEventId(evnt.Event_Id);
                    var userDntd = _organizationManager.ListOfUserDonated(evnt.Event_Id);
                    listOfvlntr.AddRange(volunteers);

                    foreach (var vlntr in volunteers)
                    {
                        if (vlntr.Status != 0)
                        {
                            if (volunteerStats.ContainsKey((int)vlntr.userId) && vlntr.Status == 1)
                            {
                                // Increment participation count if volunteer exists
                                volunteerStats[(int)vlntr.userId].TotalEventsParticipated++;
                                volunteerStats[(int)vlntr.userId].EventIds.Add(evnt.Event_Id); // Add event ID
                                volunteerStats[(int)vlntr.userId].EventImages.Add(evntImg.eventImage); // Add event image
                            }
                            else
                            {
                                var volName = _organizationManager.GetVolunteerInfoByUserId((int)vlntr.userId);
                                // Add new volunteer to the dictionary
                                volunteerStats[(int)vlntr.userId] = new TopVolunteer
                                {
                                    VolunteerId = (int)vlntr.userId,
                                    EventIds = new List<int> { evnt.Event_Id }, // Initialize with first event ID
                                    EventImages = new List<string> { evntImg.eventImage }, // Initialize with first event image
                                    Name = volName.lName + ", " + volName.fName,
                                    TotalEventsParticipated = 1
                                };
                            }
                        }
                    }

                    foreach (var dnt in userDntd)
                    {
                        if (donatorStats.ContainsKey((int)dnt.userId))
                        {
                            donatorStats[(int)dnt.userId].totalAmountDonated += (decimal)dnt.amount;

                            // Update donation for the specific event
                            var existingEventDonation = donatorStats[(int)dnt.userId].EventDonations
                                .FirstOrDefault(ed => ed.EventId == evnt.Event_Id);

                            if (existingEventDonation != null)
                            {
                                existingEventDonation.AmountDonated += (decimal)dnt.amount;
                            }
                            else
                            {
                                donatorStats[(int)dnt.userId].EventDonations.Add(new EventDonation
                                {
                                    EventId = evnt.Event_Id,
                                    EventName = evnt.Event_Name,
                                    EventImage = evntImg.eventImage,
                                    AmountDonated = (decimal)dnt.amount
                                });
                            }
                        }
                        else
                        {
                            var volName = _organizationManager.GetVolunteerInfoByUserId((int)dnt.userId);

                            donatorStats[(int)dnt.userId] = new TopDonators
                            {
                                donatorsId = (int)dnt.userId,
                                Name = volName.lName + ", " + volName.fName,
                                totalAmountDonated = (decimal)dnt.amount,
                                EventDonations = new List<EventDonation>
                            {
                                new EventDonation
                                {
                                    EventId = evnt.Event_Id,
                                    EventName = evnt.Event_Name,
                                    EventImage = evntImg.eventImage,
                                    AmountDonated = (decimal)dnt.amount
                                }
                            }
                            };
                        }
                    }
                }

                foreach (var doneEventsItem in listOfDoneEvents)
                {
                    // Retrieve volunteers for this event
                    var volunteerItems = _organizationManager.GetTotalVolunteerHistoryByEventId(doneEventsItem.eventId);

                    // Retrieve event image object
                    var evntImg = _organizationManager.GetEventImageByEventId(doneEventsItem.eventId);

                    // Retrieve users who donated (not currently used in logic)
                    var userDntd = _organizationManager.ListOfUserDonated(doneEventsItem.eventId);

                    foreach (var volItem in volunteerItems)
                    {
                        // Only proceed if volunteer's status is 1
                        if (volItem.attended == 1)
                        {
                            int userId = (int)volItem.userId;

                            // Check if this volunteer is already in the dictionary
                            if (!volunteerStats1.TryGetValue(userId, out var topVol))
                            {
                                // If not, fetch volunteer info and create a new TopVolunteer entry
                                var volInfo = _organizationManager.GetVolunteerInfoByUserId(userId);
                                topVol = new TopVolunteer
                                {
                                    VolunteerId = userId,
                                    Name = volInfo?.fName ?? "Unknown"
                                };
                                volunteerStats1[userId] = topVol;
                            }

                            // Update this volunteer's participation stats
                            volunteerStats1[userId].TotalEventsParticipated++;
                            volunteerStats1[userId].EventIds.Add(doneEventsItem.eventId);

                            // If an event image is available, store it
                            // Assuming evntImg.eventImage is the string property holding the image URL or path
                            if (evntImg != null && !string.IsNullOrEmpty(evntImg.eventImage))
                            {
                                volunteerStats1[userId].EventImages.Add(evntImg.eventImage);
                            }
                        }
                    }
                }

                // After processing all events, sort volunteers by their participation (descending) and take top 5
                var top5Volunteers = volunteerStats1.Values
                    .OrderByDescending(v => v.TotalEventsParticipated)
                    .Take(5)
                    .ToList();

                // Get top 5 volunteers by total events participated
                var topVolunteersList = volunteerStats.Values
                    .OrderByDescending(v => v.TotalEventsParticipated)
                    .Take(5)
                    .ToList();

                // Get top 5 volunteers by total events participated
                var topDonators = donatorStats.Values
                    .OrderByDescending(v => v.totalAmountDonated)
                    .Take(5)
                    .ToList();

                var indexModel1 = new Lists()
                {
                    OrgInfo = orgInfo,
                    listOfEvents = events,
                    totalDonation = totalDonation,
                    totalVolunteer = totalVolunteer,
                    eventSummary = eventSummary,
                    recentEvents = recentEvents1,
                    donationSummary = donationSummary,
                    totalSkills = totalSkills1,
                    orgEventHistory = eventHistory,
                    recentDonators = userDonated,
                    topVolunteers = top5Volunteers, // Assign the top volunteers list here
                    volunteers = listOfvlntr,
                    topDonators = topDonators,
                    listOfDonationEvent = donationList,
                    listofUserDonated = listofUserDonated,
                    getAllOrgAccounts = allOrgAcc1,
                };
                return View(indexModel1);
            }
            var allOrgEvents = _adminManager.GetAllEvents();
            var allOrgAcc = _adminManager.GetOrganizationAccount();
            var allVolunteerAccounts = _adminManager.GetVolunteerAccount();
            var allEvents = _adminManager.AllEventSummary();
            var recentEvents = _adminManager.GetAllRecentOrgEvents();
            var totalSkills = _adminManager.GetAllVolunteerSkills();

            var indexModdel = new Lists()
            {
                getAllOrgEvents = allOrgEvents,
                getAllOrgAccounts = allOrgAcc,
                getAllVolunteerAccounts = allVolunteerAccounts,
                allEventSummary = allEvents,
                recentEvents = recentEvents,
                totalSkills = totalSkills,
                recentOrgAcc = _adminManager.GetRecentOrgAccount(),
                //profilePic = profile,
            };
            return View(indexModdel);
        }
        public ActionResult ToConfirm(int userId)
        {
            var userAcc = _adminManager.GetUserById(userId);
            var orgInfo = _adminManager.GetOrgInfoByUserId(userAcc.userId);
            var validation = _adminManager.GetOrgValidationsByUserId(userAcc.userId);

            var indexModel = new Lists()
            {
                userAccount = userAcc,
                OrgInfo = orgInfo,
                orgValidation = validation
            };
            return View(indexModel);
        }
        [HttpPost]
        public JsonResult Approve(int userId)
        {
            var organization = db.UserAccount.FirstOrDefault(o => o.userId == userId);

            if (organization == null)
            {
                return Json(new { success = false, message = "Organization not found." });
            }

            organization.status = 1; // Approved status
            db.SaveChanges();

            // Email notification
            string subject = "Organization Approval Notification";
            string body = $@"
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f0f8ff;
            color: #333;
        }}
        .container {{
            width: 100%;
            max-width: 600px;
            margin: 0 auto;
            padding: 20px;
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
            overflow: hidden;
        }}
        .header {{
            background-color: #28a745;
            padding: 10px 20px;
            color: white;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 24px;
            font-weight: bold;
        }}
        .content {{
            padding: 20px;
            font-size: 16px;
            line-height: 1.6;
        }}
        .button {{
            display: block;
            width: fit-content;
            margin: 20px auto;
            padding: 10px 20px;
            background-color: #28a745;
            color: #ffffff;
            border-radius: 5px;
            text-align: center;
            text-decoration: none;
            font-size: 18px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Approval Confirmation</h1>
        </div>
        <div class='content'>
            <p>Dear {organization.email},</p>
            <p>We are pleased to inform you that your organization account has been approved. You can now access the system using the link below:</p>
            <a class='button' href='https://localhost:44330/'>Login</a>
            <p>Thank you for being a part of our community.</p>
            <p>Sincerely,<br>The Tabang Hub Team</p>
        </div>
    </div>
</body>
</html>";

            // Send email
            MailManager sendEmail = new MailManager();
            string errorResponse = "";
            bool isEmailSent = sendEmail.SendEmail(organization.email, subject, body, ref errorResponse);

            if (!isEmailSent)
            {
                return Json(new { success = false, message = "Approval successful, but email notification failed." });
            }

            return Json(new { success = true, message = "The organization account has been approved." });
        }
        [HttpPost]
        public JsonResult Reject(int userId)
        {
            var organization = db.UserAccount.FirstOrDefault(o => o.userId == userId);
            var orgInfo = db.OrgInfo.FirstOrDefault(m => m.userId == userId);

            if (organization == null)
            {
                return Json(new { success = false, message = "Organization not found." });
            }

            if (orgInfo != null)
            {
                db.OrgInfo.Remove(orgInfo);
            }

            db.UserAccount.Remove(organization);
            db.SaveChanges();

            // Email notification
            string subject = "Organization Rejection Notification";
            string body = $@"
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #ffefef;
            color: #333;
        }}
        .container {{
            ...
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Rejection Notice</h1>
        </div>
        <div class='content'>
            <p>Dear {organization.email},</p>
            <p>We regret to inform you that your organization account has not been approved.</p>
            <p>Sincerely,<br>The Tabang Hub Team</p>
        </div>
    </div>
</body>
</html>";

            // Send email
            MailManager sendEmail = new MailManager();
            string errorResponse = "";
            bool isEmailSent = sendEmail.SendEmail(organization.email, subject, body, ref errorResponse);

            if (!isEmailSent)
            {
                return Json(new { success = false, message = "Rejection successful, but email notification failed." });
            }

            return Json(new { success = true, message = "The organization account has been rejected." });
        }

        private string GetRedirectUrlForNotification(Notification notification)
        {
            // Logic to determine redirect URL based on notification type or content
            // For example:
            if (notification.type == "Registration")
            {
                return Url.Action("OrganizationAccounts", "Admin", new { id = notification.relatedId });
            }

            // Default to null if no redirection is needed
            return null;
        }

        [Authorize]
        [HttpPost]
        public ActionResult ExportReportsToPdf(int userId)
        {
            var orgInfo = _organizationManager.GetOrgInfoByUserId(userId);
            var events = _organizationManager.ListOfEvents(userId);
            var totalDonation = _organizationManager.GetTotalDonationByUserId(userId);
            var totalVolunteer = _organizationManager.GetTotalVolunteerByUserId(userId);
            var eventSummary = _organizationManager.GetEventsByUserId(userId);
            var totalSkills = _organizationManager.GetAllVolunteerSkills(userId).OrderByDescending(x => x.Value).Take(5).ToList();

            // Calculate Top 5 Volunteers
            var volunteerParticipation = new Dictionary<int, (string Name, int Count)>();

            foreach (var evnt in events)
            {
                var volunteers = _organizationManager.GetVolunteerHistoryByEventId(evnt.Event_Id);

                foreach (var volunteer in volunteers)
                {
                    if (volunteer.attended != 0) // Check if the volunteer is active
                    {
                        if (volunteerParticipation.ContainsKey((int)volunteer.userId))
                        {
                            volunteerParticipation[(int)volunteer.userId] = (
                                volunteerParticipation[(int)volunteer.userId].Name,
                                volunteerParticipation[(int)volunteer.userId].Count + 1
                            );
                        }
                        else
                        {
                            var volInfo = _organizationManager.GetVolunteerInfoByUserId((int)volunteer.userId);
                            volunteerParticipation[(int)volunteer.userId] = (
                                $"{volInfo.fName} {volInfo.lName}",
                                1
                            );
                        }
                    }
                }
            }

            var topVolunteers = volunteerParticipation
                .OrderByDescending(v => v.Value.Count)
                .Take(5)
                .Select(v => new { Name = v.Value.Name, TotalEventsParticipated = v.Value.Count })
                .ToList();

            // Create a MemoryStream
            var ms = new MemoryStream();

            // Use PdfWriter and prevent it from closing the MemoryStream
            var pdfDoc = new iTextSharp.text.Document(iTextSharp.text.PageSize.A4, 25, 25, 30, 30);
            var writer = iTextSharp.text.pdf.PdfWriter.GetInstance(pdfDoc, ms);
            writer.CloseStream = false;

            try
            {
                pdfDoc.Open();

                // Add Logo
                string logoPath = Server.MapPath("~/Content/images/tabanghub3.png");
                if (System.IO.File.Exists(logoPath))
                {
                    iTextSharp.text.Image logo = iTextSharp.text.Image.GetInstance(logoPath);
                    logo.ScaleAbsolute(100, 100);
                    logo.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                    pdfDoc.Add(logo);
                }

                // Add Report Title
                iTextSharp.text.Font titleFont = iTextSharp.text.FontFactory.GetFont("Arial", 18, iTextSharp.text.Font.BOLD);
                pdfDoc.Add(new iTextSharp.text.Paragraph("Reports Summary", titleFont)
                {
                    Alignment = iTextSharp.text.Element.ALIGN_CENTER,
                    SpacingAfter = 20
                });

                // Add Overall Statistics
                iTextSharp.text.Font subTitleFont = iTextSharp.text.FontFactory.GetFont("Arial", 14, iTextSharp.text.Font.BOLD);
                pdfDoc.Add(new iTextSharp.text.Paragraph("Overall Statistics", subTitleFont) { SpacingBefore = 10, SpacingAfter = 10 });

                pdfDoc.Add(new iTextSharp.text.Paragraph($"Overall Events: {events.Count()}"));
                pdfDoc.Add(new iTextSharp.text.Paragraph($"Overall Donations: ₱{totalDonation:N}"));
                pdfDoc.Add(new iTextSharp.text.Paragraph($"Overall Volunteers: {totalVolunteer}"));
                pdfDoc.Add(new iTextSharp.text.Paragraph("\n"));

                // Add Monthly Event Summary Table
                pdfDoc.Add(new iTextSharp.text.Paragraph("Monthly Event Summary", subTitleFont) { SpacingBefore = 10, SpacingAfter = 10 });
                iTextSharp.text.pdf.PdfPTable summaryTable = new iTextSharp.text.pdf.PdfPTable(2) { WidthPercentage = 100 };
                summaryTable.AddCell(new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("Month", iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD))) { BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY });
                summaryTable.AddCell(new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("Event Count", iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD))) { BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY });

                foreach (var month in eventSummary)
                {
                    summaryTable.AddCell(CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month.Key));
                    summaryTable.AddCell(month.Value.ToString());
                }
                pdfDoc.Add(summaryTable);

                pdfDoc.Add(new iTextSharp.text.Paragraph("\n"));

                // Add Top 5 Volunteers Table
                pdfDoc.Add(new iTextSharp.text.Paragraph("Top 5 Volunteers", subTitleFont) { SpacingBefore = 10, SpacingAfter = 10 });
                iTextSharp.text.pdf.PdfPTable volunteerTable = new iTextSharp.text.pdf.PdfPTable(2) { WidthPercentage = 100 };
                volunteerTable.AddCell(new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("Name", iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD))) { BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY });
                volunteerTable.AddCell(new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("Total Events Participated", iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD))) { BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY });

                foreach (var volunteer in topVolunteers)
                {
                    volunteerTable.AddCell(volunteer.Name);
                    volunteerTable.AddCell(volunteer.TotalEventsParticipated.ToString());
                }
                pdfDoc.Add(volunteerTable);

                pdfDoc.Add(new iTextSharp.text.Paragraph("\n"));

                // Add Top 5 Skills Table
                pdfDoc.Add(new iTextSharp.text.Paragraph("Top 5 Skills", subTitleFont) { SpacingBefore = 10, SpacingAfter = 10 });
                iTextSharp.text.pdf.PdfPTable skillTable = new iTextSharp.text.pdf.PdfPTable(2) { WidthPercentage = 100 };
                skillTable.AddCell(new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("Skill", iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD))) { BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY });
                skillTable.AddCell(new iTextSharp.text.pdf.PdfPCell(new iTextSharp.text.Phrase("Count", iTextSharp.text.FontFactory.GetFont("Arial", 12, iTextSharp.text.Font.BOLD))) { BackgroundColor = iTextSharp.text.BaseColor.LIGHT_GRAY });

                foreach (var skill in totalSkills)
                {
                    skillTable.AddCell(skill.Key); // Skill name
                    skillTable.AddCell(skill.Value.ToString()); // Skill count
                }
                pdfDoc.Add(skillTable);

                pdfDoc.Close();

                // Reset MemoryStream position
                ms.Position = 0;

                // Return the file
                return File(ms.ToArray(), "application/pdf", "ReportsSummary.pdf");
            }
            finally
            {
                writer.Dispose();
                pdfDoc.Dispose();
            }
        }
    }
}