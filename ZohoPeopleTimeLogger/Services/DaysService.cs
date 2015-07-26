using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZohoPeopleTimeLogger.ViewModel;

namespace ZohoPeopleTimeLogger.Services
{
    public class DaysService : IDaysService
    {
        public const int MaximumWorkingDaysInMonth = 25;

        public List<DayViewModel> GetDays(DateTime month)
        {
            var startOfMonth = new DateTime(month.Year, month.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
            
            var days = new List<DayViewModel>();

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
                        days.Add(DayViewModel.DayFromOtherMonth());
                    }
                    else
                    {
                        days.Add(DayViewModel.DayFromThisMonth(currentDay));

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
                    days.Add(DayViewModel.DayFromOtherMonth());
                }
            }

            return days;
        }
    }
}