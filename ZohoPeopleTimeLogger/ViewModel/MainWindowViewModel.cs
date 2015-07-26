using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using Microsoft.Practices.Unity;
using ZohoPeopleClient;
using ZohoPeopleTimeLogger.Controllers;
using ZohoPeopleTimeLogger.Events;
using ZohoPeopleTimeLogger.Services;

namespace ZohoPeopleTimeLogger.ViewModel
{
    public class MainWindowViewModel : ViewModel
    {
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
            IMonthPickerViewModel monthPickerViewModel)
        {
            this.dialogService = dialogService;
            this.daysService = daysService;
            this.loginController = loginController;
            this.authenticationStorage = authenticationStorage;

            MonthPickerViewModel = monthPickerViewModel;
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

                LoadDays(MonthPickerViewModel.CurrentDate);
            }
        }

        private void LoadDays(DateTime month)
        {
            Days = daysService.GetDays(month);
        }

        private async void Login()
        {
            var authenticationData = await loginController.Login();

            if (authenticationData != null)
            {
                authenticationStorage.SaveAuthenticationData(authenticationData);
                IsLoggedIn = true;
                UserName = authenticationData.UserName;

                LoadDays(MonthPickerViewModel.CurrentDate);
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
            LoadDays(monthChangedEventArgs.NewMonth);
        }

        public override void Cleanup()
        {
            MonthPickerViewModel.MonthChanged -= MonthPickerViewModelOnMonthChanged;
            base.Cleanup();
        }
    }
}