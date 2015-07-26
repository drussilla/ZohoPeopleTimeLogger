using System;

namespace ZohoPeopleTimeLogger.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime BeginOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime EndOfMonth(this DateTime date)
        {
            return date.BeginOfMonth().AddMonths(1).AddDays(-1);
        }

        public static DateTime MiddleOfMonth(this DateTime date)
        {
            return date.BeginOfMonth().AddDays(15);
        }
    }
}