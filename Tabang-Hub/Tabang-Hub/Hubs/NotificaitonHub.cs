using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tabang_Hub.Repository;

namespace Tabang_Hub.Hubs
{
    [HubName("notificationHub")]
    public class NotificationHub : Hub
    {
        private readonly TabangHubEntities _db;

        public NotificationHub()
        {
            _db = new TabangHubEntities();
        }

        public void SendNotification(int userId, int senderUserId, int relatedId, string type, string content, bool isBroadcast = false)
        {
            // Validate inputs
            if (userId == 0 || string.IsNullOrEmpty(content))
            {
                return;
            }

            // Save notification to the database
            var notification = new Notification
            {
                userId = isBroadcast ? (int?)null : userId, // If it's a broadcast, set userId to null
                senderUserId = senderUserId,
                relatedId = relatedId,
                type = type,
                content = content,
                broadcast = isBroadcast ? 1 : 0,
                status = 0, // Assume 0 = unread, 1 = read
                createdAt = DateTime.Now
            };

            _db.Notification.Add(notification);
            _db.SaveChanges();

            // Send notification to specific user or broadcast to all connected users
            var context = GlobalHost.ConnectionManager.GetHubContext<NotificationHub>();
            try
            {
                if (isBroadcast)
                {
                    context.Clients.All.receiveNotification(type, content);
                }
                else
                {
                    // Attempt to send the notification to the connected user
                    context.Clients.User(userId.ToString()).receiveNotification(type, content);
                }
            }
            catch (Exception ex)
            {
                // Log the exception if there’s an issue sending the real-time notification
                System.Diagnostics.Debug.WriteLine($"Error sending real-time notification: {ex.Message}");
            }
        }


        // Method to mark a notification as read
        public void MarkAsRead(int notificationId)
        {
            var notification = _db.Notification.Find(notificationId);
            if (notification != null && notification.status == 0)
            {
                notification.status = 1; // Mark as read
                notification.readAt = DateTime.Now;
                _db.SaveChanges();

                Clients.Caller.updateNotificationStatus(notificationId, "read");
            }
        }

        // Method to broadcast a notification to all users
        //public void BroadcastNotification(int senderUserId, string type, string content)
        //{
        //    SendNotification(0, senderUserId, type, content, true);
        //}
    }
}