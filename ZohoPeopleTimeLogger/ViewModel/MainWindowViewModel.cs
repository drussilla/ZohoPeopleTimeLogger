using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Practices.Unity;
using ZohoPeopleClient;
using ZohoPeopleTimeLogger.Controllers;
using ZohoPeopleTimeLogger.Events;
using ZohoPeopleTimeLogger.Extensions;
using ZohoPeopleTimeLogger.Model;
using ZohoPeopleTimeLogger.Services;

namespace ZohoPeopleTimeLogger.ViewModel
{
    public class MainWindowViewModel : ViewModel
    {
        private readonly IZohoClient zohoClient;

        private readonly IDialogService dialogService;

        private readonly IDaysService daysService;

        private readonly IAuthenticationStorage authenticationStorage;

        private readonly ILoginController loginController;

        public ICommand LoginCommand { get; private set; }

        public ICommand LogoutCommand { get; private set; }

        public ICommand FillTimeCommand { get; private set; }

        public bool IsLoggedIn { get; private set; }

        public string UserName { get; private set; }

        public IMonthPickerViewModel MonthPickerViewModel { get; private set; }

        public List<DayViewModel> Days { get; set; } 

        public MainWindowViewModel(
            IAuthenticationStorage authenticationStorage,
            IDialogService dialogService,
            IDaysService daysService, 
            ILoginController loginController,
            IMonthPickerViewModel monthPickerViewModel, 
            IZohoClient zohoClient)
        {
            this.dialogService = dialogService;
            this.daysService = daysService;
            this.loginController = loginController;
            this.authenticationStorage = authenticationStorage;

            MonthPickerViewModel = monthPickerViewModel;
            this.zohoClient = zohoClient;
            MonthPickerViewModel.MonthChanged += MonthPickerViewModelOnMonthChanged;

            LoginCommand = new RelayCommand(Login, () => !IsLoggedIn);
            LogoutCommand = new RelayCommand(Logout, () => IsLoggedIn);
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

                LoadDays(MonthPickerViewModel.CurrentDate, authData);
            }
        }

        private async void LoadDays(DateTime month, AuthenticationData auth)
        {
            Days = daysService.GetDays(month);

            var timeLogs = await zohoClient.TimeTracker.TimeLog.GetAsync(
                    auth.UserName,
                    month.BeginOfMonth(), 
                    month.EndOfMonth());

            daysService.FillDays(Days, timeLogs);
        }

        private async void Login()
        {
            var authenticationData = await loginController.Login();

            if (authenticationData != null)
            {
                authenticationStorage.SaveAuthenticationData(authenticationData);
                IsLoggedIn = true;
                UserName = authenticationData.UserName;

                LoadDays(MonthPickerViewModel.CurrentDate, authenticationData);
            }
            else
            {
                IsLoggedIn = false;
                UserName = null;
            }
        }

        private void Logout()
        {
            authenticationStorage.Clear();
            IsLoggedIn = false;
            UserName = null;
        }

        private void MonthPickerViewModelOnMonthChanged(object sender, MonthChangedEventArgs monthChangedEventArgs)
        {
            LoadDays(monthChangedEventArgs.NewMonth, authenticationStorage.GetAuthenticationData());
        }

        public override void Cleanup()
        {
            MonthPickerViewModel.MonthChanged -= MonthPickerViewModelOnMonthChanged;
            base.Cleanup();
        }
    }
}