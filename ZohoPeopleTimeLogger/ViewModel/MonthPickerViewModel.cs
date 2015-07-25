using System;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using PropertyChanged;
using ZohoPeopleTimeLogger.Events;
using ZohoPeopleTimeLogger.Services;

namespace ZohoPeopleTimeLogger.ViewModel
{
    public class MonthPickerViewModel : ViewModel
    {
        [AlsoNotifyFor("MonthString", "Month", "Year")]
        private DateTime CurrentDate { get; set; }

        public ICommand NextMonthCommand { get; private set; }

        public ICommand PreviousMonthCommand { get; private set; }

        public string MonthString
        {
            get { return CurrentDate.ToString("MMMM"); }
        }

        public int Month
        {
            get { return CurrentDate.Month; }
        }

        public int Year
        {
            get { return CurrentDate.Year; }
        }

        public EventHandler<MonthChangedEventArgs> MonthChanged = delegate { };

        public MonthPickerViewModel(IDateTimeService dateTimeService)
        {
            CurrentDate = dateTimeService.Now;
            
            NextMonthCommand = new RelayCommand(GoToNextMonth);
            PreviousMonthCommand = new RelayCommand(GoToPrevMonth);
        }

        private void GoToPrevMonth()
        {
            if (Year == DateTime.MinValue.Year && Month == DateTime.MinValue.Month)
            {
                return;
            }

            CurrentDate = CurrentDate.AddMonths(-1);
            MonthChanged(this, new MonthChangedEventArgs(Year, Month));
        }

        private void GoToNextMonth()
        {
            if (Year == DateTime.MaxValue.Year && Month == DateTime.MaxValue.Month)
            {
                return;
            }

            CurrentDate = CurrentDate.AddMonths(1);
            MonthChanged(this, new MonthChangedEventArgs(Year, Month));
        }
    }
}