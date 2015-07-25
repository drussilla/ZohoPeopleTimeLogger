using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;
using ZohoPeopleClient;
using ZohoPeopleTimeLogger.Controllers;
using ZohoPeopleTimeLogger.Model;
using ZohoPeopleTimeLogger.Services;
using ZohoPeopleTimeLogger.ViewModel;

namespace ZohoPeopleTimeLogger.UnitTests
{
    public class MainWindowViewModelTests
    {
        [Theory, AutoMoqData]
        public void ViewReady_NoLoginInformationStored_AskForLoginAndSaveValidData(
            [Frozen]Mock<IAuthenticationStorage> auth,
            [Frozen]Mock<IDialogService> dialog,
            [Frozen]Mock<IZohoClient> zoho,
            [Frozen]Mock<ILoginController> login,
            AuthenticationData data,
            MainWindowViewModel target)
        {
            auth.Setup(x => x.GetAuthenticationData()).Returns((AuthenticationData)null);
            login.Setup(x => x.Login()).Returns(Task.Run(() => data));

            target.ViewReady();

            Assert.True(target.IsLoggedIn);
            Assert.Equal(data.UserName, target.UserName);
            Assert.True(target.LogoutCommand.CanExecute(null));
            Assert.False(target.LoginCommand.CanExecute(null));

            login.Verify(x => x.Login(), Times.Once);
            auth.Verify(x => x.SaveAuthenticationData(data));
        }

        [Theory, AutoMoqData]
        public void ViewReady_LoginInformationStored_UserIsLoggedIn(
            [Frozen]Mock<IAuthenticationStorage> auth,
            [Frozen]Mock<IDialogService> dialog,
            [Frozen]Mock<IZohoClient> zoho,
            [Frozen]Mock<ILoginController> login,
            [Frozen]AuthenticationData data,
            MainWindowViewModel target)
        {
            auth.Setup(x => x.GetAuthenticationData()).Returns(data);
            
            target.ViewReady();

            Assert.True(target.IsLoggedIn);
            Assert.Equal(data.UserName, target.UserName);
            Assert.True(target.LogoutCommand.CanExecute(null));
            Assert.False(target.LoginCommand.CanExecute(null));

            login.Verify(x => x.Login(), Times.Never);
        }

        [Theory, AutoMoqData]
        public void LogoutCommand_LoginInformationStored_UserIsLoggedOut(
            [Frozen]Mock<IAuthenticationStorage> auth,
            [Frozen]Mock<IDialogService> dialog,
            [Frozen]Mock<IZohoClient> zoho,
            [Frozen]Mock<ILoginController> login,
            [Frozen]AuthenticationData data,
            MainWindowViewModel target)
        {
            auth.Setup(x => x.GetAuthenticationData()).Returns(data);
            target.ViewReady();
            
            target.LogoutCommand.Execute(null);

            Assert.False(target.IsLoggedIn);
            Assert.Null(target.UserName);
            Assert.False(target.LogoutCommand.CanExecute(null));
            Assert.True(target.LoginCommand.CanExecute(null));

            auth.Verify(x => x.Clear(), Times.Once);
        }
    }
}
