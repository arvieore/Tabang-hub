using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Tabang_Hub.Utils;
using System.Web.Mvc;
using System.Web.Helpers;
using Tabang_Hub.Repository;
using System.Data.Entity;
using Tabang_Hub.Controllers;
using System.Threading.Tasks;

namespace Tabang_Hub.Hubs
{
    public class ChatHub : Hub
    {
        private readonly TabangHubEntities _db;

        public ChatHub()
        {
            _db = new TabangHubEntities();
        }
        //public void Send(int userId, int groupId, string message)
        //{
        //    if(userId.Equals(null) || groupId.Equals(null) || string.IsNullOrEmpty(message))
        //    {
        //        return;
        //    }
        //    var user = _db.OrgInfo.Where(m => m.userId == userId).FirstOrDefault();
        //    var userName = "";

        //    if (user != null && user.UserAccount.roleId == 2)
        //    {
        //        userName = user != null ? char.ToUpper(user.orgName[0]) + user.orgName.Substring(1) : "Unknown User";
        //    }
        //    else
        //    {
        //        var userInfo = _db.VolunteerInfo.Where(m => m.userId == userId).FirstOrDefault();
        //        userName = userInfo != null ? char.ToUpper(userInfo.fName[0]) + userInfo.fName.Substring(1) + ' ' + char.ToUpper(userInfo.lName[0]) + userInfo.lName.Substring(1) : "Unknown User";
        //    }

        //    var groupChatExists = _db.GroupChat.Any(m => m.groupChatId == groupId);
        //    if (!groupChatExists)
        //    {
        //        Clients.Caller.groupChatNotFound();
        //        return;
        //    }


        //    var gc = new GroupMessages
        //    {
        //        message = message,
        //        messageAt = DateTime.Now,
        //        groupChatId = groupId,
        //        userId = userId,
        //    };

        //    _db.GroupMessages.Add(gc);
        //    _db.SaveChanges();

        //    Clients.All.broadcastMessage(userName, userId, message, groupId);
        //}

        public void Send(int userId, int groupId, string message)
        {
            if (userId == 0 || groupId == 0 || string.IsNullOrEmpty(message))
            {
                return;
            }

            // Define Philippine time zone
            TimeZoneInfo philippineTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");

            // Get current Philippine time
            DateTime philippineTime = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, philippineTimeZone);

            var user = _db.OrgInfo.FirstOrDefault(m => m.userId == userId);
            string userName = "Unknown User";

            if (user != null && user.UserAccount.roleId == 2)
            {
                userName = char.ToUpper(user.orgName[0]) + user.orgName.Substring(1);
            }
            else
            {
                var userInfo = _db.VolunteerInfo.FirstOrDefault(m => m.userId == userId);
                if (userInfo != null)
                {
                    userName = char.ToUpper(userInfo.fName[0]) + userInfo.fName.Substring(1) + ' ' +
                               char.ToUpper(userInfo.lName[0]) + userInfo.lName.Substring(1);
                }
            }

            // Check if group chat exists
            var groupChatExists = _db.GroupChat.Any(m => m.groupChatId == groupId);
            if (!groupChatExists)
            {
                Clients.Caller.groupChatNotFound();
                return;
            }

            // Create a new group message
            var gc = new GroupMessages
            {
                message = message,
                messageAt = philippineTime, // Use Philippine time
                groupChatId = groupId,
                userId = userId,
            };

            _db.GroupMessages.Add(gc);
            _db.SaveChanges();

            // Broadcast the message to all clients
            Clients.All.broadcastMessage(userName, userId, message, groupId);
        }


        public void GetAllMessages(int groupId)
        {
            var groupChatExists = _db.GroupChat.Any(m => m.groupChatId == groupId);

            if (!groupChatExists)
            {
                Clients.Caller.groupChatNotFound();
                return;
            }

            var messages = _db.sp_GetAllMessage1(groupId);
            Clients.Caller.loadMessages(messages);
        }
    }
}