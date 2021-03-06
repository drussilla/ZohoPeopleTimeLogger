﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;
using ZohoPeopleClient;
using ZohoPeopleClient.Model.LeaveApi;
using ZohoPeopleClient.Model.TimeTrackerApi;
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
            var job = new Mock<IJobService>();
            var target = new DaysService(zoho.Object,  auth.Object, job.Object);

            var days = target.GetDays(month);

            for (int i = 0; i < DaysService.TotalDaysInATable; i++)
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
        public async void FillDays_TimeLogRecieved_FilledWithCorrectValues(
            [Frozen] Mock<IZohoClient> zoho,
            [Frozen] Mock<IAuthenticationStorage> auth,
            AuthenticationData authData,
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

            auth.Setup(x => x.GetAuthenticationData())
                .Returns(authData);
            var days = target.GetDays(startOfTheMonth);

            zoho.Setup(x => x.TimeTracker.TimeLog.GetAsync(authData.Id, startOfTheMonth, endOfTheMonth, "all", "all"))
                .ReturnsAsync(timeLogs);
            
            // Act
            await target.FillDaysWithTimeLogsAsync(days, startOfTheMonth);

            // Assert
            zoho.Verify(x => x.TimeTracker.TimeLog.GetAsync(authData.Id, startOfTheMonth, endOfTheMonth, "all", "all"));
            zoho.Verify(x => x.Leave.GetHolidaysAsync(authData.Id));
            
            var firstDayInCalendar = days.First(x => x.Day == startOfTheMonth.Day);
            Assert.True(firstDayInCalendar.IsActive);
            Assert.True(firstDayInCalendar.IsFilled);
            Assert.Equal(TimeSpan.FromHours(8), firstDayInCalendar.Hours);
            Assert.False(firstDayInCalendar.IsHoliday);
            
            var secondDayInCalendar = days.First(x => x.Day == middleOfTheMonth.Day);
            Assert.True(secondDayInCalendar.IsActive);
            Assert.True(secondDayInCalendar.IsFilled);
            Assert.Equal(TimeSpan.FromHours(14), secondDayInCalendar.Hours);
            Assert.False(secondDayInCalendar.IsHoliday);
            
            var secondMiddleDayInCalendar = days.First(x => x.Day == middleOfTheMonth.Day + 1);
            Assert.False(secondMiddleDayInCalendar.IsHoliday);
            
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
        public async void FillDays_HolidaysRecieved_FilledWithCorrectValues(
            [Frozen] Mock<IZohoClient> zoho,
            [Frozen] Mock<IAuthenticationStorage> auth,
            AuthenticationData authData,
            DaysService target)
        {
            var startOfTheMonth = new DateTime(2015, 07, 01);
            var middleOfTheMonth = new DateTime(2015, 07, 15);
            var endOfTheMonth = startOfTheMonth.EndOfMonth();

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

            
            auth.Setup(x => x.GetAuthenticationData())
                .Returns(authData);
            var days = target.GetDays(startOfTheMonth);

            zoho.Setup(x => x.Leave.GetHolidaysAsync(It.IsAny<string>())).ReturnsAsync(holidays);

            // Act
            await target.FillDaysWithTimeLogsAsync(days, startOfTheMonth);

            // Assert
            zoho.Verify(x => x.Leave.GetHolidaysAsync(authData.Id));

            var firstDayInCalendar = days.First(x => x.Day == startOfTheMonth.Day);
            Assert.True(firstDayInCalendar.IsHoliday);
            Assert.Equal(singleDayyOffHoliday.Name, firstDayInCalendar.HolidayName);

            var secondDayInCalendar = days.First(x => x.Day == middleOfTheMonth.Day);
            Assert.True(secondDayInCalendar.IsHoliday);
            Assert.Equal(coupleDaysHoliday.Name, secondDayInCalendar.HolidayName);

            var secondMiddleDayInCalendar = days.First(x => x.Day == middleOfTheMonth.Day + 1);
            Assert.True(secondMiddleDayInCalendar.IsHoliday);
            Assert.Equal(coupleDaysHoliday.Name, secondMiddleDayInCalendar.HolidayName);

            var thirdDayInMonth = days.First(x => x.Day == endOfTheMonth.Day);
            Assert.False(thirdDayInMonth.IsHoliday);
            Assert.Null(thirdDayInMonth.HolidayName);
        }

        //TODO: fix test
        //[Theory, AutoMoqData]
        //public async void FillDays_VacationsRecieved_FilledWithCorrectValues(
        //    [Frozen] Mock<IZohoClient> zoho,
        //    [Frozen] Mock<IAuthenticationStorage> auth,
        //    AuthenticationData authData,
        //    DaysService target)
        //{
        //    var startOfTheMonth = new DateTime(2015, 08, 03);
        //    var middleOfTheMonth = startOfTheMonth.MiddleOfMonth();
        //    var endOfTheMonth = startOfTheMonth.EndOfMonth();

        //    dynamic vacation1 = new Dictionary<string, string>();
        //    vacation1["From"] = middleOfTheMonth.AddDays(-1).ToString("dd-MMM-yyyy");
        //    vacation1["To"] = middleOfTheMonth.AddDays(1).ToString("dd-MMM-yyyy");
        //    vacation1["ApprovalStatus"] = "Approved";

        //    dynamic vacation2 = new Dictionary<string, string>();
        //    vacation2["From"] = middleOfTheMonth.AddDays(5).ToString("dd-MMM-yyyy");
        //    vacation2["To"] = endOfTheMonth.ToString("dd-MMM-yyyy");
        //    vacation2["ApprovalStatus"] = "Approved";

        //    dynamic vacation3 = new Dictionary<string, string>();
        //    vacation3["From"] = startOfTheMonth.ToString("dd-MMM-yyyy");
        //    vacation3["To"] = startOfTheMonth.AddDays(1).ToString("dd-MMM-yyyy");
        //    vacation3["ApprovalStatus"] = "Canceled";

        //    var vacations = new List<dynamic>
        //    {
        //        vacation1,
        //        vacation2,
        //        vacation3
        //    };

        //    var viewName = "leave";
        //    var holidayName = "Vacation";
            
        //    auth.Setup(x => x.GetAuthenticationData())
        //        .Returns(authData);
        //    var days = target.GetDays(startOfTheMonth);

        //    zoho.Setup(x => x.FetchRecord.GetByFormAsync(It.IsAny<string>())).ReturnsAsync(vacations);

        //    // Act
        //    await target.FillDaysWithTimeLogsAsync(days, startOfTheMonth);

        //    // Assert
        //    zoho.Verify(x => x.FetchRecord.GetByFormAsync(viewName), Times.Once);

        //    var firstDayInCalendar = days.First(x => x.Day == startOfTheMonth.Day);
        //    Assert.False(firstDayInCalendar.IsHoliday);
        //    Assert.Null(firstDayInCalendar.HolidayName);

        //    var secondDayInCalendar = days.First(x => x.Day == middleOfTheMonth.Day + 1);
        //    Assert.True(secondDayInCalendar.IsHoliday);
        //    Assert.Equal(holidayName, secondDayInCalendar.HolidayName);

        //    var secondMiddleDayInCalendar = days.First(x => x.Day == middleOfTheMonth.Day + 2);
        //    Assert.False(secondMiddleDayInCalendar.IsHoliday);
        //    Assert.Null(secondMiddleDayInCalendar.HolidayName);

        //    var thirdDayInMonth = days.First(x => x.Day == endOfTheMonth.Day);
        //    Assert.True(thirdDayInMonth.IsHoliday);
        //    Assert.Equal(holidayName, thirdDayInMonth.HolidayName);

        //    var beginningOfTheMonth = days.First(x => x.Day == startOfTheMonth.Day);
        //    Assert.False(beginningOfTheMonth.IsHoliday);
        //    Assert.Null(beginningOfTheMonth.HolidayName);
        //}

        [Theory, AutoMoqData]
        public async void FillMissingTimeLogs_AllNotFilledAndActiveDayFilledWithDeafultJob(
            [Frozen] Mock<IAuthenticationStorage> auth,
            [Frozen] Mock<IJobService> job,
            AuthenticationData authData,
            DaysService target)
        {
            var startOfTheMonth = new DateTime(2015, 07, 01);
            
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
            notFilledDay2.Setup(x => x.Date).Returns(startOfTheMonth.AddDays(1));
            days.Add(notFilledDay2.Object);

            var notFilledHolidayDay = new Mock<IDayViewModel>();
            notFilledHolidayDay.Setup(x => x.IsActive).Returns(true);
            notFilledHolidayDay.Setup(x => x.IsFilled).Returns(false);
            notFilledHolidayDay.Setup(x => x.IsHoliday).Returns(true);
            notFilledHolidayDay.Setup(x => x.Date).Returns(startOfTheMonth.AddDays(2));
            days.Add(notFilledHolidayDay.Object);

            var filledDay2 = new Mock<IDayViewModel>();
            filledDay2.Setup(x => x.IsActive).Returns(true);
            filledDay2.Setup(x => x.IsFilled).Returns(true);
            days.Add(filledDay2.Object);

            var dayFromOtherMonth2 = new Mock<IDayViewModel>();
            days.Add(dayFromOtherMonth2.Object);

            var jobId = "1";

            job.Setup(x => x.GetJob(It.IsAny<DateTime>())).ReturnsAsync(jobId);
            auth.Setup(x => x.GetAuthenticationData()).Returns(authData);
            
            // Act
            await target.FillMissingTimeLogsAsync(days);

            // Assert
            job.Verify(x => x.GetJob(startOfTheMonth), Times.Once);

            dayFromOtherMonth1.Verify(x => x.FillHoursAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            filledDay1.Verify(x => x.FillHoursAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            notFilledDay1.Verify(x => x.FillHoursAsync(authData.Id, jobId), Times.Once);
            notFilledDay2.Verify(x => x.FillHoursAsync(authData.Id, jobId), Times.Once);
            notFilledHolidayDay.Verify(x => x.FillHoursAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            dayFromOtherMonth2.Verify(x => x.FillHoursAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            filledDay2.Verify(x => x.FillHoursAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Theory, AutoMoqData]
        public async void FillMissingTimeLogs_SingleDay(
           [Frozen] Mock<IAuthenticationStorage> auth,
           [Frozen] Mock<IJobService> job,
           AuthenticationData authData,
           DaysService target)
        {
            var startOfTheMonth = new DateTime(2015, 07, 01);

            var notFilledDay1 = new Mock<IDayViewModel>();
            notFilledDay1.Setup(x => x.IsActive).Returns(true);
            notFilledDay1.Setup(x => x.IsFilled).Returns(false);
            notFilledDay1.Setup(x => x.Date).Returns(startOfTheMonth);

            var jobId = "1";

            job.Setup(x => x.GetJob(It.IsAny<DateTime>())).ReturnsAsync(jobId);
            auth.Setup(x => x.GetAuthenticationData()).Returns(authData);

            // Act
            await target.FillMissingTimeLogsAsync(notFilledDay1.Object);

            // Assert
            job.Verify(x => x.GetJob(startOfTheMonth), Times.Once);

            notFilledDay1.Verify(x => x.FillHoursAsync(authData.Id, jobId), Times.Once);
        }
    }
}