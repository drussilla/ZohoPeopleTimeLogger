using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using GalaSoft.MvvmLight.CommandWpf;
using ZohoPeopleTimeLogger.Controllers;
using ZohoPeopleTimeLogger.Events;
using ZohoPeopleTimeLogger.Exceptions;
using ZohoPeopleTimeLogger.Model;
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

        public ICommand FillSingleDayCommand { get; private set; }

        public bool IsLoggedIn { get; private set; }

        public string UserName { get; private set; }

        public IMonthPickerViewModel MonthPickerViewModel { get; private set; }

        public List<IDayViewModel> Days { get; set; } 

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

            LoginCommand = new RelayCommand(async () => await Login(), () => !IsLoggedIn);
            LogoutCommand = new RelayCommand(Logout, () => IsLoggedIn);
            FillTimeCommand = new RelayCommand(FillTime);
            FillSingleDayCommand = new RelayCommand<IDayViewModel>(FillSingleDay);

            Days = daysService.GetDays(MonthPickerViewModel.CurrentDate);
        }

        public override async Task ViewReady()
        {
            var authData = authenticationStorage.GetAuthenticationData();
            if (authData == null)
            {
                await Login();
            }
            else
            {
                IsLoggedIn = true;
                UserName = authData.UserName;
                bool isTokenValid = await loginController.LoginWithToken(authData);

                if (isTokenValid)
                {
                    if (string.IsNullOrEmpty(authData.Id))
                    {
                        await GetAndSaveUserId(authData);
                    }
                    LoadDays(MonthPickerViewModel.CurrentDate);
                }
                else
                {
                    Logout();
                    await Login();
                }
            }
        }

        public override void Cleanup()
        {
            MonthPickerViewModel.MonthChanged -= MonthPickerViewModelOnMonthChanged;
            base.Cleanup();
        }

        private async void LoadDays(DateTime month)
        {
            Days = daysService.GetDays(month);

            var progress = await dialogService.ShowProgress("Loading data from server", "Please wait");
            if (progress != null)
            {
                progress.SetIndeterminate();
            }

            await daysService.FillDaysWithTimeLogsAsync(Days, month);

            if (progress != null)
            {
                await progress.CloseAsync();
            }
        }

        private async Task Login()
        {
            var authenticationData = await loginController.LoginWithPassword();

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

            foreach (var dayViewModel in Days)
            {
                dayViewModel.Clear();
            }
        }

        private void MonthPickerViewModelOnMonthChanged(object sender, MonthChangedEventArgs monthChangedEventArgs)
        {
            LoadDays(monthChangedEventArgs.NewMonth);
        }

        private async void FillTime()
        {
            if (AnyNotFilledDays())
            {
                try
                {
                    await daysService.FillMissingTimeLogsAsync(Days);
                }
                catch (JobNotFoundException ex)
                {
                    await dialogService.ShowMessageAsync("Oops...", 
                        "I cannot make you happy, master :( \n\n" +
                        "There is no job in ZOHO for this month\n" +
                        "Just drink some coffee and tell your manager about this issue...\n" +
                        "Stay cool, my friend! ;)\n\n" +
                        "Additional info: " + ex.Message);
                }
            }
            else
            {
                await dialogService.ShowMessageAsync("Relax! You already happy!",
                    "You have no empty days in this month. So don't worry, boss will be happy!");
            }
        }

        private async void FillSingleDay(IDayViewModel dayViewModel)
        {
            try
            {
                await daysService.FillMissingTimeLogsAsync(dayViewModel);
            }
            catch (JobNotFoundException ex)
            {
                await dialogService.ShowMessageAsync("Oops...", 
                    "Sorry, cannot log working time for this day. Job is not found in ZOHO :( Just relax and drink coffee....\n" +
                    "....and check this problem with your manager.\n" +
                    "Stay cool! ;)\n\n " +
                    "Additional info: " + ex.Message);
            }
            
        }

        private bool AnyNotFilledDays()
        {
            return Days.Any(x => x.IsActive && !x.IsFilled & !x.IsHoliday);
        }

        private async Task GetAndSaveUserId(AuthenticationData authData)
        {
            var progress = await dialogService.ShowProgress("Loading data from server", "Please wait");
            progress.SetIndeterminate();
            var id = await loginController.GetEmployeeId(authData.UserName);
            authData.Id = id;
            authenticationStorage.SaveAuthenticationData(authData);
            await progress.CloseAsync();
        }
    }
}