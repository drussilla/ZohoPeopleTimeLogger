using System;

namespace ZohoPeopleTimeLogger.Services
{
    public class DateTimeService : IDateTimeService
    {
        public DateTime Now
        {
            get { return DateTime.Now; }
        }
    }
}