using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;
using ZohoPeopleClient.Model.TimeTrackerApi;

namespace ZohoPeopleTimeLogger.ViewModel
{
    public interface IDayViewModel
    {
        bool IsActive { get; set; }
        bool IsFilled { get; set; }
        int Day { get; set; }
        TimeSpan Hours { get; }
        ICommand DeleteCommand { get; set; }
        DateTime Date { get; }
        string JobsDescription { get; }
        bool IsHoliday { get; }
        string HolidayName { get; }

        void FillLogs(List<TimeLog> logs);

        Task FillHoursAsync(string user, string jobId);
        
        void Clear();

        void MarkAsHoliday(string name);
    }
}