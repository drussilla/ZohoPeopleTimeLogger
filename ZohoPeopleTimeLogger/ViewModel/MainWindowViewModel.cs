using System;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ZohoPeopleClient;
using ZohoPeopleClient.Exceptions;
using ZohoPeopleTimeLogger.Controllers;
using ZohoPeopleTimeLogger.Services;

namespace ZohoPeopleTimeLogger.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IDialogService dialogService;

        private readonly IZohoClient zohoClient;

        private readonly IAuthenticationStorage authenticationStorage;

        private readonly ILoginController loginController;

        public ICommand LoginCommand { get; private set; }

        public ICommand LogoutCommand { get; private set; }

        public ICommand FillTimeCommand { get; private set; }

        public bool IsLoggedIn { get; set; }

        public string UserName { get; set; }

        public MainWindowViewModel(
            IAuthenticationStorage authenticationStorage,
            IDialogService dialogService,
            IZohoClient zohoClient, 
            ILoginController loginController)
        {
            this.dialogService = dialogService;
            this.zohoClient = zohoClient;
            this.loginController = loginController;
            this.authenticationStorage = authenticationStorage;

            if (authenticationStorage.GetAuthenticationData() == null)
            {
                Login();
            }

            LoginCommand = new RelayCommand(Login, () => !IsLoggedIn);
        }

        private async void Login()
        {
            var authenticationData = await loginController.Login();

            if (authenticationData != null)
            {
                authenticationStorage.SaveAuthenticationData(authenticationData);
            }
        }
    }
}