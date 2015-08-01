using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using ZohoPeopleTimeLogger.Controllers;

namespace ZohoPeopleTimeLogger.Services
{
    public class DialogService : IDialogService
    {
        private readonly MetroWindow currentWindow;

        public DialogService()
        {
            currentWindow = Application.Current.MainWindow as MetroWindow;
        }

        public Task<LoginDialogData> ShowLogin()
        {
            return currentWindow.ShowLoginAsync("Authentication", "Enter your credentials",
                            new LoginDialogSettings
                            {
                                AnimateHide = false,
                                AnimateShow = true,
                                NegativeButtonVisibility = Visibility.Visible
                            });
        }

        public async Task<IProgressDialogController> ShowProgress(string title, string message)
        {
            var controller = await currentWindow.ShowProgressAsync(title, message);
            return new MahAppsProgressDialogControllerAdapter(controller);
        }

        public Task<MessageDialogResult> ShowMessageAsync(string title, string errorMessage)
        {
            return currentWindow.ShowMessageAsync(title , errorMessage);
        }
    }
}