using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;
using ZohoPeopleClient;
using ZohoPeopleClient.TimeTrackerApi;
using ZohoPeopleTimeLogger.Extensions;
using ZohoPeopleTimeLogger.Services;
using ZohoPeopleTimeLogger.ViewModel;

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

        [Fact]
        public void FillDays_FillWithCorrectValues()
        {
            var startOfTheMonth = new DateTime(2015, 07, 01);
            var middleOfTheMonth = new DateTime(2015, 07, 15);
            var endOfTheMonth = startOfTheMonth.EndOfMonth();
            
            var firstLog = new TimeLog { WorkDate = startOfTheMonth, Hours = TimeSpan.FromHours(8) };
            var secondLog1 = new TimeLog { WorkDate = middleOfTheMonth, Hours = TimeSpan.FromHours(2) };
            var secondLog2 = new TimeLog { WorkDate = middleOfTheMonth, Hours = TimeSpan.FromHours(12) };
            var thirdLog = new TimeLog { WorkDate = endOfTheMonth, Hours = TimeSpan.FromHours(10) };

            var timeLogs = new List<TimeLog>
            {
                firstLog,
                secondLog1,
                secondLog2,
                thirdLog
            };

            var target = new DaysService();
            var days = target.GetDays(startOfTheMonth);

            // Act
            target.FillDaysWithTimeLogs(days, timeLogs);

            // Assert
            var firstDayInCalendar = days.First(x => x.Day == startOfTheMonth.Day);
            Assert.True(firstDayInCalendar.IsActive);
            Assert.True(firstDayInCalendar.IsFilled);
            Assert.Equal(TimeSpan.FromHours(8), firstDayInCalendar.Hours);

            var secondDayInCalendar = days.First(x => x.Day == middleOfTheMonth.Day);
            Assert.True(secondDayInCalendar.IsActive);
            Assert.True(secondDayInCalendar.IsFilled);
            Assert.Equal(TimeSpan.FromHours(14), secondDayInCalendar.Hours);

            var thirdDayInMonth = days.First(x => x.Day == endOfTheMonth.Day);
            Assert.True(thirdDayInMonth.IsActive);
            Assert.True(thirdDayInMonth.IsFilled);
            Assert.Equal(TimeSpan.FromHours(10), thirdDayInMonth.Hours);

            var notFilledDay = days.First(x => x.Day == 2);
            Assert.True(notFilledDay.IsActive);
            Assert.False(notFilledDay.IsFilled);
        }
    }
}