using System;

namespace ZohoPeopleTimeLogger.Events
{
    public class MonthChangedEventArgs : EventArgs
    {
        public DateTime NewMonth { get; private set; }

        public MonthChangedEventArgs(DateTime newMonth)
        {
            NewMonth = newMonth;
        }
    }
}