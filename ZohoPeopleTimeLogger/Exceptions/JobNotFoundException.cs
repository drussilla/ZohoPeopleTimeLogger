using System;

namespace ZohoPeopleTimeLogger.Exceptions
{
    public class JobNotFoundException : Exception
    {
        public DateTime Month { get; private set; }

        public JobNotFoundException(DateTime month) : base("Job for this month " + month.ToString("MMMM yyyy") + " not found")
        {
            Month = month;
        }
    }
}