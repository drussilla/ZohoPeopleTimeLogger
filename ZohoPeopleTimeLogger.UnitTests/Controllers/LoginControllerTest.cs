using System;
using System.Collections.Generic;
using System.Net;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;
using ZohoPeopleClient;
using ZohoPeopleClient.Model.TimeTrackerApi;
using ZohoPeopleTimeLogger.Controllers;
using ZohoPeopleTimeLogger.Model;
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
        public async void LoginWithToken_TokenIsValid_ReturnTrue(
            [Frozen]Mock<IZohoClient> zoho,
            [Frozen]Mock<IDateTimeService> dateTime,
            [Frozen]Mock<IDialogService> dialog,
            [Frozen]Mock<IProgressDialogController> progressDialog,
            AuthenticationData authData,
            DateTime date,
            LoginController target)
        {
            dateTime
                .Setup(x => x.Now)
                .Returns(date);

            dialog
                .Setup(x => x.ShowProgress(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(progressDialog.Object);

            zoho
                .Setup(x => x.TimeTracker.TimeLog.GetAsync(authData.UserName, date, date, "all", "all"))
                .ReturnsAsync(new List<TimeLog>());

            var result = await target.LoginWithToken(authData);
            
            Assert.True(result);
            zoho.Verify(x => x.Login(authData.Token), Times.Once);
            zoho.Verify(x => x.TimeTracker.TimeLog.GetAsync(authData.UserName, date, date, "all", "all"));
            dialog.Verify(x => x.ShowProgress(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            progressDialog.Verify(x => x.CloseAsync(), Times.Once);
        }

        [Theory, AutoMoqData]
        public async void LoginWithToken_TokenIsNotValid_ReturnTrue(
            [Frozen]Mock<IZohoClient> zoho,
            [Frozen]Mock<IDateTimeService> dateTime,
            [Frozen]Mock<IDialogService> dialog,
            [Frozen]Mock<IProgressDialogController> progressDialog,
            AuthenticationData authData,
            DateTime date,
            LoginController target)
        {
            var webResponse = new Mock<HttpWebResponse>();
            webResponse.Setup(x => x.StatusCode).Returns(HttpStatusCode.BadRequest);
            dialog
                .Setup(x => x.ShowProgress(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(progressDialog.Object);

            zoho
                .Setup(x => x.TimeTracker.TimeLog.GetAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), "all", "all"))
                .Throws(new WebException("", null, WebExceptionStatus.ProtocolError, webResponse.Object));

            var result = await target.LoginWithToken(authData);

            Assert.False(result);
            dialog.Verify(x => x.ShowProgress(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            progressDialog.Verify(x => x.CloseAsync(), Times.Once);
        }

        [Theory, AutoMoqData]
        public async void LoginWithToken_WebException_Throw(
            [Frozen]Mock<IZohoClient> zoho,
            [Frozen]Mock<IDateTimeService> dateTime,
            [Frozen]Mock<IDialogService> dialog,
            [Frozen]Mock<IProgressDialogController> progressDialog,
            AuthenticationData authData,
            DateTime date,
            LoginController target)
        {
            dialog
                .Setup(x => x.ShowProgress(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(progressDialog.Object);

            zoho
                .Setup(x => x.TimeTracker.TimeLog.GetAsync(It.IsAny<string>(), It.IsAny<DateTime>(), It.IsAny<DateTime>(), "all", "all"))
                .Throws(new WebException());

            await Assert.ThrowsAsync<WebException>(async () => await target.LoginWithToken(authData));
        }
    }
}