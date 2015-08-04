using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;
using ZohoPeopleClient;
using ZohoPeopleClient.Model.TimeTrackerApi;
using ZohoPeopleTimeLogger.ViewModel;

namespace ZohoPeopleTimeLogger.UnitTests.ViewModels
{
    public class DayViewModelTests
    {

        [Theory, AutoMoqData]
        public void FillFillHoursAsync_FillHoursAndAmeActive(
            [Frozen] Mock<IZohoClient> zoho)
        {
            var user = "user";
            string jobId = "1";

            var target = DayViewModel.DayFromThisMonth(1, new DateTime(2015, 01, 01), zoho.Object);
            zoho.Setup(x => x.TimeTracker.TimeLog.GetAsync(user, target.Date, target.Date, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<TimeLog>());

            target.FillHoursAsync(user, jobId);

            zoho.Verify(
                x => x.TimeTracker.TimeLog.AddAsync(user, target.Date, jobId, TimeSpan.FromHours(8), "non-billable"));
            zoho.Verify(x => x.TimeTracker.TimeLog.GetAsync(user, target.Date, target.Date, "all", "all"));
           
            Assert.True(target.IsActive);
            Assert.True(target.IsFilled);
        }

        [Theory, AutoMoqData]
        public void Clear_AllInfoInDayCleared(
            [Frozen] Mock<IZohoClient> zoho,
            TimeLog log,
            TimeLog log2)
        {
            var target = FilledDay(zoho, log, log2, true);

            target.Clear();

            Assert.True(target.IsActive);
            Assert.False(target.IsFilled);
            Assert.False(target.IsHoliday);
            Assert.Null(target.HolidayName);
        }

        [Theory, AutoMoqData]
        public void MarkAsHoliday_DayIsMarkedAsHoliday(
            [Frozen] Mock<IZohoClient> zoho,
            TimeLog log,
            TimeLog log2)
        {
            var target = FilledDay(zoho, log, log2, false);

            target.MarkAsHoliday("test");

            Assert.True(target.IsHoliday);
            Assert.Equal("test", target.HolidayName);
        }


        [Theory, AutoMoqData]
        public void Delete_DeleteAllJobsAndClearDay(
            [Frozen] Mock<IZohoClient> zoho, 
            TimeLog log,
            TimeLog log2)
        {
            var target = FilledDay(zoho, log, log2, true);

            target.DeleteCommand.Execute(null);

            Assert.True(target.IsActive);
            Assert.False(target.IsFilled);
            Assert.True(target.IsHoliday);
            Assert.Equal("test", target.HolidayName);

            zoho.Verify(x => x.TimeTracker.TimeLog.DeleteAsync(log.TimelogId), Times.Once);
            zoho.Verify(x => x.TimeTracker.TimeLog.DeleteAsync(log2.TimelogId), Times.Once);
        }

        private static IDayViewModel FilledDay(Mock<IZohoClient> zoho, TimeLog log, TimeLog log2, bool isHoliday)
        {
            var logs = new List<TimeLog>
            {
                log,
                log2
            };
            var target = DayViewModel.DayFromThisMonth(1, new DateTime(2015, 01, 01), zoho.Object);
            target.FillLogs(logs);
            if (isHoliday)
            {
                target.MarkAsHoliday("test");
            }

            return target;
        }
    }
}