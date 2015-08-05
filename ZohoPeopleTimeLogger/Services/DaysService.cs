using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZohoPeopleClient;
using ZohoPeopleTimeLogger.Extensions;
using ZohoPeopleTimeLogger.ViewModel;

namespace ZohoPeopleTimeLogger.Services
{
    public class DaysService : IDaysService
    {
        private readonly IZohoClient zohoClient;
        private readonly IAuthenticationStorage auth;
        private readonly IJobService jobService;

        public const string DayOffHolidayRemark = "Day off for all employees";
        public const int TotalDaysInATable = 25;
        
        public DaysService(IZohoClient zohoClient, IAuthenticationStorage auth, IJobService jobService)
        {
            this.zohoClient = zohoClient;
            this.auth = auth;
            this.jobService = jobService;
        }

        public List<IDayViewModel> GetDays(DateTime month)
        {
            var startOfMonth = month.BeginOfMonth();
            var endOfMonth = month.EndOfMonth();
            
            var days = new List<IDayViewModel>();

            int firstDayOfMonthIndex = (int)startOfMonth.DayOfWeek - 1;
            int currentMonthDay = 1;

            if (startOfMonth.DayOfWeek == DayOfWeek.Sunday)
            {
                currentMonthDay++;
                firstDayOfMonthIndex = 0;
            }
            else if (startOfMonth.DayOfWeek == DayOfWeek.Saturday)
            {
                currentMonthDay += 2;
                firstDayOfMonthIndex = 0;
            }

            for (int dayIndexInTable = 0; dayIndexInTable < TotalDaysInATable; dayIndexInTable++)
            {
                if (dayIndexInTable >= firstDayOfMonthIndex)
                {
                    if (currentMonthDay > endOfMonth.Day)
                    {
                        days.Add(DayViewModel.DayFromOtherMonth(zohoClient));
                    }
                    else
                    {
                        days.Add(DayViewModel.DayFromThisMonth(currentMonthDay, month, zohoClient));

                        if (IsWeekEnded(dayIndexInTable))
                        {
                            currentMonthDay += 3;
                        }
                        else
                        {
                            currentMonthDay++;
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

        private static bool IsWeekEnded(int dayOfMonth)
        {
            return (dayOfMonth + 1) % 5 == 0;
        }

        public async Task FillDaysWithTimeLogsAsync(List<IDayViewModel> days, DateTime month)
        {
            await FillTimeLogs(days, month);
            await FillHolidays(days);
        }

        private async Task FillTimeLogs(List<IDayViewModel> days, DateTime month)
        {
            var timeLogs = await zohoClient.TimeTracker.TimeLog.GetAsync(
                auth.GetAuthenticationData().UserName,
                month.BeginOfMonth(),
                month.EndOfMonth());

            if (timeLogs == null || !timeLogs.Any())
            {
                return;
            }

            var groupedByDate = timeLogs.GroupBy(x => x.WorkDate);

            foreach (var itemsInLog in groupedByDate)
            {
                var dayToFill = days.FirstOrDefault(x => x.Day == itemsInLog.Key.Day);
                dayToFill?.FillLogs(itemsInLog.ToList());
            }
        }

        private async Task FillHolidays(List<IDayViewModel> days)
        {
            var holidays = await zohoClient.Leave.GetHolidaysAsync(auth.GetAuthenticationData().UserName);

            if (holidays == null || !holidays.Any())
            {
                return;
            }

            foreach (var day in days)
            {
                var holiday = holidays.FirstOrDefault(x => x.FromDate <= day.Date && x.ToDate >= day.Date);
                if (holiday != null && 
                    holiday.Remarks.Equals(DayOffHolidayRemark, StringComparison.OrdinalIgnoreCase))
                {
                    day.MarkAsHoliday(holiday.Name);
                }
            }
        }

        public async Task FillMissingTimeLogsAsync(List<IDayViewModel> days)
        {
            var daysToFill = days.Where(x => x.IsActive && !x.IsFilled && !x.IsHoliday).ToList();
            var jobId = await jobService.GetJob(daysToFill.First().Date);
            
            foreach (var dayViewModel in daysToFill)
            {
                await dayViewModel.FillHoursAsync(auth.GetAuthenticationData().UserName, jobId);
            }
        }

        public async Task FillMissingTimeLogsAsync(IDayViewModel day)
        {
            var jobId = await jobService.GetJob(day.Date);
            await day.FillHoursAsync(auth.GetAuthenticationData().UserName, jobId);
        }
    }
}