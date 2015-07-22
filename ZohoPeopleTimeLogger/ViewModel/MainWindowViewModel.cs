using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using ZohoPeopleTimeLogger.Services;

namespace ZohoPeopleTimeLogger.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        public readonly IDialogService dialogService;

        public ICommand LoginCommand { get; set; }

        public ICommand  LogoutCommand { get; set; }

        public bool IsLoggedIn { get; set; }

        public string UserName { get; set; }

        public MainWindowViewModel()
        {
            LoginCommand = new RelayCommand(Login, () => !IsLoggedIn);
            dialogService = new DialogService();
        }

        private async void Login()
        {
            await dialogService.ShowLogin();
        }
    }
}