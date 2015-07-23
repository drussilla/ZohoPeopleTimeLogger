using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;

namespace ZohoPeopleTimeLogger.Services
{
    public interface IDialogService
    {
        Task<LoginDialogData> ShowLogin();

        Task<ProgressDialogController> ShowProgress(string title, string message);
        Task<MessageDialogResult> ShowMessageAsync(string title, string errorMessage);
    }
}