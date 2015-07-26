using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.ServiceLocation;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;
using ZohoPeopleClient;
using ZohoPeopleTimeLogger.Controllers;
using ZohoPeopleTimeLogger.Events;
using ZohoPeopleTimeLogger.Model;
using ZohoPeopleTimeLogger.Services;
using ZohoPeopleTimeLogger.ViewModel;

namespace ZohoPeopleTimeLogger.UnitTests
{
    public class MainWindowViewModelTests
    {
        [Theory, AutoMoqData]
        public void ViewReady_NoLoginInformationStored_AskForLoginAndSaveValidDataAndLoadDays(
            [Frozen]Mock<IAuthenticationStorage> auth,
            [Frozen]Mock<IDialogService> dialog,
            [Frozen]Mock<IZohoClient> zoho,
            [Frozen]Mock<ILoginController> login,
            [Frozen]Mock<IDaysService> daysService,
            [Frozen]Mock<IMonthPickerViewModel> monthPicker,
            AuthenticationData data,
            MainWindowViewModel target)
        {
            var date = DateTime.Now;
            var days = Enumerable.Range(0, 25).Select(x => new DayViewModel()).ToList();
            auth.Setup(x => x.GetAuthenticationData()).Returns((AuthenticationData)null);
            login.Setup(x => x.Login()).Returns(Task.Run(() => data));
            monthPicker.Setup(x => x.CurrentDate).Returns(date);
            daysService.Setup(x => x.GetDays(date))
                .Returns(() => days);

            target.ViewReady();

            Assert.True(target.IsLoggedIn);
            Assert.Equal(data.UserName, target.UserName);
            Assert.Equal(days, target.Days);

            Assert.True(target.LogoutCommand.CanExecute(null));
            Assert.False(target.LoginCommand.CanExecute(null));
            
            login.Verify(x => x.Login(), Times.Once);
            auth.Verify(x => x.SaveAuthenticationData(data));
            daysService.Verify(x => x.GetDays(date), Times.Once);
        }

        [Theory, AutoMoqData]
        public void ViewReady_LoginInformationStored_UserIsLoggedIn(
            [Frozen]Mock<IAuthenticationStorage> auth,
            [Frozen]Mock<IDialogService> dialog,
            [Frozen]Mock<IZohoClient> zoho,
            [Frozen]Mock<ILoginController> login,
            [Frozen]Mock<IDaysService> daysService,
            [Frozen]Mock<IMonthPickerViewModel> monthPicker,
            [Frozen]AuthenticationData data,
            MainWindowViewModel target)
        {
            var date = DateTime.Now;
            var days = Enumerable.Range(0, 25).Select(x => new DayViewModel()).ToList();
            auth.Setup(x => x.GetAuthenticationData()).Returns(data);
            monthPicker.Setup(x => x.CurrentDate).Returns(date);
            daysService.Setup(x => x.GetDays(date))
                .Returns(() => days);
            
            target.ViewReady();

            Assert.True(target.IsLoggedIn);
            Assert.Equal(data.UserName, target.UserName);
            Assert.True(target.LogoutCommand.CanExecute(null));
            Assert.False(target.LoginCommand.CanExecute(null));
            Assert.Equal(days, target.Days);

            login.Verify(x => x.Login(), Times.Never);
            daysService.Verify(x => x.GetDays(date), Times.Once);
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

        [Theory, AutoMoqData]
        public void MonthChanged_NewMonthDataLoaded(
            [Frozen]Mock<IDaysService> daysService,
            [Frozen]Mock<IMonthPickerViewModel> monthPicker,
            MainWindowViewModel target)
        {
            var newMonth = new DateTime(2013, 01, 01);
            var days = Enumerable.Range(0, 25).Select(x => new DayViewModel()).ToList();
            daysService.Setup(x => x.GetDays(newMonth)).Returns(days);

            monthPicker.Raise(x => x.MonthChanged += null, new MonthChangedEventArgs(newMonth));

            daysService.Verify(x => x.GetDays(newMonth), Times.Once);
            Assert.Equal(days, target.Days);
        }
    }
}
