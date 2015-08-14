using System;
using System.Collections.Generic;
using System.Net;
using MahApps.Metro.Controls.Dialogs;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;
using ZohoPeopleClient;
using ZohoPeopleClient.Exceptions;
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
            dialog.Setup(x => x.ShowLogin(It.IsAny<string>())).ReturnsAsync(null);

            var result = target.LoginWithPassword();

            Assert.Null(result.Result);
        }

        [Theory, AutoMoqData]
        public void LoginWithPassword_ValidPasswordPassed_AuthDataReturned(
            [Frozen]Mock<IDialogService> dialog,
            [Frozen]Mock<IZohoClient> zoho, 
            [Frozen]Mock<IProgressDialogController> progressDialog,
            LoginController target)
        {
            var userName = "test";
            var password = "pass";
            var token = "token";
            var id = "42";

            dialog
                .Setup(x => x.ShowLogin(It.IsAny<string>()))
                .ReturnsAsync(new LoginDialogData { Username = userName, Password = password});
            dialog
                .Setup(x => x.ShowProgress(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(progressDialog.Object);
            zoho
                .Setup(x => x.LoginAsync(userName, password))
                .ReturnsAsync(token);

            dynamic employee1 = new Dictionary<string, string>();
            employee1["Email ID"] = userName;
            employee1["EmployeeID"] = id;

            dynamic employee2 = new Dictionary<string, string>();
            employee2["Email ID"] = "random";
            employee2["EmployeeID"] = "11";
            zoho
                .Setup(x => x.FetchRecord.GetAsync(It.IsAny<string>()))
                .ReturnsAsync(new List<dynamic> {employee1, employee2});

            var result = target.LoginWithPassword().Result;

            progressDialog.Verify(x => x.SetIndeterminate());
            zoho.Verify(x => x.LoginAsync(userName, password));
            progressDialog.Verify(x => x.CloseAsync());
            zoho.Verify(x => x.FetchRecord.GetAsync("P_EmployeeView"));

            Assert.NotNull(result);
            Assert.Equal(userName, result.UserName);
            Assert.Equal(token, result.Token);
            Assert.Equal(id, result.Id);
        }

        [Theory, AutoMoqData]
        public void LoginWithPassword_WrongPasswordAndSecondTimeTry_SameLoginRamains(
            [Frozen]Mock<IDialogService> dialog,
            [Frozen]Mock<IZohoClient> zoho,
            [Frozen]Mock<IProgressDialogController> progressDialog,
            LoginController target)
        {
            var userName = "test";
            var password = "pass";
            
            dialog
                .Setup(x => x.ShowLogin(null))
                .ReturnsAsync(new LoginDialogData { Username = userName, Password = password });
            dialog
                .Setup(x => x.ShowProgress(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(progressDialog.Object);
            zoho
                .Setup(x => x.LoginAsync(userName, password))
                .ThrowsAsync(new ApiLoginErrorException(""));

            target.LoginWithPassword().Wait();
            target.LoginWithPassword().Wait();

            dialog.Verify(x => x.ShowLogin(null), Times.Once);
            dialog.Verify(x => x.ShowLogin(userName), Times.Once);

            progressDialog.Verify(x => x.CloseAsync());
        }

        [Theory, AutoMoqData]
        public void LoginWithPassword_WrongPasswordAndSecondTimeTryPressedCancel_LoginIsEmpty(
            [Frozen]Mock<IDialogService> dialog,
            [Frozen]Mock<IZohoClient> zoho,
            [Frozen]Mock<IProgressDialogController> progressDialog,
            LoginController target)
        {
            var userName = "test";
            var password = "pass";

            dialog
                .Setup(x => x.ShowLogin(null))
                .ReturnsAsync(new LoginDialogData { Username = userName, Password = password });
            dialog
                .Setup(x => x.ShowProgress(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(progressDialog.Object);
            zoho
                .Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ThrowsAsync(new ApiLoginErrorException(""));

            target.LoginWithPassword().Wait();

            dialog.Verify(x => x.ShowLogin(null), Times.Once);

            zoho
                .Setup(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(null);
            target.LoginWithPassword().Wait();
            dialog.Verify(x => x.ShowLogin(userName), Times.Once);

            target.LoginWithPassword().Wait();
            dialog.Verify(x => x.ShowLogin(null), Times.Exactly(2));
            
            progressDialog.Verify(x => x.CloseAsync());
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
                .Setup(x => x.TimeTracker.TimeLog.GetAsync(authData.Id, date, date, "all", "all"))
                .ReturnsAsync(new List<TimeLog>());

            var result = await target.LoginWithToken(authData);
            
            Assert.True(result);
            zoho.Verify(x => x.Login(authData.Token), Times.Once);
            zoho.Verify(x => x.TimeTracker.TimeLog.GetAsync(authData.Id, date, date, "all", "all"));
            dialog.Verify(x => x.ShowProgress(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            progressDialog.Verify(x => x.CloseAsync(), Times.Once);
        }

        [Theory, AutoMoqData]
        public async void LoginWithToken_TokenIsNotValid_ReturnFalse(
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