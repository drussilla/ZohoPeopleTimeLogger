using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ZohoPeopleTimeLogger.ViewModel;

namespace ZohoPeopleTimeLogger.Services
{
    public interface IDaysService
    {
        List<IDayViewModel> GetDays(DateTime month);
        Task FillDaysWithTimeLogsAsync(List<IDayViewModel> days, DateTime month);
        Task FillMissingTimeLogsAsync(List<IDayViewModel> days);
        Task FillMissingTimeLogsAsync(IDayViewModel day);
    }
}