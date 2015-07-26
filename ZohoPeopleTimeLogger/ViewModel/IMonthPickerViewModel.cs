using System;
using System.Windows.Input;
using ZohoPeopleTimeLogger.Events;

namespace ZohoPeopleTimeLogger.ViewModel
{
    public interface IMonthPickerViewModel
    {
        DateTime CurrentDate { get; }
        ICommand NextMonthCommand { get; }
        ICommand PreviousMonthCommand { get; }
        event EventHandler<MonthChangedEventArgs> MonthChanged;
    }
}