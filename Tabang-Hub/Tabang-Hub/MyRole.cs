using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;

namespace Tabang_Hub
{
    public class MyRole : RoleProvider
    {
        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }

        public override string[] GetAllRoles()
        {
            using (var db = new TabangHubEntities())
            {
                return db.Role.Select(m => m.roleName).ToArray();
            }
        }

        public override string[] GetRolesForUser(string username)
        {
            using (var db = new TabangHubEntities())
            {
                return db.vw_UserRoles.Where(m => m.email == username).Select(m => m.roleName).ToArray();
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }

        public string GetRoleName(int roleId)
        {
            switch (roleId)
            {
                case 1:
                    return "Volunteer";
                case 2:
                    return "Organization";
                case 3:
                    return "Admin";
                default:
                    return "Unknown";
            }
        }
    }
}