using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;

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

        public Task<ProgressDialogController> ShowProgress(string title, string message)
        {
            return currentWindow.ShowProgressAsync(title, message);
        }

        public Task<MessageDialogResult> ShowMessageAsync(string title, string errorMessage)
        {
            return currentWindow.ShowMessageAsync(title , errorMessage);
        }
    }
}