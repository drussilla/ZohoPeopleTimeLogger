using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using PropertyChanged;
using ZohoPeopleClient;
using ZohoPeopleClient.TimeTrackerApi;

namespace ZohoPeopleTimeLogger.ViewModel
{
    public class DayViewModel : ViewModelBase, IDayViewModel
    {
        private readonly IZohoClient zohoClient;
        private List<TimeLog> timeLogs;

        public bool IsBusy { get; private set; }

        public static IDayViewModel DayFromOtherMonth(IZohoClient zohoClient)
        {
            return new DayViewModel(zohoClient) { IsActive = false };
        }

        public static IDayViewModel DayFromThisMonth(int day, DateTime month, IZohoClient zohoClient)
        {
            return new DayViewModel(zohoClient) { IsActive = true, Day = day, Date = new DateTime(month.Year, month.Month, day)};
        }

        private DayViewModel(IZohoClient zohoClient)
        {
            this.zohoClient = zohoClient;
            DeleteCommand = new RelayCommand(DeleteTimeLog);
        }

        public void FillLogs(List<TimeLog> logs)
        {
            timeLogs = logs;
            IsFilled = true;
            RaisePropertyChanged("Hours");
        }

        public async Task FillHoursAsync(string user, string jobId)
        {
            IsBusy = true;
            await zohoClient.TimeTracker.TimeLog.AddAsync(user, Date, jobId, TimeSpan.FromHours(8), "non-billable");
            var logsItemsForThisDay = await zohoClient.TimeTracker.TimeLog.GetAsync(user, Date, Date);
            IsBusy = false;
            FillLogs(logsItemsForThisDay);
        }

        public bool IsActive { get; set; }

        [AlsoNotifyFor("JobsDescription")]
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

        public ICommand DeleteCommand { get; set; }

        public DateTime Date { get; private set; }
        
        public string JobsDescription
        {
            get
            {
                if (timeLogs == null)
                {
                    return string.Empty;
                }

                return string.Join(", ", timeLogs.Select(x => x.JobName));
            }
        }

        public void Clear()
        {
            timeLogs = null;
            IsFilled = false;
            RaisePropertyChanged("Hours");
        }

        private async void DeleteTimeLog()
        {
            IsBusy = true;

            foreach (var timeLog in timeLogs)
            {
                await zohoClient.TimeTracker.TimeLog.DeleteAsync(timeLog.TimelogId);
            }

            Clear();

            IsBusy = false;
        }
    }
}