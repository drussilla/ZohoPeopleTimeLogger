using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using ZohoPeopleClient.TimeTrackerApi;

namespace ZohoPeopleTimeLogger.ViewModel
{
    public class DayViewModel : ViewModelBase
    {
        private List<TimeLog> timeLogs;

        public static DayViewModel DayFromOtherMonth()
        {
            return new DayViewModel { IsActive = false };
        }

        public static DayViewModel DayFromThisMonth(int day)
        {
            return new DayViewModel { IsActive = true, Day = day };
        }

        public void FillLogs(List<TimeLog> logs)
        {
            timeLogs = logs;
            IsFilled = true;
            RaisePropertyChanged("Hours");
        }

        public bool IsActive { get; set; }

        public bool IsFilled { get; set; }

        public int Day { get; set; }

        public TimeSpan Hours
        {
            get
            {
                if (timeLogs == null)
                {
                    return TimeSpan.Zero;
                }

                return timeLogs.Select(x => x.Hours).Aggregate((x, y) => x.Add(y));
            }
        }

        public ICommand Delete { get; set; }

        public void Clear()
        {
            timeLogs = null;
            IsFilled = false;
            RaisePropertyChanged("Hours");
        }
    }
}