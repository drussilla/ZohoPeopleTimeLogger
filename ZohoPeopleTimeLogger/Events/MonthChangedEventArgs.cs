using System;

namespace ZohoPeopleTimeLogger.Events
{
    public class MonthChangedEventArgs : EventArgs
    {
        public int Month { get; private set; }

        public int Year { get; private set; }

        public MonthChangedEventArgs(int year, int month)
        {
            Month = month;
            Year = year;
        }
    }
}