using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ZohoPeopleClient;
using ZohoPeopleClient.Exceptions;
using ZohoPeopleTimeLogger.Controllers;
using ZohoPeopleTimeLogger.Services;

namespace ZohoPeopleTimeLogger.ViewModel
{
    public class MainWindowViewModel : ViewModel
    {
        private readonly IDialogService dialogService;

        private readonly IZohoClient zohoClient;

        private readonly IAuthenticationStorage authenticationStorage;

        private readonly ILoginController loginController;

        public ICommand LoginCommand { get; private set; }

        public ICommand LogoutCommand { get; private set; }

        public ICommand FillTimeCommand { get; private set; }

        public bool IsLoggedIn { get; private set; }

        public string UserName { get; private set; }

        public ObservableCollection<DayViewModel> Days { get; set; } 

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

            LoginCommand = new RelayCommand(Login, () => !IsLoggedIn);
        }

        public override void ViewReady()
        {
            var authData = authenticationStorage.GetAuthenticationData();
            if (authData == null)
            {
                Login();
            }
            else
            {
                IsLoggedIn = true;
                UserName = authData.UserName;

                LoadDays();
            }
        }

        private void LoadDays()
        {
            Days = new ObservableCollection<DayViewModel>();
            Days.Add(new DayViewModel());
        }

        private async void Login()
        {
            var authenticationData = await loginController.Login();

            if (authenticationData != null)
            {
                authenticationStorage.SaveAuthenticationData(authenticationData);
                IsLoggedIn = true;
                UserName = authenticationData.UserName;
            }
            else
            {
                IsLoggedIn = false;
                UserName = "";
            }
        }
    }
}