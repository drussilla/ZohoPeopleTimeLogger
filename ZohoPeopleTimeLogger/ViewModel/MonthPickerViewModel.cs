using System;
using System.Windows;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ZohoPeopleTimeLogger.Events;
using ZohoPeopleTimeLogger.Services;

namespace ZohoPeopleTimeLogger.ViewModel
{
    public class MonthPickerViewModel : ViewModel
    {
        public ICommand NextMonthCommand { get; private set; }

        public ICommand PreviousMonthCommand { get; private set; }

        public string MonthString
        {
            get { return new DateTime(Year, Month, 1).ToString("MMMM"); }
        }

        public int Month { get; private set; }
        
        public int Year { get; private set; }

        public EventHandler<MonthChangedEventArgs> MonthChanged = delegate { };

        public MonthPickerViewModel(IDateTimeService dateTimeService)
        {
            var currentDate = dateTimeService.Now;
            Month = currentDate.Month;
            Year = currentDate.Year;
            
            NextMonthCommand = new RelayCommand(GoToNextMonth);
            PreviousMonthCommand = new RelayCommand(GoToPrevMonth);
        }

        private void GoToPrevMonth()
        {
            if (Year == DateTime.MinValue.Year && Month == DateTime.MinValue.Month)
            {
                return;
            }

            if (Month == 1)
            {
                Month = 12;
                Year--;
            }
            else
            {
                Month--;
            }

            MonthChanged(this, new MonthChangedEventArgs(Year, Month));
        }

        private void GoToNextMonth()
        {
            if (Year == DateTime.MaxValue.Year && Month == DateTime.MaxValue.Month)
            {
                return;
            }

            if (Month == 12)
            {
                Year++;
                Month = 1;
            }
            else
            {
                Month++;
            }

            MonthChanged(this, new MonthChangedEventArgs(Year, Month));
        }
    }
}