using System;
using System.Windows;
using ZohoPeopleClient;
using MahApps.Metro.Controls.Dialogs;
using ZohoPeopleClient.Exceptions;

namespace ZohoPeopleTimeLogger.View
{
    /// <summary>
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView
    {
        public MainWindowView()
        {
            InitializeComponent();
        }

        private async void ShowLoginDialog()
        {
            LoginDialogData result;
            do
            {
                result =
                    await
                        this.ShowLoginAsync("Authentication", "Enter your credentials",
                            new LoginDialogSettings
                            {
                                AnimateHide = false,
                                AnimateShow = true
                            });
            } while (result == null);

            ProgressDialogController progress = await this.ShowProgressAsync("Authentication", "Wait for response from server", false);
            progress.SetIndeterminate();

            bool isLoginError = false;
            string errorText = "";
            var zohoClient = new ZohoClient();
            try
            {
                await zohoClient.LoginAsync(result.Username, result.Password);
            }
            catch (ApiLoginErrorException exception)
            {
                errorText = exception.Response;
                isLoginError = true;
            }
            
            await progress.CloseAsync();

            if (isLoginError)
            {
                await this.ShowMessageAsync("Error durong login", errorText);
            }
            else
            {
                await this.ShowMessageAsync("Authentication Information",
                            String.Format("Username: {0}\nPassword: {1}", result.Username, result.Password));
            }
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            ShowLoginDialog();
        }
    }
}
