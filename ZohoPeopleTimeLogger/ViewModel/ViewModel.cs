using System.ComponentModel;
using System.Threading.Tasks;
using GalaSoft.MvvmLight;

namespace ZohoPeopleTimeLogger.ViewModel
{
    public abstract class ViewModel : ViewModelBase
    {
        public virtual async Task ViewReady()
        {
        }
    }
}