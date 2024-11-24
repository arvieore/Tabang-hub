using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Tabang_Hub.Repository;
using Tabang_Hub.Utils;

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
        public ActionResult VolunteerAccounts()
        {

            var volunteerAccount = _adminManager.GetVolunteerAccounts();

            var indexModel = new Lists()
            {
                volunteerAccounts = volunteerAccount,
            };
            return View(indexModel);
        }
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
            var unreadNotifications = db.Notification
                .Where(n => n.userId == 3 && n.status == 0)
                .OrderByDescending(n => n.createdAt)
                .ToList();

            return Json(unreadNotifications, JsonRequestBehavior.AllowGet);
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
                var user = _adminManager.DeleteUser(userId);
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

            if (skillImage != null && skillImage.ContentLength > 0)
            {
                var directoryPath = Server.MapPath("~/Content/SkillImages");
                Directory.CreateDirectory(directoryPath);

                var fileName = Path.GetFileName(skillImage.FileName);
                var path = Path.Combine(directoryPath, fileName);
                skillImage.SaveAs(path);
                skill.skillImage = fileName;

                string errMsg = string.Empty;
                if (_adminManager.AddSkills(skill, ref errMsg) == ErrorCode.Success)
                {
                    var users = _adminManager.GetAllUser();
                    var skll = _adminManager.GetSkillById(skill.skillId);

                    foreach (var usr in users)
                    {
                        var str = $"New skill has been created named {skll.skillName}!";
                        if (_organizationManager.SentNotif(usr.userId, UserId, UserId, "Add Skill", str, 0, ref ErrorMessage) != ErrorCode.Success)
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

                // Fetch the current skill details from the database
                var existingSkill = _adminManager.GetSkillById(skillId);
                if (existingSkill == null)
                {
                    return Json(new { success = false, message = "Skill not found." });
                }

                // Handle image update if a new image is uploaded
                string imagePath = existingSkill.skillImage; // Keep the current image by default
                if (skillImage != null && skillImage.ContentLength > 0)
                {
                    var directoryPath = Server.MapPath("~/Content/SkillImages/");
                    if (!Directory.Exists(directoryPath))
                    {
                        Directory.CreateDirectory(directoryPath);
                    }

                    var fileName = Path.GetFileName(skillImage.FileName);
                    var path = Path.Combine(directoryPath, fileName);
                    skillImage.SaveAs(path);
                    imagePath = fileName; // Update imagePath to the new file name
                }

                // Update skill in the database
                string errMsg = string.Empty;
                if (_adminManager.EditSkill(skillId, skillName, imagePath, ref errMsg) == ErrorCode.Success)
                {
                    var volunteerSkill = _adminManager.GetVolunteerSkillBySkillId(skillId);

                    foreach (var vol in volunteerSkill)
                    {
                        var sendNotif = _organizationManager.SentNotif((int)vol.userId, UserId, (int)vol.skillId, "Edit Skill", $"The {vol.Skills.skillName} skill has been updated and removed from your skill set.", 0, ref errMsg);
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
                var recentEvents1 = _organizationManager.GetRecentOngoingEventsByUserId((int)organizationId);
                var totalSkills1 = _organizationManager.GetAllVolunteerSkills((int)organizationId);
                var userDonated = _organizationManager.GetRecentUserDonationsByUserId((int)organizationId);
                var allOrgAcc1 = _adminManager.GetOrganizationAccount();

                var indexModdel1 = new Lists()
                {
                    OrgInfo = orgInfo,
                    listOfEvents = events,
                    totalDonation = totalDonation,
                    totalVolunteer = totalVolunteer,
                    eventSummary = eventSummary,
                    recentEvents = recentEvents1,
                    totalSkills = totalSkills1,
                    recentDonators = userDonated,
                    getAllOrgAccounts = allOrgAcc1,                    
                    //profilePic = profile,
                };
                return View(indexModdel1);
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
    }
}