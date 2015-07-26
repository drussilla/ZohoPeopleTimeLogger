using System;
using System.Collections;
using Moq;
using Xunit;
using ZohoPeopleClient;
using ZohoPeopleTimeLogger.Services;

namespace ZohoPeopleTimeLogger.UnitTests.Services
{
    public class DayServiceTests
    {
        public static IEnumerable GetDays_CorrectDays_Data()
        {
            yield return new object[] { new DateTime(2014, 07, 01), 1, 23 };
            yield return new object[] { new DateTime(2014, 08, 01), 4, 24 };
            yield return new object[] { new DateTime(2014, 09, 01), 0, 21 };
            yield return new object[] { new DateTime(2014, 11, 01), 0, 19 };
            yield return new object[] { new DateTime(2015, 02, 01), 0, 19 };
            yield return new object[] { new DateTime(2015, 05, 01), 4, 24 };
        }

        [Theory]
        [MemberData("GetDays_CorrectDays_Data")]
        public void GetDays_CorrectDays(
            DateTime month, 
            int firstDayOfMonthIndex, 
            int lastDayOfMonthIndex)
        {
            var target = new DaysService();

            var days = target.GetDays(month);

            for (int i = 0; i < DaysService.MaximumWorkingDaysInMonth; i++)
            {
                if (i >= firstDayOfMonthIndex && i <= lastDayOfMonthIndex)
                {
                    Assert.True(days[i].IsActive, string.Format("Day is not active. {0}", month));
                    var dayOfWeekActual = i%5;
                    var dayOfWeekExpected = (int)new DateTime(month.Year, month.Month, days[i].Day).DayOfWeek - 1;
                    Assert.Equal(dayOfWeekExpected, dayOfWeekActual);
                }
                else
                {
                    Assert.False(days[i].IsActive, string.Format("Day is active. {0}", month));
                }
            }
        }
    }
}