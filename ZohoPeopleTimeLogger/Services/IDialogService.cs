using System.Threading.Tasks;
using MahApps.Metro.Controls.Dialogs;
using ZohoPeopleTimeLogger.Controllers;
using ZohoPeopleTimeLogger.Model;

namespace ZohoPeopleTimeLogger.Services
{
    public interface IDialogService
    {
        Task<LoginData> ShowLogin(string defaultLogin);

        Task<IProgressDialogController> ShowProgress(string title, string message);
        Task<MessageDialogResult> ShowMessageAsync(string title, string errorMessage);
    }
}