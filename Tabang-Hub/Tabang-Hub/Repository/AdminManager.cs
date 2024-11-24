using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tabang_Hub.Utils;

namespace Tabang_Hub.Repository
{
    public class AdminManager
    {
        private BaseRepository<vw_VolunteerAccounts> _volunteerAccounts;
        private BaseRepository<vw_OrganizationAccounts> _organizationAccounts;
        private BaseRepository<UserAccount> _userAccount;
        private BaseRepository<VolunteerInfo> _volunteerInfo;
        private BaseRepository<ProfilePicture> _profilePic;
        private BaseRepository<Volunteers> _joinedEvent;
        private BaseRepository<VolunteerSkill> _volunteerSkill;
        private BaseRepository<UserRoles> _userRoles;
        private BaseRepository<OrgSkillRequirement> _orgSkillRequirement;
        private BaseRepository<OrgEventImage> _orgEventImage;
        private BaseRepository<UserDonated> _userDonated;
        private BaseRepository<Volunteers> _volunteers;
        private BaseRepository<OrgEvents> _orgEvents;
        private BaseRepository<OrgValidation> _orgValidation;
        private BaseRepository<OrgInfo> _orgInfo;
        private BaseRepository<Skills> _skills;


        public AdminManager()
        {
            _volunteerAccounts = new BaseRepository<vw_VolunteerAccounts>();
            _organizationAccounts = new BaseRepository<vw_OrganizationAccounts>();
            _userAccount = new BaseRepository<UserAccount>();
            _volunteerInfo = new BaseRepository<VolunteerInfo>();
            _profilePic = new BaseRepository<ProfilePicture>();
            _joinedEvent = new BaseRepository<Volunteers>();
            _volunteerSkill = new BaseRepository<VolunteerSkill>();
            _userRoles = new BaseRepository<UserRoles>();
            _orgSkillRequirement = new BaseRepository<OrgSkillRequirement>();
            _orgEventImage = new BaseRepository<OrgEventImage>();
            _userDonated = new BaseRepository<UserDonated>();
            _volunteers = new BaseRepository<Volunteers>();
            _orgEvents = new BaseRepository<OrgEvents>();
            _orgValidation = new BaseRepository<OrgValidation>();
            _orgInfo = new BaseRepository<OrgInfo>();
            _skills = new BaseRepository<Skills>();
        }

        public List<vw_VolunteerAccounts> GetVolunteerAccounts()
        {
            return _volunteerAccounts.GetAll();
        }
        public List<vw_OrganizationAccounts> GetOrganizationAccounts()
        {
            return _organizationAccounts.GetAll();
        }
        public Skills GetSkillByName(string name)
        {
            return _skills._table.Where(m => m.skillName == name).FirstOrDefault();
        }
        public UserAccount GetUserById(int userId)
        {
            return _userAccount._table.Where(m => m.userId == userId).FirstOrDefault();
        }
        public VolunteerInfo GetVolunteerInfoByUserId(int userId)
        {
            return _volunteerInfo._table.Where(m => m.userId == userId).FirstOrDefault();
        }
        public List<ProfilePicture> GetProfileByUserId(int userId)
        {
            return _profilePic.GetAll().Where(m => m.userId == userId).ToList();
        }
        public List<Volunteers> GetJoinedEventByUserId(int userId)
        {
            return _joinedEvent.GetAll().Where(m => m.userId == userId).ToList();
        }
        public List<VolunteerSkill> GetVolunteerSkillsByUserId(int userId)
        {
            return _volunteerSkill.GetAll().Where(m => m.userId == userId).ToList();
        }
        
        public List<UserRoles> GetRolesByUserId(int userId)
        {
            return _userRoles.GetAll().Where(m => m.userId == userId).ToList();
        }
        public List<OrgSkillRequirement> GetSkillsRequirementByEventId(int eventId)
        {
            return _orgSkillRequirement.GetAll().Where(m => m.eventId == eventId).ToList();
        }
        public List<OrgEventImage> GetEventImagesByEventId(int eventId)
        {
            return _orgEventImage.GetAll().Where(m => m.eventId == eventId).ToList();
        }
        public List<UserDonated> GetUserDonatedByEventId(int eventId)
        {
            return _userDonated.GetAll().Where(m => m.eventId == eventId).ToList();
        }
        public List<Volunteers> GetVolunteersByEventId(int eventId)
        {
            return _volunteers.GetAll().Where(m => m.eventId == eventId).ToList();
        }
        public List<UserAccount> GetAllUser()
        {
            return _userAccount.GetAll();
        }
        public List<OrgValidation> GetListOrgValidationsByUserId(int userId)
        {
            return _orgValidation.GetAll().Where(m => m.userId == userId).ToList();
        }
        public OrgValidation GetOrgValidationsByUserId(int userId)
        {
            return _orgValidation._table.Where(m => m.userId == userId).FirstOrDefault();
        }
        public OrgInfo GetOrgInfoByUserId(int userId)
        {
            return _orgInfo._table.Where(m => m.userId == userId).FirstOrDefault();
        }
        public List<OrgEvents> GetEventsByUserId(int userId)
        {
            return _orgEvents.GetAll().Where(m => m.userId == userId).ToList();
        }
        public List<Skills> GetSkills()
        {
            return _skills.GetAll();
        }
        public Skills GetSkillById(int skillId)
        {
            return _skills._table.Where(m => m.skillId == skillId).FirstOrDefault();
        }
        public List<VolunteerSkill> GetVolunteerSkillsBySkillId(int skillId)
        { 
            return _volunteerSkill._table.Where(m => m.skillId == skillId).ToList();
        }
        //Admin Reports
        public List<VolunteerSkill> GetAllSkills()
        { 
            return _volunteerSkill.GetAll();
        }
        public List<OrgSkillRequirement> GetOrgSkillRequirementsByEventId(int eventId)
        { 
            return _orgSkillRequirement._table.Where(m => m.eventId == eventId).ToList();
        }
        public List<VolunteerSkill> GetVolunteerSkillBySkillId(int skillId)
        {
            return _volunteerSkill._table.Where(m => m.skillId == skillId).ToList();
        }
        public Dictionary<string, int> GetAllVolunteerSkills()
        {
            // Step 1: Get the list of events created by the user (Organization or Event Creator)
            var allSkills = GetAllSkills(); // Assuming ListOfEvents(userId) returns a list of events
            // Step 2: Initialize a dictionary to count the frequency of each skill by name
            Dictionary<string, int> skillFrequency = new Dictionary<string, int>();

                // For each volunteer, get their skills
                foreach (var volunteer in allSkills)
                {
                    // Assuming you have a method GetVolunteerSkillsByUserId to get the skills of a volunteer
                    var skills = GetVolunteerSkillsByUserId((int)volunteer.userId);

                    // Count the occurrence of each skill by its name
                    foreach (var skill in skills)
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

            // Step 4: Return the dictionary containing the skills and their counts
            return skillFrequency;
        }
        public ErrorCode EditSkill(int skillId, string skillName, string imagePath, ref string errMsg)
        {
            var toUpdate = GetSkillById(skillId);
            var toDelete = GetVolunteerSkillBySkillId((int)skillId);

            if (toUpdate == null)
            {
                errMsg = "Skill not found.";
                return ErrorCode.Error;
            }

            // Update properties
            toUpdate.skillName = skillName;
            toUpdate.skillImage = imagePath;

            // Attempt to update the database
            if (_skills.Update(toUpdate.skillId, toUpdate, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            foreach (var del in toDelete)
            {
                if (_volunteerSkill.Delete(del.skillId) != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }
            return ErrorCode.Success;
        }
        public List<OrgInfo> GetRecentOrgAccount()
        {
            return _orgInfo.GetAll()
                       .Where(u => u.UserAccount.roleId == 2) // Filter to only organization accounts
                       .OrderByDescending(u => u.userId) // Sort by the 'CreatedDate' property in descending order for recent accounts
                       .Take(5) // Limit the result to the most recent 10 accounts (adjust as needed)
                       .ToList();
        }
        public List<UserDonated> GetRecentDonated()
        {
            return _userDonated.GetAll()
                        .OrderByDescending(u => u.donatedAt)
                        .Take(5)
                        .ToList();
        }
        public List<OrgEvents> GetAllEvents()
        {
            return _orgEvents.GetAll().Where(m => m.status != 2).ToList();
        }
        public List<UserDonated> GetAllDonators()
        {
            return _userDonated.GetAll();
        }
        public List<OrgEvents> GetAllRecentOrgEvents()
        {
            var recentEvents = _orgEvents._table
                .Where(m => m.dateStart.HasValue && m.dateEnd.HasValue &&
                            m.dateStart.Value <= DateTime.Now && m.dateEnd.Value > DateTime.Now)
                // Events that have started but not yet ended
                .OrderByDescending(m => m.dateStart.Value) // Order by dateStart, most recent first
                .Take(5) // Get the top 5 most recent events
                .ToList();

            return recentEvents;
        }
        //public List<OrgInfo> GetAllRecentOrganization()
        //{
        //    var recentEvents = _orgInfo._table
        //        .Where(m => m.UserAccount.roleId == 2 && m.createAd.HasValue &&
        //                    m.dateStart.Value <= DateTime.Now && m.dateEnd.Value > DateTime.Now)
        //        // Events that have started but not yet ended
        //        .OrderByDescending(m => m.dateStart.Value) // Order by dateStart, most recent first
        //        .Take(5) // Get the top 5 most recent events
        //        .ToList();

        //    return recentEvents;
        //}
        public Dictionary<int, int> AllEventSummary()
        {
            // Events from the main table
            var eventSummary = _orgEvents._table
                .Where(m => m.dateStart.HasValue && m.dateEnd.HasValue && m.dateEnd.Value <= DateTime.Now)
                .GroupBy(m => m.dateStart.Value.Month)
                .ToDictionary(
                    group => group.Key,
                    group => group.Count()
                );

            // Events from the history table
            var historySummary = _orgEvents._table.Where(m => m.status == 2)
                .GroupBy(m => m.dateStart.Value.Month)
                .ToDictionary(
                    group => group.Key,
                    group => group.Count()
                );

            // Merge the two dictionaries
            foreach (var month in historySummary.Keys)
            {
                if (eventSummary.ContainsKey(month))
                {
                    eventSummary[month] += historySummary[month];
                }
                else
                {
                    eventSummary[month] = historySummary[month];
                }
            }

            return eventSummary;
        }
        public List<VolunteerInfo> GetVolunteerAccount()
        {
            return _volunteerInfo.GetAll();
        }
        public List<UserAccount> GetPendingOrg()
        { 
            return _userAccount._table.Where(m => m.status == 3).ToList();
        }
        public List<OrgInfo> GetOrganizationAccount()
        {
            return _orgInfo.GetAll();
        }
        public ErrorCode DeleteUser(int userId)
        {
            var user = GetUserById(userId);
            if (user == null)
            {
                return ErrorCode.Error;
            }

            var info = GetVolunteerInfoByUserId(user.userId);
            var skills = GetVolunteerSkillsByUserId(user.userId);
            var joinedEvents = GetJoinedEventByUserId(user.userId);
            var profilePic = GetProfileByUserId(user.userId);
            var userRoles = GetRolesByUserId(user.userId);

            if (info != null)
            {
                var result = _volunteerInfo.Delete(info.volunteerId);
                if (result != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }

            if (joinedEvents != null)
            {
                foreach (var joinedEvent in joinedEvents)
                {
                    var result = _joinedEvent.Delete(joinedEvent.applyVolunteerId);
                    if (result != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                }
            }

            if (skills != null)
            {
                foreach (var skill in skills)
                {
                    var result = _volunteerSkill.Delete(skill.volunteerSkillId);
                    if (result != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                }
            }

            if (profilePic != null)
            {
                foreach (var profile in profilePic)
                {
                    var result = _profilePic.Delete(profile.profileId);
                    if (result != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                }
            }
            if (userRoles != null)
            {
                foreach (var roles in userRoles)
                {
                    var result = _userRoles.Delete(roles.userRoleId);
                    if (result != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                }
            }
            var deleteUserResult = _userAccount.Delete(user.userId);
            if (deleteUserResult != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
        public ErrorCode DeleteOrganization(int userId)
        {
            var user = GetUserById(userId);
            if (user == null)
            {
                return ErrorCode.Error;
            }

            var events = GetEventsByUserId(user.userId);
            var validation = GetListOrgValidationsByUserId(user.userId);
            var profile = GetProfileByUserId(user.userId);
            var orgInfo = GetOrgInfoByUserId(user.userId);
            var userRoles = GetRolesByUserId(user.userId);
            var userAcc = GetUserById(user.userId);

            foreach (var orgEvent in events)
            {
                var skillRequirements = GetSkillsRequirementByEventId(orgEvent.eventId);
                var orgEventImage = GetEventImagesByEventId(orgEvent.eventId);
                var userDonate = GetUserDonatedByEventId(orgEvent.eventId);
                var volunteer = GetVolunteersByEventId(orgEvent.eventId);


                foreach (var skillRequirement in skillRequirements)
                {
                    var result = _orgSkillRequirement.Delete(skillRequirement.skillRequirementId);
                    if (result != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                }

                foreach (var eventImage in orgEventImage)
                {
                    var result = _orgEventImage.Delete(eventImage.eventImageId);
                    if (result != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                }

                foreach (var donated in userDonate)
                {
                    var result = _userDonated.Delete(donated.orgUserDonatedId);
                    if (result != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                }

                foreach (var volunteers in volunteer)
                {
                    var result = _volunteers.Delete(volunteers.applyVolunteerId);
                    if (result != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                }

                var results = _orgEvents.Delete(orgEvent.eventId);
                if (results != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }
            foreach (var valid in validation)
            {
                var result = _orgValidation.Delete(valid.orgValidationId);
                if (result != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }
            foreach (var profiles in profile)
            {
                var result = _profilePic.Delete(profiles.profileId);
                if (result != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }
            var deleteInf = _orgInfo.Delete(user.userId);
            if (deleteInf != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            foreach (var roles in userRoles)
            {
                var result = _userRoles.Delete(roles.userId);
                if (result != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }
            var deleteOrg = _userAccount.Delete(user.userId);
            if (deleteOrg != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }

            return ErrorCode.Success;
        }
        public ErrorCode AddSkills(Skills skill, ref string errMsg)
        {
            if (skill == null)
            {
                errMsg = "Skill information is required.";
                return ErrorCode.Error;
            }

            if (_skills.Create(skill, out errMsg) != ErrorCode.Success)
            {
                return ErrorCode.Error;
            }
            return ErrorCode.Success;
        }
        public ErrorCode DeleteSkill(int skillId)
        {
            var skill = GetSkillById(skillId);
            var volunteerSkill = GetVolunteerSkillsBySkillId(skillId);
            var events = GetAllEvents();

            foreach (var evnt in events)
            {
                var skillReq = GetOrgSkillRequirementsByEventId(evnt.eventId);

                foreach (var skllrq in skillReq)
                {
                    if (skllrq.skillId == skillId)
                    {
                        if (_orgSkillRequirement.Delete(skllrq.skillRequirementId) != ErrorCode.Success)
                        {
                            return ErrorCode.Error;
                        }
                    }
                }
            }
            
            if (volunteerSkill != null)
            {
                foreach (var skills in volunteerSkill)
                {
                    if (_volunteerSkill.Delete(skills.volunteerSkillId) != ErrorCode.Success)
                    {
                        return ErrorCode.Error;
                    }
                }
            }
            if (skill != null)
            {
                if (_skills.Delete(skill.skillId) != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }
            return ErrorCode.Success;
        }
        public ErrorCode DeactivateAccount(int userId, ref string errMsg)
        {
            var user = GetUserById(userId);
            user.status = 0;

            if (user != null)
            {
                if (_userAccount.Update(user.userId, user, out errMsg) != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }
            return ErrorCode.Success;
        }
        public ErrorCode ReactivateAccount(int userId, ref string errMsg)
        {
            var user = GetUserById(userId);
            user.status = 1;

            if (user != null)
            {
                if (_userAccount.Update(user.userId, user, out errMsg) != ErrorCode.Success)
                {
                    return ErrorCode.Error;
                }
            }
            return ErrorCode.Success;
        }
    }
}