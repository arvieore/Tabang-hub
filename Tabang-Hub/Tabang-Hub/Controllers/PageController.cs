using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using Tabang_Hub.Repository;
using Tabang_Hub.Utils;
using System.Collections.ObjectModel;
using System.Data.Entity;
using static Tabang_Hub.Utils.Lists;

namespace Tabang_Hub.Controllers
{
    [Authorize(Roles = "Volunteer,Organization,Admin")]
    public class PageController : BaseController
    {
        // GET: Page
        [Authorize]
        public async Task<ActionResult> Index()
        {
            var user = _userManager.GetUserByEmail(User.Identity.Name);
            if (user != null)
            {
                switch (user.roleId)
                {
                    case 1:

                        var getInfo = db.VolunteerInfo.Where(m => m.userId == UserId).ToList();
                        var getVolunteerSkills = db.VolunteerSkill.Where(m => m.userId == UserId).ToList();
                        var getSkills = _skills.GetAll().ToList();
                        var getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();
                        var getOrgInfo = _orgInfo.GetAll().ToList();
                        _volunteerManager.CheckVolunteerEventEndByUserId(UserId);
                        var getVolunteers = _volunteers.GetAll().ToList();
                        var orgEventsSelectId = _orgEvents.GetAll().Where(m => m.dateEnd >= DateTime.Now).Select(m => m.eventId).ToList();

                        var donationEvents = _userManager.ListOfOngoinDonationEvent();
                        var donationList = new List<Donation>();

                        foreach (var donationEvent in donationEvents)
                        {
                            var donationImages = _userManager.GetDonationEventImageByEventId(donationEvent.donationEventId);
                            foreach (var donationImage in donationImages)
                            {
                                donationList.Add(new Donation
                                {
                                    Event = new List<DonationEvent> { donationEvent },
                                    Image = donationImages
                                });
                            }
                        }


                        if (getProfile.Count() <= 0)
                        {
                            var defaultPicture = new ProfilePicture
                            {
                                userId = UserId,
                                profilePath = "default.jpg"
                            };
                            _profilePic.Create(defaultPicture);

                            getProfile = db.ProfilePicture.Where(m => m.userId == UserId).ToList();
                        }


                        var getUserDonated = new List<UserDonated>();
                        foreach (var eventId in orgEventsSelectId)
                        {           
                            getUserDonated = _userDonated.GetAll().Where(m => m.eventId == eventId && m.userId == UserId).ToList();
                        }
                        if (getVolunteerSkills.Count() != 0)
                        {
                            var recommendedEvents = await _volunteerManager.RunRecommendation(UserId);

                            var filteredEvent = new List<vw_ListOfEvent>();

                            foreach (var recommendedEvent in recommendedEvents)
                            {
                                var isExcluded = _volunteers.GetAll().Any(v => v.eventId == recommendedEvent.EventID && v.Status == 1);
                                if (!isExcluded)
                                {
                                    var matchedEvents = _listsOfEvent.GetAll().Where(m => m.Event_Id == recommendedEvent.EventID).ToList();
                                    filteredEvent.AddRange(matchedEvents);
                                }
                            }


                            var indexModel = new Lists()
                            {
                                volunteersInfo = getInfo,
                                volunteersSkills = getVolunteerSkills,
                                skills = getSkills,
                                picture = getProfile,
                                listOfEvents = filteredEvent.OrderByDescending(m => m.Event_Id).ToList(),
                                volunteers = getVolunteers,
                                orgInfos = getOrgInfo,
                                listofUserDonated = getUserDonated,
                                detailsEventImage = _eventImages.GetAll().ToList(),
                                listOfEventsSection = db.vw_ListOfEvent.Where(m => m.End_Date >= DateTime.Now && m.status != 3).ToList(),
                                ListOfDonationEvents = donationList,

                                skillRequirement1 = db.OrgSkillRequirement.Where(m => m.eventId != 3).ToList(),
                                matchSkillByUserId = db.sp_checkMatchByUserId(UserId).ToList(),
                                volunteersStatusEvent = _volunteers.GetAll().Where(m => m.userId == UserId).ToList(),
                                sp_userListEvent = db.sp_UserListEvent(UserId).ToList(),
                                orgEvents = _orgEvents.GetAll().Where(m => m.status == 1).ToList()
                            };
                            return View(indexModel);
                        }
                        else
                        {
                            var indexModel = new Lists()
                            {
                                volunteersInfo = getInfo,
                                volunteersSkills = getVolunteerSkills,
                                skills = getSkills,
                                picture = getProfile,
                                listOfEvents = db.vw_ListOfEvent.OrderByDescending(m => m.Event_Id).ToList(),
                                volunteers = getVolunteers,
                                orgInfos = getOrgInfo,
                                listofUserDonated = getUserDonated,
                                detailsEventImage = _eventImages.GetAll().ToList(),
                                listOfEventsSection = db.vw_ListOfEvent.Where(m => m.End_Date >= DateTime.Now & m.status != 3).ToList(),

                                volunteersStatusEvent = _volunteers.GetAll().Where(m => m.userId == UserId).ToList(),
                                sp_userListEvent = db.sp_UserListEvent(UserId).ToList(),
                                orgEvents = _orgEvents.GetAll().Where(m => m.status == 1).ToList()
                            };
                            return View(indexModel);
                        }
                    case 2:
                        return RedirectToAction("Index", "Organization");
                    case 3:
                        return RedirectToAction("Index", "Admin");
                }
            }
            return View();
        }
        public JsonResult GetFilteredEvents(string searchTerm)
        {
            var events = db.vw_ListOfEvent
                          .Where(e => e.End_Date >= DateTime.Now && e.Event_Name.Contains(searchTerm))
                          .Select(e => new
                          {
                              e.Event_Id,
                              e.Event_Name,
                              EventImage = db.OrgEventImage.Where(m => m.eventId == e.Event_Id).Select(m => m.eventImage).FirstOrDefault() // Adjust as needed
                          })
                          .ToList();

            return Json(events, JsonRequestBehavior.AllowGet);
        }
        [AllowAnonymous]
        public ActionResult ChooseRegister()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult RegisterOrg()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult RegisterOrg(UserAccount u, OrgInfo o, String ConfirmPass, OrgValidation ov, UserRoles r, HttpPostedFileBase picture1, HttpPostedFileBase picture2)
        {
            // Backend validation checks
            List<string> validationErrors = new List<string>();
            o.profilePath = "~/Content/IdPicture/download (4).jpg";
            o.orgEmail = u.email;

            // Check if phone number starts with '09' and has 11 digits
            if (string.IsNullOrWhiteSpace(o.phoneNum) || !Regex.IsMatch(o.phoneNum, @"^09\d{9}$"))
            {
                validationErrors.Add("Phone number must start with '09' and be 11 digits long.");
            }

            // Check if zip code has exactly 4 digits
            if (string.IsNullOrWhiteSpace(o.zipCode) || !Regex.IsMatch(o.zipCode, @"^\d{4}$"))
            {
                validationErrors.Add("Zip code must be exactly 4 digits.");
            }

            // Check if email is provided
            if (string.IsNullOrWhiteSpace(u.email))
            {
                validationErrors.Add("Email is required.");
            }

            // Check if password is provided and meets the criteria
            if (string.IsNullOrWhiteSpace(u.password))
            {
                validationErrors.Add("Password is required.");
            }
            else
            {
                if (!Regex.IsMatch(u.password, @"[A-Z]")) // At least one uppercase letter
                {
                    validationErrors.Add("Password must contain at least one uppercase letter.");
                }
                if (!Regex.IsMatch(u.password, @"[a-z]")) // At least one lowercase letter
                {
                    validationErrors.Add("Password must contain at least one lowercase letter.");
                }
                if (!Regex.IsMatch(u.password, @"\d")) // At least one number
                {
                    validationErrors.Add("Password must contain at least one number.");
                }
                if (!Regex.IsMatch(u.password, @"[!@#$%^&*(),.?""{}|<>]")) // At least one special character
                {
                    validationErrors.Add("Password must contain at least one special character.");
                }
                if (u.password.Length < 8) // Minimum length of 8 characters
                {
                    validationErrors.Add("Password must be at least 8 characters long.");
                }
            }

            // Check if confirm password matches
            if (u.password != ConfirmPass)
            {
                validationErrors.Add("Confirm Password does not match Password.");
            }

            // Check if organization name is provided
            if (string.IsNullOrWhiteSpace(o.orgName))
            {
                validationErrors.Add("Organization Name is required.");
            }

            // Check if valid IDs are uploaded
            if (picture1 == null || picture1.ContentLength <= 0)
            {
                validationErrors.Add("Valid ID 1 is required.");
            }
            if (picture2 == null || picture2.ContentLength <= 0)
            {
                validationErrors.Add("Valid ID 2 is required.");
            }

            // If there are validation errors, return the view with error messages
            if (validationErrors.Count > 0)
            {
                TempData["ErrorMessage"] = string.Join("<br/>", validationErrors);
                return View();
            }

            // Process picture1
            if (picture1 != null && picture1.ContentLength > 0)
            {
                var inputFileName = Path.GetFileName(picture1.FileName);
                var serverSavePath = Path.Combine(Server.MapPath("~/Content/IdPicture/"), inputFileName);

                if (!Directory.Exists(Server.MapPath("~/Content/IdPicture/")))
                    Directory.CreateDirectory(Server.MapPath("~/Content/IdPicture/"));

                picture1.SaveAs(serverSavePath);
                ov.idPicture1 = inputFileName;
            }

            // Process picture2
            if (picture2 != null && picture2.ContentLength > 0)
            {
                var inputFileName = Path.GetFileName(picture2.FileName);
                var serverSavePath = Path.Combine(Server.MapPath("~/Content/IdPicture/"), inputFileName);

                if (!Directory.Exists(Server.MapPath("~/Content/IdPicture/")))
                    Directory.CreateDirectory(Server.MapPath("~/Content/IdPicture/"));

                picture2.SaveAs(serverSavePath);
                ov.idPicture2 = inputFileName;
            }

            // Register the organization
            if (_userManager.OrgRegister(u, o, ov, r, ref ErrorMessage) != ErrorCode.Success)
            {
                TempData["ErrorMessage"] = ErrorMessage;
                return View();
            }

            string registrationMessage = $"A new organization named {o.orgName} is awaiting for your registration approval.";
            if (_organizationManager.SentNotif(3, u.userId, u.userId, "Registration", registrationMessage, 0, ref ErrorMessage) != ErrorCode.Success)
            {
                TempData["ErrorMessage"] = ErrorMessage;
                return View();
            }

            // Store email and account information in TempData and Session
            TempData["email"] = u.email;
            Session["email"] = u.email;
            Session["NewAccountId"] = u.userId;

            // Generate and send OTP
            Random random = new Random();
            int randomOTP = random.Next(1000, 10000);
            Session["randomOTP"] = randomOTP;

            MailManager sendOTP = new MailManager();
            string subject = "Welcome to our website!";
            string userEmail = u.email;
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
                    background-color: #007bff;
                    padding: 10px 20px;
                    color: white;
                    text-align: center;
                }}
                .header img {{
                    max-width: 100px;
                    display: block;
                    margin: 0 auto;
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
                    color: #333;
                }}
                .otp {{
                    display: block;
                    width: fit-content;
                    margin: 20px auto;
                    padding: 10px 20px;
                    background-color: #007bff;
                    color: #ffffff;
                    border-radius: 5px;
                    font-size: 24px;
                    text-align: center;
                }}
                .footer {{
                    text-align: center;
                    margin-top: 20px;
                    padding: 10px;
                    background-color: #f1f1f1;
                    color: #555;
                    font-size: 14px;
                }}
                .footer a {{
                    color: #007bff;
                    text-decoration: none;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>Welcome to Tabang Hub!</h1>
                </div>
                <div class='content'>
                    <p>Dear {u.email},</p>
                    <p>Thank you for joining <strong>Tabang Hub</strong>. We're excited to have you as part of our community. To complete your registration, please verify your email address by entering the following One-Time Password (OTP) on the verification page:</p>
                    <div class='otp'>{randomOTP}</div>
                    <p>Thank you,<br>The Tabang Hub Team</p>
                </div>
                <div class='footer'>
                    <p>&copy; 2024 Tabang Hub. All rights reserved.</p>
                    <p>
                        <a href='https://www.tabanghub.com/terms'>Terms of Service</a> |
                        <a href='https://www.tabanghub.com/privacy'>Privacy Policy</a>
                    </p>
                </div>
            </div>
        </body>
        </html>";

            string errorResponse = "";
            bool isOTPSent = sendOTP.SendEmail(userEmail, subject, body, ref errorResponse);
            if (isOTPSent)
            {
                return RedirectToAction("Verify");
            }

            // If OTP sending fails, return to the registration view with an error message
            TempData["ErrorMessage"] = "An error occurred while sending the OTP. Please try again.";
            return View();
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Register(UserAccount u, VolunteerInfo v, UserRoles r, string ConfirmPass)
        {
            u.status = 0;
            u.roleId = 1;

            try
            {

                List<string> validationErrors = new List<string>();

                // Password validation
                if (!Regex.IsMatch(u.password, @"[A-Z]"))
                    validationErrors.Add("Password must contain at least one uppercase letter.");
                if (!Regex.IsMatch(u.password, @"[a-z]"))
                    validationErrors.Add("Password must contain at least one lowercase letter.");
                if (!Regex.IsMatch(u.password, @"\d"))
                    validationErrors.Add("Password must contain at least one number.");
                if (!Regex.IsMatch(u.password, @"[!@#$%^&*(),.?""{}|<>]"))
                    validationErrors.Add("Password must contain at least one special character.");
                if (u.password.Length < 8)
                    validationErrors.Add("Password must be at least 8 characters long.");

                // Check if confirm password matches
                if (u.password != ConfirmPass)
                    validationErrors.Add("Passwords do not match.");

                if (validationErrors.Any())
                {
                    TempData["ErrorMessage"] = string.Join("<br>", validationErrors);
                    return View(u);
                }

                if (_userManager.Register(u, v, r, ref ErrorMessage) != ErrorCode.Success)
                {
                    TempData["ErrorMessage"] = ErrorMessage;
                    return View();
                }

                TempData["email"] = u.email;
                Session["email"] = u.email;
                Session["NewAccountId"] = u.userId;

                Random random = new Random();
                int randomOTP = random.Next(1000, 10000);
                Session["randomOTP"] = randomOTP;

                MailManager sendOTP = new MailManager();
                string subject = "Welcome to our website!";
                string userEmail = u.email;
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
                    background-color: #007bff;
                    padding: 10px 20px;
                    color: white;
                    text-align: center;
                }}
                .header img {{
                    max-width: 100px;
                    display: block;
                    margin: 0 auto;
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
                    color: #333;
                }}
                .otp {{
                    display: block;
                    width: fit-content;
                    margin: 20px auto;
                    padding: 10px 20px;
                    background-color: #007bff;
                    color: #ffffff;
                    border-radius: 5px;
                    font-size: 24px;
                    text-align: center;
                }}
                .footer {{
                    text-align: center;
                    margin-top: 20px;
                    padding: 10px;
                    background-color: #f1f1f1;
                    color: #555;
                    font-size: 14px;
                }}
                .footer a {{
                    color: #007bff;
                    text-decoration: none;
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>Welcome to Tabang Hub!</h1>
                </div>
                <div class='content'>
                    <p>Dear {u.email},</p>
                    <p>Thank you for joining <strong>Tabang Hub</strong>. We're excited to have you as part of our community. To complete your registration, please verify your email address by entering the following One-Time Password (OTP) on the verification page:</p>
                    <div class='otp'>{randomOTP}</div>
                    <p>Thank you,<br>The Tabang Hub Team</p>
                </div>
                <div class='footer'>
                    <p>&copy; 2024 Tabang Hub. All rights reserved.</p>
                    <p>
                        <a href='https://www.tabanghub.com/terms'>Terms of Service</a> |
                        <a href='https://www.tabanghub.com/privacy'>Privacy Policy</a>
                    </p>
                </div>
            </div>
        </body>
        </html>";

                string errorResponse = "";

                bool isOTPSent = sendOTP.SendEmail(userEmail, subject, body, ref errorResponse);
                if (isOTPSent)
                {
                    return RedirectToAction("Verify");
                }

                TempData["ErrorMessage"] = "An error occurred while sending the OTP. Please try again.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error! " + ex.Message;
                return RedirectToAction("Register");
            }
        }
        [AllowAnonymous]
        public ActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index");
            }
            return View();
        }
        [AllowAnonymous]
        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            var user = _userManager.GetUserByEmail(username);

            if (user == null)
            {
                ViewBag.Error = "Email does not exist.";
                return View();
            }

            if (_userManager.Login(username, password, ref ErrorMessage) == ErrorCode.Success)
            {
                // Check if the user's status is pending admin approval (status 3)
                if (user.status == 3)
                {
                    ViewBag.Error = "Your registration is pending admin approval. Please wait for further updates.";
                    return View();
                }

                if (user.status != (int)Status.Active)
                {
                    TempData["email"] = user.email;
                    TempData["login"] = 1;
                    Session["email"] = user.email;
                    Session["NewAccountId"] = user.userId;

                    Random random = new Random();
                    int randomOTP = random.Next(1000, 10000);
                    Session["randomOTP"] = randomOTP;

                    MailManager sendOTP = new MailManager();
                    string subject = "Welcome to Tabang Hub!";
                    string userEmail = user.email;
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
            background-color: #007bff;
            padding: 10px 20px;
            color: white;
            text-align: center;
        }}
        .content {{
            padding: 20px;
            font-size: 16px;
            line-height: 1.6;
            color: #333;
        }}
        .otp {{
            display: block;
            width: fit-content;
            margin: 20px auto;
            padding: 10px 20px;
            background-color: #007bff;
            color: #ffffff;
            border-radius: 5px;
            font-size: 24px;
            text-align: center;
        }}
        .footer {{
            text-align: center;
            margin-top: 20px;
            padding: 10px;
            background-color: #f1f1f1;
            color: #555;
            font-size: 14px;
        }}
        .footer a {{
            color: #007bff;
            text-decoration: none;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to Tabang Hub!</h1>
        </div>
        <div class='content'>
            <p>Dear {user.email},</p>
            <p>Thank you for joining <strong>Tabang Hub</strong>. We're excited to have you as part of our community. To complete your registration, please verify your email address by entering the following One-Time Password (OTP) on the verification page:</p>
            <div class='otp'>{randomOTP}</div>
            <p>Thank you,<br>The Tabang Hub Team</p>
        </div>
        <div class='footer'>
            <p>&copy; 2024 Tabang Hub. All rights reserved.</p>
            <p>
                <a href='https://www.tabanghub.com/terms'>Terms of Service</a> |
                <a href='https://www.tabanghub.com/privacy'>Privacy Policy</a>
            </p>
        </div>
    </div>
</body>
</html>";

                    string errorResponse = "";

                    bool isOTPSent = sendOTP.SendEmail(userEmail, subject, body, ref errorResponse);
                    if (isOTPSent)
                    {
                        return RedirectToAction("Verify");
                    }

                    TempData["ErrorMessage"] = "An error occurred while sending the OTP. Please try again.";
                    return RedirectToAction("Login");
                }

                // Authenticate user and redirect based on role
                FormsAuthentication.SetAuthCookie(username, false);

                if (user.roleId == 1)
                {
                    return RedirectToAction("Index");
                }
                else if (user.roleId == 2)
                {
                    return RedirectToAction("Index", "Organization");
                }
                else if (user.roleId == 3)
                {
                    return RedirectToAction("Index", "Admin");
                }
            }
            else
            {
                ViewBag.Error = ErrorMessage;
            }

            return View();
        }


        [AllowAnonymous]
        public ActionResult Verify()
        {
            if (String.IsNullOrEmpty(TempData["email"] as String))
                return RedirectToAction("Login");

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public JsonResult Verify(int otp1, int otp2, int otp3, int otp4)
        {
            // Retrieve the email and expected OTP from the session
            string email = Session["email"] as string;
            string expectedOTP = Session["randomOTP"]?.ToString();

            // Validate session data
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(expectedOTP))
            {
                return Json(new { success = false, message = "Session has expired. Please log in again." });
            }

            // Retrieve the user based on the email
            var user = _userManager.GetUserByEmail(email);
            if (user == null)
            {
                return Json(new { success = false, message = "User not found." });
            }

            // Combine entered OTP
            string enteredOTP = $"{otp1}{otp2}{otp3}{otp4}";

            // Verify the entered OTP
            if (enteredOTP == expectedOTP)
            {
                string redirectUrl;

                // Check if the user is an organization admin (role ID 2)
                if (user.roleId == 2)
                {
                    // Update status to 'Pending Approval' for organization users
                    if (_userManager.UpdateUserStatus(user.userId, 3, ref ErrorMessage) != ErrorCode.Success)
                    {
                        return Json(new { success = false, message = "Failed to update user status. Please try again." });
                    }
                   
                    redirectUrl = Url.Action("AdminApprove", "Page");
                }
                else
                {
                    // For other roles, update status to 'Active'
                    if (_userManager.UpdateUserStatus(user.userId, (int)Status.Active, ref ErrorMessage) != ErrorCode.Success)
                    {
                        return Json(new { success = false, message = "Failed to update user status. Please try again." });
                    }

                    // Set authentication cookie
                    FormsAuthentication.SetAuthCookie(user.email, false);

                    // Determine redirect URL based on role
                    switch (user.roleId)
                    {
                        case 1: // Regular user role
                            redirectUrl = Url.Action("Index", "Page");
                            break;
                        case 3: // Admin role
                            redirectUrl = Url.Action("Index", "Admin");
                            break;
                        default:
                            redirectUrl = Url.Action("Login");
                            break;
                    }
                }

                return Json(new { success = true, message = "Email has been verified!", redirectUrl });
            }
            else
            {
                // Return error if OTP is incorrect
                return Json(new { success = false, message = "Incorrect OTP. Please try again!" });
            }
        }
        [AllowAnonymous]
        public ActionResult AdminApprove()
        {
            return View();
        }
        [AllowAnonymous]
        public ActionResult Logout()
        {
            Session.Clear();
            FormsAuthentication.SignOut();
            return RedirectToAction("Login");
        }
    }
}