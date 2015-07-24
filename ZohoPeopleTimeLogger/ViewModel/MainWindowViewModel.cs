using System;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ZohoPeopleClient;
using ZohoPeopleClient.Exceptions;
using ZohoPeopleTimeLogger.Services;

namespace ZohoPeopleTimeLogger.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IDialogService dialogService;

        private readonly IZohoClient zohoClient;

        private readonly IAuthenticationService authenticationService;

        public ICommand LoginCommand { get; private set; }

        public ICommand LogoutCommand { get; private set; }

        public ICommand FillTimeCommand { get; private set; }

        public bool IsLoggedIn { get; set; }

        public string UserName { get; set; }

        public MainWindowViewModel(IAuthenticationService authenticationService, IDialogService dialogService, IZohoClient zohoClient)
        {
            this.dialogService = dialogService;
            this.zohoClient = zohoClient;
            this.authenticationService = authenticationService;

            LoginCommand = new RelayCommand(Login, () => !IsLoggedIn);
        }

        private async void Login()
        {
            var loginDetails = await dialogService.ShowLogin();

            var progress = await dialogService.ShowProgress("Authentication", "Wait for response from server");
            progress.SetIndeterminate();

            var isError = false;
            var errorMessage = "";
            try
            {
                await zohoClient.LoginAsync(loginDetails.Username, loginDetails.Password);
            }
            catch (ApiLoginErrorException exception)
            {
                isError = true;
                errorMessage = exception.Response;
            }

            await progress.CloseAsync();

            if (isError)
            {
                await dialogService.ShowMessageAsync("Authentication error", errorMessage);
            }
        }
    }
}