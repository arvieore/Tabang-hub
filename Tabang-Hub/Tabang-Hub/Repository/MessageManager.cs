using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Tabang_Hub.Repository
{
    public class MessageManager
    {
        private BaseRepository<GroupChat> _groupChat;
        private BaseRepository<OrgEventImage> _eventImage;
        public MessageManager() 
        {
            _groupChat = new BaseRepository<GroupChat>();
            _eventImage = new BaseRepository<OrgEventImage>();
        }

        public List<GroupChat> GetGroupChatByUserId(int userId)
        { 
            return _groupChat._table.Where(m => m.OrgInfo.userId == userId).ToList();
        }
        public OrgEventImage GetEventImageByEventId(int eventId)
        {
            return _eventImage._table.Where(m => m.eventId == eventId).FirstOrDefault();
        }
    }
}