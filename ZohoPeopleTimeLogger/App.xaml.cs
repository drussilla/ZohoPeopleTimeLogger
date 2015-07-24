using System.Windows;
using Microsoft.Practices.ServiceLocation;
using Microsoft.Practices.Unity;
using ZohoPeopleClient;
using ZohoPeopleTimeLogger.Controllers;
using ZohoPeopleTimeLogger.Services;

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
            container.RegisterType<IZohoClient, ZohoClient>();
            container.RegisterType<IDialogService, DialogService>();
            container.RegisterType<ILoginController, LoginController>();

            ServiceLocator.SetLocatorProvider(() => new UnityServiceLocator(container));
        }
    }
}
