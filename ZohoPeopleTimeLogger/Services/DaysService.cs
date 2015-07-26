using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZohoPeopleClient;
using ZohoPeopleTimeLogger.Exceptions;
using ZohoPeopleTimeLogger.Extensions;
using ZohoPeopleTimeLogger.ViewModel;

namespace ZohoPeopleTimeLogger.Services
{
    public class DaysService : IDaysService
    {
        private readonly IZohoClient zohoClient;
        private readonly IAuthenticationStorage auth;
        public const int MaximumWorkingDaysInMonth = 25;

        public DaysService(IZohoClient zohoClient, IAuthenticationStorage auth)
        {
            this.zohoClient = zohoClient;
            this.auth = auth;
        }

        public List<IDayViewModel> GetDays(DateTime month)
        {
            var startOfMonth = new DateTime(month.Year, month.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            
            var days = new List<IDayViewModel>();

            int firstDayIndex = (int)startOfMonth.DayOfWeek - 1;
            int currentDay = 1;

            if (startOfMonth.DayOfWeek == DayOfWeek.Sunday)
            {
                currentDay++;
                firstDayIndex = 0;
            }
            else if (startOfMonth.DayOfWeek == DayOfWeek.Saturday)
            {
                currentDay += 2;
                firstDayIndex = 0;
            }

            for (int i = 0; i < MaximumWorkingDaysInMonth; i++)
            {
                if (i >= firstDayIndex)
                {
                    if (currentDay > endOfMonth.Day)
                    {
                        days.Add(DayViewModel.DayFromOtherMonth(zohoClient));
                    }
                    else
                    {
                        days.Add(DayViewModel.DayFromThisMonth(currentDay, month, zohoClient));

                        if ((i + 1) % 5 == 0)
                        {
                            currentDay += 3;
                        }
                        else
                        {
                            currentDay++;
                        }
                    }
                }
                else
                {
                    days.Add(DayViewModel.DayFromOtherMonth(zohoClient));
                }
            }

            return days;
        }

        public async Task FillDaysWithTimeLogsAsync(List<IDayViewModel> days, DateTime month)
        {
            var timeLogs = await zohoClient.TimeTracker.TimeLog.GetAsync(
                    auth.GetAuthenticationData().UserName,
                    month.BeginOfMonth(),
                    month.EndOfMonth());

            var groupedByDate = timeLogs.GroupBy(x => x.WorkDate);

            foreach (var itemsInLog in groupedByDate)
            {
                var dayToFill = days.FirstOrDefault(x => x.Day == itemsInLog.Key.Day);
                if (dayToFill != null)
                {
                    dayToFill.FillLogs(itemsInLog.ToList());
                }
            }
        }

        public async Task FillMissingTimeLogsAsync(List<IDayViewModel> days)
        {
            var daysToFill = days.Where(x => x.IsActive && !x.IsFilled).ToList();
            var jobId = await FindJobForThisMonth(daysToFill.First().Date.MiddleOfMonth());
            
            foreach (var dayViewModel in daysToFill)
            {
                await dayViewModel.FillHoursAsync(auth.GetAuthenticationData().UserName, jobId);
            }
        }

        private async Task<string> FindJobForThisMonth(DateTime date)
        {
            var allJobs = await zohoClient.TimeTracker.Jobs.GetAsync();
            var jobFromThisMonth = allJobs.FirstOrDefault(
                x =>
                    x.FromDate <= date
                    && x.ToDate >= date);

            if (jobFromThisMonth == null)
            {
                throw new JobNotFoundException(date);
            }

            return jobFromThisMonth.JobId;
        }
    }
}