using System;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ZohoPeopleTimeLogger.Events;
using ZohoPeopleTimeLogger.Services;

namespace ZohoPeopleTimeLogger.ViewModel
{
    public class MonthPickerViewModel : ViewModel, IMonthPickerViewModel
    {
        public DateTime CurrentDate { get; private set; }

        public ICommand NextMonthCommand { get; private set; }

        public ICommand PreviousMonthCommand { get; private set; }

        public event EventHandler<MonthChangedEventArgs> MonthChanged = delegate { };

        public MonthPickerViewModel(IDateTimeService dateTimeService)
        {
            CurrentDate = dateTimeService.Now;
            
            NextMonthCommand = new RelayCommand(GoToNextMonth);
            PreviousMonthCommand = new RelayCommand(GoToPrevMonth);
        }

        private void GoToPrevMonth()
        {
            if (CurrentDate.Year == DateTime.MinValue.Year && CurrentDate.Month == DateTime.MinValue.Month)
            {
                return;
            }

            CurrentDate = CurrentDate.AddMonths(-1);
            MonthChanged(this, new MonthChangedEventArgs(CurrentDate));
        }

        private void GoToNextMonth()
        {
            if (CurrentDate.Year == DateTime.MaxValue.Year && CurrentDate.Month == DateTime.MaxValue.Month)
            {
                return;
            }

            CurrentDate = CurrentDate.AddMonths(1);
            MonthChanged(this, new MonthChangedEventArgs(CurrentDate));
        }
    }
}