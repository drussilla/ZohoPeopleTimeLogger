using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using ZohoPeopleTimeLogger.Controllers;

namespace ZohoPeopleTimeLogger.Services
{
    public interface IDialogService
    {
        Task<LoginDialogData> ShowLogin();

        Task<IProgressDialogController> ShowProgress(string title, string message);
        Task<MessageDialogResult> ShowMessageAsync(string title, string errorMessage);
    }
}