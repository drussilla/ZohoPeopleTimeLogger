using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;
using ZohoPeopleClient;
using ZohoPeopleTimeLogger.Controllers;
using ZohoPeopleTimeLogger.Services;

namespace ZohoPeopleTimeLogger.UnitTests.Controllers
{
    public class LoginControllerTest
    {
        [Theory, AutoMoqData]
        public void LoginWithPassword_CancelWasPressed_ReturnNull(
            Mock<IDialogService> dialog,
            Mock<IZohoClient> zoho,
            LoginController target)
        {
            dialog.Setup(x => x.ShowLogin()).ReturnsAsync(null);

            var result = target.LoginWithPassword();

            Assert.Null(result.Result);
        }

        [Theory, AutoMoqData]
        public void LoginWithToken_CancelWasPressed_ReturnNull(
            [Frozen]Mock<IZohoClient> zoho,
            LoginController target)
        {
            var token = "Test";
            target.LoginWithToken(token);

            zoho.Verify(x => x.Login(token), Times.Once);
        }
    }
}