using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tabang_Hub.Utils;
using Tabang_Hub.Repository;

namespace Tabang_Hub.Repository
{
    public class CalendarManager
    {
        private readonly TabangHubEntities db;

        public BaseRepository<vw_ListOfEvent> _vw_listOfEvent;
        public BaseRepository<OrgEvents> _orgEvents;
        public BaseRepository<OrgInfo> _orgInfo;
        public BaseRepository<OrgEventImage> _orgEventImage;
        public BaseRepository<VolunteerInfo> _volunteerInfo;
        public BaseRepository<VolunteersHistory> _volunteerHistory;
        public BaseRepository<VolunteerSkill> _volunteerSkill;
        public BaseRepository<Skills> _skill;

        public VolunteerManager _volunteerManager;
        public OrganizationManager _organizationManager;
        public CalendarManager()
        {
            db = new TabangHubEntities();

            _vw_listOfEvent = new BaseRepository<vw_ListOfEvent>();
            _orgEvents = new BaseRepository<OrgEvents>();
            _orgInfo = new BaseRepository<OrgInfo>();
            _orgEventImage = new BaseRepository<OrgEventImage>();
            _volunteerInfo = new BaseRepository<VolunteerInfo>();
            _volunteerHistory = new BaseRepository<VolunteersHistory>();
            _volunteerSkill = new BaseRepository<VolunteerSkill>();
            _skill = new BaseRepository<Skills>();

            _volunteerManager = new VolunteerManager();
            _organizationManager = new OrganizationManager();
        }


        public (List<Event> ongoingEvents, List<Event> pendingEvents, List<Event> eventHistory) GetEventParticipate(int userId)
        {
            List<Event> ongoingEvents = new List<Event>();
            var accepted = _volunteerManager.GetVolunteersEventParticipateByUserId(userId);
            var acce = accepted.OrderByDescending(m => m.applyVolunteerId).Select(e => _orgEvents.GetAll().FirstOrDefault(o => o.eventId == e.eventId)).ToList();
            foreach (var a in acce)
            {
                Event e = new Event
                {
                    eventId = a.eventId,
                    Title = a.eventTitle,
                    Details = a.eventDescription,
                    Start_Date = a.dateStart.Value.ToString("yyyy-MM-dd"),
                    End_Date = a.dateEnd.Value.ToString("yyyy-MM-dd"),
                    Image = _orgEventImage.GetAll().Where(m => m.eventId == a.eventId).Select(m => m.eventImage).FirstOrDefault()
                };
                ongoingEvents.Add(e);
            }

            List<Event> pendingEvents = new List<Event>();
            var pending = _volunteerManager.GetVolunteersEventPendingByUserId(userId);
            foreach (var p in pending)
            {
                var getEvent = _orgEvents.GetAll().Where(m => m.eventId == p.eventId && m.status == 1).FirstOrDefault();
                Event e = new Event
                {
                    eventId = (int)getEvent.eventId,
                    Title = getEvent.eventTitle,
                    Details = getEvent.eventDescription,
                    Start_Date = getEvent.dateStart.Value.ToString("yyyy-MM-dd"),
                    End_Date = getEvent.dateEnd.Value.ToString("yyyy-MM-dd"),
                    Status= getEvent.status,
                    Image = _orgEventImage.GetAll().Where(m => m.eventId == p.eventId).Select(m => m.eventImage).FirstOrDefault()
                };
                pendingEvents.Add(e);
            }

            List<Event> eventHistory = new List<Event>();
            var history = db.sp_VolunteerHistory(userId).ToList();
            foreach (var h in history)
            {
                Event e = new Event
                {
                    eventId = (int)h.eventId,
                    Title = h.eventTitle,
                    Details = h.eventDescription,
                    Start_Date = h.dateStart.Value.ToString("yyyy-MM-dd"),
                    End_Date = h.dateEnd.Value.ToString("yyyy-MM-dd"),
                    Status = h.status,
                    Image = _orgEventImage.GetAll().Where(m => m.eventId == h.eventId).Select(m => m.eventImage).FirstOrDefault()
                };
                if(!eventHistory.Any(m => m.eventId == e.eventId) && h.status == 2)
                {
                    eventHistory.Add(e);
                }
            }

            return (ongoingEvents, pendingEvents, eventHistory);
        }
    }
}