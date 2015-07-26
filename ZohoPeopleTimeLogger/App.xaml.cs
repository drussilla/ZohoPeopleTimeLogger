using System.Windows;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using ZohoPeopleClient;
using ZohoPeopleTimeLogger.Controllers;
using ZohoPeopleTimeLogger.Services;
using ZohoPeopleTimeLogger.ViewModel;

namespace ZohoPeopleTimeLogger
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            UnityContainer container = new UnityContainer();

            container.RegisterType<IAuthenticationStorage, AuthenticationStorage>();
            container.RegisterType<IZohoClient, ZohoClient>(new ContainerControlledLifetimeManager());
            container.RegisterType<IDialogService, DialogService>();
            container.RegisterType<ILoginController, LoginController>();
            container.RegisterType<IDateTimeService, DateTimeService>();
            container.RegisterType<IDaysService, DaysService>();
            container.RegisterType<IMonthPickerViewModel, MonthPickerViewModel>();

            ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(container));
        }
    }
}
