using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Security;
using Tabang_Hub.Utils;

namespace Tabang_Hub.Repository
{
    public class UserManager
    {
        private BaseRepository<UserAccount> _userAcc;
        private BaseRepository<VolunteerInfo> _volunteerInfo;
        private BaseRepository<OrgInfo> _orgInfo;
        private BaseRepository<OrgValidation> _orgValid;
        private BaseRepository<Skills> _volunteerSkills;
        private BaseRepository<ProfilePicture> _profilePic;
        private BaseRepository<UserRoles> _userRoles;
        public UserManager() 
        {
            _userAcc = new BaseRepository<UserAccount>();
            _volunteerInfo = new BaseRepository<VolunteerInfo>();
            _orgInfo = new BaseRepository<OrgInfo>();
            _orgValid = new BaseRepository<OrgValidation>();
            _volunteerSkills = new BaseRepository<Skills>();
            _profilePic = new BaseRepository<ProfilePicture>();
            _userRoles = new BaseRepository<UserRoles>();

        }

        public UserAccount GetUserByEmail(String email)
        {
            return _userAcc._table.Where(m => m.email == email).FirstOrDefault();
        }
        public List<UserAccount> ListOfAdmin()
        {
            return _userAcc._table.Where(m => m.roleId == 3).ToList();
        }
        public UserAccount GetUserById(int id)
        { 
            return _userAcc._table.Where(m => m.userId == id).FirstOrDefault();
        }
        public ErrorCode Login(string email, string password, ref string errMsg)
        {
            var userLogin = GetUserByEmail(email);
            if (userLogin == null)
            {
                errMsg = "User does not exist!";
                return ErrorCode.Error;
            }

            if (!userLogin.password.Equals(password))
            {
                errMsg = "Password is incorrect";
                return ErrorCode.Error;
            }

            // Check if the user status is pending admin approval (status 3)
            if (userLogin.status == 3)
            {
                errMsg = "Your registration is pending admin approval.";
                return ErrorCode.Error;
            }

            // Ensure only Gmail addresses are allowed for roles other than admin (roleId != 3)
            if (userLogin.roleId != 3 && !email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase))
            {
                errMsg = "Only Gmail addresses are allowed for this role";
                return ErrorCode.Error;
            }

            // User exists, password is correct, and all checks passed
            errMsg = "Login successful";
            return ErrorCode.Success;
        }

        public ErrorCode Register(UserAccount u, VolunteerInfo v, UserRoles r, ref String errMsg)
        {
            u.roleId = 1;
            u.status = 0;

            if (GetUserByEmail(u.email) != null)
            {
                errMsg = "Email Already Exist";
                return ErrorCode.Error;
            }

            if (_userAcc.Create(u, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            v.userId = u.userId;          
            if (_volunteerInfo.Create(v, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            r.userId = u.userId;
            r.userRole = u.roleId;
            if (_userRoles.Create(r, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            return ErrorCode.Success;
        }

        public ErrorCode OrgRegister(UserAccount u, OrgInfo o, OrgValidation ov, UserRoles r, ref String errMsg)
        {
            u.roleId = 2;
            u.status = 0;
            o.orgEmail = u.email;
            var profilePic = new ProfilePicture();
            if (GetUserByEmail(u.email) != null)
            {
                errMsg = "Email Already Exist";
                return ErrorCode.Error;
            }
            if (ov.idPicture1 == null)
            {
                errMsg = "Please provide Id picture";
                return ErrorCode.Error;
            }
            if (ov.idPicture2 == null)
            {
                errMsg = "Please provide Id picture";
                return ErrorCode.Error;
            }

            if (_userAcc.Create(u, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            ov.userId = u.userId;
            if (_orgValid.Create(ov, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            profilePic.profilePath = "~/Content/images/tabanghub3.png";
            //profilePic.userId = u.userId;
            //if (_profilePic.Create(profilePic, out errMsg) != ErrorCode.Success)
            //{
            //    return ErrorCode.Error;
            //}
            r.userId = u.userId;
            r.userRole = u.roleId;
            if (_userRoles.Create(r, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            o.userId = u.userId;

            //o.profileId = profilePic.profileId;
            if (_orgInfo.Create(o, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            return ErrorCode.Success;
        }
        public ErrorCode UpdateUserStatus(int userId, short newStatus, ref string errMsg)
        {
            // First, retrieve the user account by its ID
            var user = _userAcc.Get(userId);
            var uInfo = _volunteerInfo.GetAll().Where(m => m.userId == userId).FirstOrDefault();

            if (user.roleId == 2)
            {
                user.status = newStatus;

                if (_userAcc.Update(user.userId, user, out errMsg) != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }
            else if (user != null && uInfo != null)
            {
                // Update the status field
                user.status = newStatus;
                uInfo.fName = " ";
                uInfo.lName = " ";
                _volunteerInfo.Update(userId, uInfo, out errMsg);
                // Now, call the Update method to save the changes
                return (ErrorCode)_userAcc.Update(userId, user, out errMsg);
            }
            else
            {
                errMsg = "User not found";
                return ErrorCode.Error;
            }
            return ErrorCode.Success;
        }
    }
}