using System;
using System.Collections.Generic;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;
using ZohoPeopleClient;
using ZohoPeopleClient.Model.TimeTrackerApi;
using ZohoPeopleTimeLogger.Exceptions;
using ZohoPeopleTimeLogger.Extensions;
using ZohoPeopleTimeLogger.Services;

namespace ZohoPeopleTimeLogger.UnitTests.Services
{
    public class JobServiceTests
    {
        [Theory, AutoMoqData]
        public async void GetJob_JobsToDateAndFromDateTheSame_UseSecondJob(
            [Frozen] Mock<IZohoClient> zohoClient,
            JobService target)
        {
            
            var startOfTheMonth = new DateTime(2015, 07, 01);
            var prevMonth = startOfTheMonth.AddMonths(-1);
            var nextMonth = startOfTheMonth.AddMonths(1);

            var job1 = new Job { JobId = "1", FromDate = prevMonth, ToDate = startOfTheMonth };
            var job2 = new Job { JobId = "2", FromDate = startOfTheMonth, ToDate = nextMonth };

            zohoClient.Setup(x => x.TimeTracker.Jobs.GetAsync()).ReturnsAsync(new List<Job> {job1, job2});

            // Act
            var jobId = await target.GetJob(startOfTheMonth);

            // Assert
            zohoClient.Verify(x => x.TimeTracker.Jobs.GetAsync(), Times.Once);

            Assert.Equal(job2.JobId, jobId);
        }

        [Theory, AutoMoqData]
        public async void GetJob_JobsIntersect_UseJobWithLaterToDate(
            [Frozen] Mock<IZohoClient> zohoClient,
            JobService target)
        {
            var startFirst = new DateTime(2015, 07, 01);
            var endFirst = startFirst.EndOfMonth();

            var startSecond = new DateTime(2015, 07, 15);
            var endSecond = startSecond.AddMonths(1).EndOfMonth();

            var job1 = new Job { JobId = "1", FromDate = startFirst, ToDate = endFirst };
            var job2 = new Job { JobId = "2", FromDate = startSecond, ToDate = endSecond };

            zohoClient.Setup(x => x.TimeTracker.Jobs.GetAsync()).ReturnsAsync(new List<Job> { job1, job2 });

            // Act
            var jobId = await target.GetJob(startSecond.AddDays(1));

            // Assert
            zohoClient.Verify(x => x.TimeTracker.Jobs.GetAsync(), Times.Once);

            Assert.Equal(job2.JobId, jobId);
        }

        [Theory, AutoMoqData]
        public async void GetJob_JobsToDateAndFromDateDifferent_ReturnAllJobs(
            [Frozen] Mock<IZohoClient> zohoClient,
            JobService target)
        {

            var startFirst = new DateTime(2015, 07, 01);
            var endFirst = startFirst.EndOfMonth();

            var startSecond = new DateTime(2015, 08, 01);
            var endSecond = startSecond.EndOfMonth();

            var job1 = new Job { JobId = "1", FromDate = startFirst, ToDate = endFirst };
            var job2 = new Job { JobId = "2", FromDate = startSecond, ToDate = endSecond };

            zohoClient.Setup(x => x.TimeTracker.Jobs.GetAsync()).ReturnsAsync(new List<Job> { job1, job2 });

            // Act
            var firstJobId1 = await target.GetJob(startFirst);
            var firstJobId2 = await target.GetJob(endFirst);

            var secondJobId1 = await target.GetJob(startSecond);
            var secondJobId2 = await target.GetJob(endSecond);

            // Assert
            zohoClient.Verify(x => x.TimeTracker.Jobs.GetAsync(), Times.Exactly(4));

            Assert.Equal(job1.JobId, firstJobId1);
            Assert.Equal(job1.JobId, firstJobId2);

            Assert.Equal(job2.JobId, secondJobId1);
            Assert.Equal(job2.JobId, secondJobId2);
        }

        [Theory, AutoMoqData]
        public async void GetJob_JobNotFound_ThrowException(
            [Frozen] Mock<IZohoClient> zoho,
            JobService target)
        {
            var date = new DateTime(2015, 01, 01);
            
            zoho.Setup(x => x.TimeTracker.Jobs.GetAsync()).ReturnsAsync(new List<Job>
            {
                new Job
                {
                    FromDate = new DateTime(2010, 01, 01),
                    ToDate = new DateTime(2010, 02, 02)
                }
            });

            var ex = await Assert.ThrowsAsync<JobNotFoundException>(async () => await target.GetJob(date));
            Assert.Equal(date, ex.Month);
        }
    }
}