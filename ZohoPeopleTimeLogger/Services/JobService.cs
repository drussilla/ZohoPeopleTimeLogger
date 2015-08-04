using System;
using System.Linq;
using System.Threading.Tasks;
using ZohoPeopleClient;
using ZohoPeopleTimeLogger.Exceptions;

namespace ZohoPeopleTimeLogger.Services
{
    public class JobService : IJobService
    {
        private readonly IZohoClient zohoClient;

        public JobService(IZohoClient zohoClient)
        {
            this.zohoClient = zohoClient;
        }

        public async Task<string> GetJob(DateTime date)
        {
            var allJobs = await zohoClient.TimeTracker.Jobs.GetAsync();
            var jobsForThisDay = allJobs
                .Where(
                    x =>
                        x.FromDate <= date
                        && x.ToDate >= date)
                .ToList();

            if (jobsForThisDay == null || !jobsForThisDay.Any())
            {
                throw new JobNotFoundException(date);
            }

            return jobsForThisDay.OrderByDescending(x => x.ToDate).First().JobId;
        }
    }
}