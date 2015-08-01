using System;
using System.Collections.Generic;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;
using ZohoPeopleClient;
using ZohoPeopleClient.Model.TimeTrackerApi;
using ZohoPeopleClient.TimeTrackerApi;
using ZohoPeopleTimeLogger.ViewModel;

namespace ZohoPeopleTimeLogger.UnitTests
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
        public void Delete_DeleteAllJobsAndClearDay([Frozen] Mock<IZohoClient> zoho, TimeLog log, TimeLog log2)
        {
            var logs = new List<TimeLog>
            {
                log,
                log2
            };
            var target = DayViewModel.DayFromThisMonth(1, new DateTime(2015, 01, 01), zoho.Object);
            target.FillLogs(logs);

            target.DeleteCommand.Execute(null);

            Assert.True(target.IsActive);
            Assert.False(target.IsFilled);

            zoho.Verify(x => x.TimeTracker.TimeLog.DeleteAsync(log.TimelogId), Times.Once);
            zoho.Verify(x => x.TimeTracker.TimeLog.DeleteAsync(log2.TimelogId), Times.Once);
        }
    }
}