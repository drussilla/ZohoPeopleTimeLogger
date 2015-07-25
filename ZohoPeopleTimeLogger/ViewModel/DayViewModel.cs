using System.Windows.Input;
using GalaSoft.MvvmLight;

namespace ZohoPeopleTimeLogger.ViewModel
{
    public class DayViewModel : ViewModelBase
    {
        public static DayViewModel DayFromOtherMonth()
        {
            return new DayViewModel { IsActive = false };
        }

        public static DayViewModel DayFromThisMonth(int day)
        {
            return new DayViewModel { IsActive = true, Day = day };
        }

        public bool IsActive { get; set; }

        public bool IsFilled { get; set; }

        public int Day { get; set; }

        public string Hours { get; set; }

        public ICommand Delete { get; set; }
    }
}