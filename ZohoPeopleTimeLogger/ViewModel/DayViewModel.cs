using System.Windows.Input;
using GalaSoft.MvvmLight;

namespace ZohoPeopleTimeLogger.ViewModel
{
    public class DayViewModel : ViewModelBase
    {
        public bool IsActive { get; set; }

        public bool IsFilled { get; set; }

        public string Day { get; set; }

        public string Hours { get; set; }

        public ICommand Delete { get; set; }
    }
}