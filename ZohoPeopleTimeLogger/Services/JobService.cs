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
        private readonly IAuthenticationStorage authenticationStorage;

        public JobService(IZohoClient zohoClient, IAuthenticationStorage authenticationStorage)
        {
            this.zohoClient = zohoClient;
            this.authenticationStorage = authenticationStorage;
        }

        public async Task<string> GetJob(DateTime date)
        {
            var authData = authenticationStorage.GetAuthenticationData();
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

            var fullName = await GetFullName(authData.UserName);

            return jobsForThisDay.OrderByDescending(x => x.ToDate).First(x => x.Assignees.Any(y => y.Name == fullName)).JobId;
        }

        public async Task<string> GetFullName(string username)
        {
            var employees = await zohoClient.FetchRecord.GetByViewAsync("P_EmployeeView");

            var employeeRecord =
                employees.FirstOrDefault(
                    x => ((string)x["Email ID"]).Equals(username, StringComparison.OrdinalIgnoreCase));
            if (employeeRecord == null)
            {
                throw new Exception("Cannot find user Id. User name: " + username);
            }

            return employeeRecord["Last Name"] + " " + employeeRecord["First Name"];
        }
    }
}