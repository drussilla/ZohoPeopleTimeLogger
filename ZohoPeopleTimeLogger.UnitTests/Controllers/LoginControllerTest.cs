using Moq;
using Xunit;
using ZohoPeopleClient;
using ZohoPeopleTimeLogger.Controllers;
using ZohoPeopleTimeLogger.Services;

namespace ZohoPeopleTimeLogger.UnitTests.Controllers
{
    public class LoginControllerTest
    {
        [Theory, AutoMoqData]
        public void Login_CancelWasPressed_ReturnNull(
            Mock<IDialogService> dialog,
            Mock<IZohoClient> zoho,
            LoginController target)
        {
            dialog.Setup(x => x.ShowLogin()).ReturnsAsync(null);

            var result = target.Login();

            Assert.Null(result.Result);
        } 
    }
}