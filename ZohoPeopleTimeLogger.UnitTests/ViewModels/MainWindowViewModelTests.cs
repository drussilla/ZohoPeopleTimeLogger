using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;
using ZohoPeopleClient;
using ZohoPeopleClient.TimeTrackerApi;
using ZohoPeopleTimeLogger.Controllers;
using ZohoPeopleTimeLogger.Events;
using ZohoPeopleTimeLogger.Extensions;
using ZohoPeopleTimeLogger.Model;
using ZohoPeopleTimeLogger.Services;
using ZohoPeopleTimeLogger.ViewModel;

namespace ZohoPeopleTimeLogger.UnitTests
{
    public class MainWindowViewModelTests
    {
        [Theory, AutoMoqData]
        public void Ctor_DaysLoaded(
            [Frozen]Mock<IAuthenticationStorage> auth,
            [Frozen]Mock<IDialogService> dialog,
            [Frozen]Mock<IZohoClient> zoho,
            [Frozen]Mock<ILoginController> login,
            [Frozen]Mock<IDaysService> daysService,
            [Frozen]Mock<IMonthPickerViewModel> monthPicker)
        {
            var date = new DateTime(2015, 04, 22);
            daysService.Setup(x => x.GetDays(date))
                .Returns(() => Enumerable.Range(0, 25).Select(x => new DayViewModel()).ToList());
            monthPicker.Setup(x => x.CurrentDate).Returns(date);

            var target = new MainWindowViewModel(auth.Object, dialog.Object, daysService.Object, login.Object,
                monthPicker.Object, zoho.Object);

            Assert.Equal(DaysService.MaximumWorkingDaysInMonth, target.Days.Count);
            daysService.Verify(x => x.GetDays(date), Times.Once);
        }

        [Theory, AutoMoqData]
        public void ViewReady_NoLoginInformationStored_AskForLoginAndSaveValidDataAndLoadDays(
            [Frozen]Mock<IAuthenticationStorage> auth,
            [Frozen]Mock<IDialogService> dialog,
            [Frozen]Mock<IZohoClient> zoho,
            [Frozen]Mock<ILoginController> login,
            [Frozen]Mock<IDaysService> daysService,
            [Frozen]Mock<IMonthPickerViewModel> monthPicker,
            [Frozen]AuthenticationData data,
            MainWindowViewModel target)
        {
            var startOfTheMonth = new DateTime(2015, 07, 01);
            var endOfTheMonth = startOfTheMonth.EndOfMonth();
            var middleOfTheMonth = new DateTime(2015, 07, 15);
            var days = Enumerable.Range(0, 25).Select(x => new DayViewModel()).ToList();
            var firstLog = new TimeLog { WorkDate = startOfTheMonth, Hours = TimeSpan.FromHours(8) };
            var secondLog = new TimeLog { WorkDate = endOfTheMonth, Hours = TimeSpan.FromHours(2) };
            var thirdLog = new TimeLog { WorkDate = middleOfTheMonth, Hours = TimeSpan.FromHours(10) };
            var timeLogs = new List<TimeLog>
            {
                firstLog,
                secondLog,
                thirdLog
            };

            auth.Setup(x => x.GetAuthenticationData()).Returns((AuthenticationData)null);
            login.Setup(x => x.Login()).Returns(Task.Run(() => data));
            monthPicker.Setup(x => x.CurrentDate).Returns(startOfTheMonth);
            daysService.Setup(x => x.GetDays(startOfTheMonth))
                .Returns(() => days);
            zoho
                .Setup(x => x.TimeTracker.TimeLog.GetAsync(data.UserName, startOfTheMonth, endOfTheMonth, "all", "all"))
                .ReturnsAsync(timeLogs);

            target.ViewReady();

            Assert.True(target.IsLoggedIn);
            Assert.Equal(data.UserName, target.UserName);
            Assert.Equal(days, target.Days);

            Assert.True(target.LogoutCommand.CanExecute(null));
            Assert.False(target.LoginCommand.CanExecute(null));
            
            login.Verify(x => x.Login(), Times.Once);
            auth.Verify(x => x.SaveAuthenticationData(data));
            daysService.Verify(x => x.GetDays(startOfTheMonth), Times.Once);
            zoho.Verify(x => x.TimeTracker.TimeLog.GetAsync(data.UserName, startOfTheMonth, endOfTheMonth, "all", "all"), Times.Once);
            daysService.Verify(x => x.FillDaysWithTimeLogs(days, timeLogs), Times.Once);
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
            var startOfTheMonth = new DateTime(2015, 07, 01);
            var endOfTheMonth = startOfTheMonth.EndOfMonth();
            var middleOfTheMonth = new DateTime(2015, 07, 15);
            var days = 
                Enumerable.Range(0, 25)
                .Select(x => new DayViewModel())
                .ToList();
            var firstLog = new TimeLog {WorkDate = startOfTheMonth, Hours = TimeSpan.FromHours(8)};
            var secondLog = new TimeLog { WorkDate = endOfTheMonth, Hours = TimeSpan.FromHours(2)};
            var thirdLog = new TimeLog { WorkDate = middleOfTheMonth, Hours = TimeSpan.FromHours(10) };
            var timeLogs = new List<TimeLog>
            {
                firstLog,
                secondLog,
                thirdLog
            };

            auth
                .Setup(x => x.GetAuthenticationData())
                .Returns(data);
            monthPicker
                .Setup(x => x.CurrentDate)
                .Returns(startOfTheMonth);
            daysService
                .Setup(x => x.GetDays(startOfTheMonth))
                .Returns(() => days);
            zoho
                .Setup(x => x.TimeTracker.TimeLog.GetAsync(data.UserName, startOfTheMonth, endOfTheMonth, "all", "all"))
                .ReturnsAsync(timeLogs);
            
            target.ViewReady();

            Assert.True(target.IsLoggedIn);
            Assert.Equal(data.UserName, target.UserName);
            Assert.True(target.LogoutCommand.CanExecute(null));
            Assert.False(target.LoginCommand.CanExecute(null));
            Assert.Equal(days, target.Days);
            
            login.Verify(x => x.Login(), Times.Never);
            zoho.Verify(x => x.Login(data.Token), Times.Once);
            daysService.Verify(x => x.GetDays(startOfTheMonth), Times.Once);
            zoho.Verify(x => x.TimeTracker.TimeLog.GetAsync(data.UserName, startOfTheMonth, endOfTheMonth, "all", "all"), Times.Once);
            daysService.Verify(x => x.FillDaysWithTimeLogs(days, timeLogs), Times.Once);
        }

        [Theory, AutoMoqData]
        public void LogoutCommand_LoginInformationStored_UserIsLoggedOut(
            [Frozen]Mock<IAuthenticationStorage> auth,
            [Frozen]Mock<IDialogService> dialog,
            [Frozen]Mock<IZohoClient> zoho,
            [Frozen]Mock<ILoginController> login,
            [Frozen]Mock<IDaysService> daysService,
            [Frozen]AuthenticationData data,
            MainWindowViewModel target)
        {
            auth.Setup(x => x.GetAuthenticationData()).Returns(data);
            daysService.Setup(x => x.GetDays(It.IsAny<DateTime>()))
                .Returns(Enumerable.Range(0, 25).Select(x => new DayViewModel { IsFilled = true }).ToList());
            
            target.ViewReady();
            
            target.LogoutCommand.Execute(null);

            Assert.False(target.IsLoggedIn);
            Assert.Null(target.UserName);
            Assert.False(target.LogoutCommand.CanExecute(null));
            Assert.True(target.LoginCommand.CanExecute(null));
            Assert.All(target.Days, x => { Assert.False(x.IsFilled); });

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
