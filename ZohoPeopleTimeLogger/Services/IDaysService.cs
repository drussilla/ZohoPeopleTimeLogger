using System;
using System.Collections.Generic;
using ZohoPeopleClient.TimeTrackerApi;
using ZohoPeopleTimeLogger.ViewModel;

namespace ZohoPeopleTimeLogger.Services
{
    public interface IDaysService
    {
        List<DayViewModel> GetDays(DateTime month);
        void FillDaysWithTimeLogs(List<DayViewModel> days, List<TimeLog> timeLogs);
    }
}