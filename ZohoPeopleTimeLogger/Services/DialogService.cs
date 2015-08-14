using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using ZohoPeopleTimeLogger.Controllers;
using ZohoPeopleTimeLogger.Model;

namespace ZohoPeopleTimeLogger.Services
{
    public class DialogService : IDialogService
    {
        private readonly MetroWindow currentWindow;

        public DialogService()
        {
            currentWindow = Application.Current.MainWindow as MetroWindow;
        }

        public async Task<LoginData> ShowLogin(string defaultLogin)
        {
            var data = await currentWindow.ShowLoginAsync("Authentication", "Enter your credentials",
                            new LoginDialogSettings
                            {
                                AnimateHide = false,
                                AnimateShow = true,
                                InitialUsername = defaultLogin,
                                UsernameWatermark = "Email...",
                                NegativeButtonVisibility = Visibility.Visible
                            });
            if (data == null)
            {
                return null;
            }

            return new LoginData {Username = data.Username, Password = data.Password};
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