using System.Threading.Tasks;
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

            login.Verify(x => x.Login(), Times.Once);
            auth.Verify(x => x.SaveAuthenticationData(data));
        }
    }
}
