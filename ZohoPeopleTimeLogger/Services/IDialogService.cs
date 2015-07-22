using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;

namespace ZohoPeopleTimeLogger.Services
{
    public interface IDialogService
    {
        Task<LoginDialogData> ShowLogin();
    }
}