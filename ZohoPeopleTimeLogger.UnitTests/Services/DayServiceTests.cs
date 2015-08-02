using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;
using ZohoPeopleClient;
using ZohoPeopleClient.Model.LeaveApi;
using ZohoPeopleClient.Model.TimeTrackerApi;
using ZohoPeopleTimeLogger.Exceptions;
using ZohoPeopleTimeLogger.Extensions;
using ZohoPeopleTimeLogger.Model;
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
            var zoho = new Mock<IZohoClient>();
            var auth = new Mock<IAuthenticationStorage>();
            var target = new DaysService(zoho.Object,  auth.Object);

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

        [Theory, AutoMoqData]
        public async void FillDays_FillWithCorrectValues(
            [Frozen] Mock<IZohoClient> zoho,
            [Frozen] Mock<IAuthenticationStorage> auth,
            DaysService target)
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

            var singleDayyOffHoliday = new Holiday
            {
                FromDate = startOfTheMonth,
                ToDate = startOfTheMonth,
                Name = "singleHoliday",
                Remarks = DaysService.DayOffHolidayRemark
            };

            var singleDayNotOffHoliday = new Holiday
            {
                FromDate = endOfTheMonth,
                ToDate = endOfTheMonth,
                Name = "singleNoDayOffHoliday",
                Remarks = "Could be given off."
            };

            var coupleDaysHoliday = new Holiday
                {
                    FromDate = middleOfTheMonth,
                    ToDate = middleOfTheMonth.AddDays(1),
                    Name = "twoDays",
                    Remarks = DaysService.DayOffHolidayRemark
            };

            var holidays = new List<Holiday>
            {
                singleDayyOffHoliday,
                singleDayNotOffHoliday,
                coupleDaysHoliday
            };

            var userName = "test";

            auth.Setup(x => x.GetAuthenticationData())
                .Returns(() => new AuthenticationData {UserName = userName, Token = "123"});
            var days = target.GetDays(startOfTheMonth);

            zoho.Setup(x => x.TimeTracker.TimeLog.GetAsync(userName, startOfTheMonth, endOfTheMonth, "all", "all"))
                .ReturnsAsync(timeLogs);
            zoho.Setup(x => x.Leave.GetHolidaysAsync(userName)).ReturnsAsync(holidays);

            // Act
            await target.FillDaysWithTimeLogsAsync(days, startOfTheMonth);

            // Assert
            zoho.Verify(x => x.TimeTracker.TimeLog.GetAsync(userName, startOfTheMonth, endOfTheMonth, "all", "all"));
            zoho.Verify(x => x.Leave.GetHolidaysAsync(userName));
            
            var firstDayInCalendar = days.First(x => x.Day == startOfTheMonth.Day);
            Assert.True(firstDayInCalendar.IsActive);
            Assert.True(firstDayInCalendar.IsFilled);
            Assert.Equal(TimeSpan.FromHours(8), firstDayInCalendar.Hours);
            Assert.True(firstDayInCalendar.IsHoliday);
            Assert.Equal(singleDayyOffHoliday.Name, firstDayInCalendar.HolidayName);

            var secondDayInCalendar = days.First(x => x.Day == middleOfTheMonth.Day);
            Assert.True(secondDayInCalendar.IsActive);
            Assert.True(secondDayInCalendar.IsFilled);
            Assert.Equal(TimeSpan.FromHours(14), secondDayInCalendar.Hours);
            Assert.True(secondDayInCalendar.IsHoliday);
            Assert.Equal(coupleDaysHoliday.Name, secondDayInCalendar.HolidayName);

            var secondMiddleDayInCalendar = days.First(x => x.Day == middleOfTheMonth.Day + 1);
            Assert.True(secondMiddleDayInCalendar.IsHoliday);
            Assert.Equal(coupleDaysHoliday.Name, secondMiddleDayInCalendar.HolidayName);

            var thirdDayInMonth = days.First(x => x.Day == endOfTheMonth.Day);
            Assert.True(thirdDayInMonth.IsActive);
            Assert.True(thirdDayInMonth.IsFilled);
            Assert.Equal(TimeSpan.FromHours(10), thirdDayInMonth.Hours);
            Assert.False(thirdDayInMonth.IsHoliday);
            Assert.Null(thirdDayInMonth.HolidayName);

            var notFilledDay = days.First(x => x.Day == 2);
            Assert.True(notFilledDay.IsActive);
            Assert.False(notFilledDay.IsFilled);
        }

        [Theory, AutoMoqData]
        public async void FillMissingTimeLogs_AllNotFilledAndActiveDayFilledWithDeafultJob(
            [Frozen] Mock<IZohoClient> zoho,
            [Frozen] Mock<IAuthenticationStorage> auth,
            DaysService target)
        {
            var user = "user";

            var startOfTheMonth = new DateTime(2015, 07, 01);
            var prevMonth = startOfTheMonth.AddMonths(-1);
            var nextMonth = startOfTheMonth.AddMonths(1);
            
            var days = new List<IDayViewModel>();

            var dayFromOtherMonth1 = new Mock<IDayViewModel>();
            days.Add(dayFromOtherMonth1.Object);

            var filledDay1 = new Mock<IDayViewModel>();
            filledDay1.Setup(x => x.IsActive).Returns(true);
            filledDay1.Setup(x => x.IsFilled).Returns(true);
            days.Add(filledDay1.Object);

            var notFilledDay1 = new Mock<IDayViewModel>();
            notFilledDay1.Setup(x => x.IsActive).Returns(true);
            notFilledDay1.Setup(x => x.IsFilled).Returns(false);
            notFilledDay1.Setup(x => x.Date).Returns(startOfTheMonth);
            days.Add(notFilledDay1.Object);

            var notFilledDay2 = new Mock<IDayViewModel>();
            notFilledDay2.Setup(x => x.IsActive).Returns(true);
            notFilledDay2.Setup(x => x.IsFilled).Returns(false);
            notFilledDay1.Setup(x => x.Date).Returns(startOfTheMonth.AddDays(1));
            days.Add(notFilledDay2.Object);
            
            var filledDay2 = new Mock<IDayViewModel>();
            filledDay2.Setup(x => x.IsActive).Returns(true);
            filledDay2.Setup(x => x.IsFilled).Returns(true);
            days.Add(filledDay2.Object);

            var dayFromOtherMonth2 = new Mock<IDayViewModel>();
            days.Add(dayFromOtherMonth2.Object);


            var job1 = new Job {JobId = "1", FromDate = prevMonth, ToDate = nextMonth};
            var job2 = new Job {JobId = "2", FromDate = new DateTime(2010, 11, 11), ToDate = new DateTime(2011, 11, 11)};

            zoho.Setup(x => x.TimeTracker.Jobs.GetAsync()).ReturnsAsync(new List<Job> {job1, job2});
            auth.Setup(x => x.GetAuthenticationData()).Returns(new AuthenticationData {UserName = user});
            
            // Act
            await target.FillMissingTimeLogsAsync(days);

            // Assert
            zoho.Verify(x => x.TimeTracker.Jobs.GetAsync(), Times.Once);

            dayFromOtherMonth1.Verify(x => x.FillHoursAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            filledDay1.Verify(x => x.FillHoursAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            notFilledDay1.Verify(x => x.FillHoursAsync(user, job1.JobId), Times.Once);
            notFilledDay2.Verify(x => x.FillHoursAsync(user, job1.JobId), Times.Once);
            dayFromOtherMonth2.Verify(x => x.FillHoursAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            filledDay2.Verify(x => x.FillHoursAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Theory, AutoMoqData]
        public  async void FillMissingTimeLogs_JobNotFound_ThrowException(
            [Frozen] Mock<IZohoClient> zoho,
            [Frozen] Mock<IAuthenticationStorage> auth,
            DaysService target)
        {
            var date = new DateTime(2015, 01, 01);
            var days = new List<IDayViewModel>
            {
                DayViewModel.DayFromThisMonth(1, date, zoho.Object)
            };

            zoho.Setup(x => x.TimeTracker.Jobs.GetAsync()).ReturnsAsync(new List<Job>
            {
                new Job
                {
                    FromDate = new DateTime(2010, 01, 01),
                    ToDate = new DateTime(2010, 02, 02)
                }
            });

            var ex = await Assert.ThrowsAsync<JobNotFoundException>(async () => await target.FillMissingTimeLogsAsync(days));
            Assert.Equal(date.MiddleOfMonth(), ex.Month);
        }

        [Theory, AutoMoqData]
        public async void FillMissingTimeLogs_JobsToDateAndFromDateTheSame_UseSecondJob(
            [Frozen] Mock<IZohoClient> zoho,
            [Frozen] Mock<IAuthenticationStorage> auth,
            DaysService target)
        {
            var user = "user";

            var startOfTheMonth = new DateTime(2015, 07, 01);
            var prevMonth = startOfTheMonth.AddMonths(-1);
            var nextMonth = startOfTheMonth.AddMonths(1);

            var days = new List<IDayViewModel>();

            var notFilledDay1 = new Mock<IDayViewModel>();
            notFilledDay1.Setup(x => x.IsActive).Returns(true);
            notFilledDay1.Setup(x => x.IsFilled).Returns(false);
            notFilledDay1.Setup(x => x.Date).Returns(startOfTheMonth);
            days.Add(notFilledDay1.Object);

            var job1 = new Job { JobId = "1", FromDate = prevMonth, ToDate = startOfTheMonth };
            var job2 = new Job { JobId = "2", FromDate = startOfTheMonth, ToDate = nextMonth };

            zoho.Setup(x => x.TimeTracker.Jobs.GetAsync()).ReturnsAsync(new List<Job> { job1, job2 });
            auth.Setup(x => x.GetAuthenticationData()).Returns(new AuthenticationData { UserName = user });

            // Act
            await target.FillMissingTimeLogsAsync(days);

            // Assert
            zoho.Verify(x => x.TimeTracker.Jobs.GetAsync(), Times.Once);

            notFilledDay1.Verify(x => x.FillHoursAsync(user, job2.JobId), Times.Once);
        }
    }
}