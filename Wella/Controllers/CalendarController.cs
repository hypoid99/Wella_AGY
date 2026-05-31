using System;
using System.Collections.Generic;
using System.Linq;
using Wella.Common;
using Wella.Models;

namespace Wella.Controllers
{
    public class CalendarController
    {
        private readonly StorageManager _storage;
        private List<CalendarEvent> _events = new List<CalendarEvent>();

        public CalendarController(StorageManager storage)
        {
            _storage = storage;
            LoadEvents();
        }

        public void LoadEvents()
        {
            _events = _storage.LoadFromFile(AppConfig.CalendarDataPath, new List<CalendarEvent>());
        }

        public void SaveEvents()
        {
            _storage.SaveToFile(AppConfig.CalendarDataPath, _events);
        }

        public List<CalendarEvent> GetEventsForDate(DateTime date)
        {
            return _events.Where(e => e.EventDate.Date == date.Date).ToList();
        }

        public List<CalendarEvent> GetEventsForMonth(int year, int month)
        {
            return _events.Where(e => e.EventDate.Year == year && e.EventDate.Month == month).ToList();
        }

        public void AddEvent(CalendarEvent ev)
        {
            _events.Add(ev);
            SaveEvents();
        }

        public bool DeleteEvent(string id)
        {
            var ev = _events.FirstOrDefault(e => e.Id == id);
            if (ev != null)
            {
                _events.Remove(ev);
                SaveEvents();
                return true;
            }
            return false;
        }
    }
}
