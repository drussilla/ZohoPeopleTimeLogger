using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;
using ZohoPeopleClient;
using ZohoPeopleTimeLogger.Controllers;
using ZohoPeopleTimeLogger.Events;
using ZohoPeopleTimeLogger.Model;
using ZohoPeopleTimeLogger.Services;
using ZohoPeopleTimeLogger.ViewModel;

namespace ZohoPeopleTimeLogger.UnitTests.ViewModels
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
                .Returns(() => Enumerable.Range(0, 25).Select(x => DayViewModel.DayFromOtherMonth(zoho.Object)).ToList());
            monthPicker.Setup(x => x.CurrentDate).Returns(date);

            var target = new MainWindowViewModel(auth.Object, dialog.Object, daysService.Object, login.Object,
                monthPicker.Object);

            Assert.Equal(DaysService.TotalDaysInATable, target.Days.Count);
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

            var days = Enumerable.Range(0, 25).Select(x => DayViewModel.DayFromOtherMonth(zoho.Object)).ToList();
            
            auth.Setup(x => x.GetAuthenticationData()).Returns((AuthenticationData)null);
            login.Setup(x => x.LoginWithPassword()).Returns(Task.Run(() => data));
            monthPicker.Setup(x => x.CurrentDate).Returns(startOfTheMonth);
            daysService.Setup(x => x.GetDays(startOfTheMonth))
                .Returns(() => days);
            
            target.ViewReady().Wait();

            Assert.True(target.IsLoggedIn);
            Assert.Equal(data.UserName, target.UserName);
            Assert.Equal(days, target.Days);

            Assert.True(target.LogoutCommand.CanExecute(null));
            Assert.False(target.LoginCommand.CanExecute(null));
            
            login.Verify(x => x.LoginWithPassword(), Times.Once);
            auth.Verify(x => x.SaveAuthenticationData(data));
            daysService.Verify(x => x.GetDays(startOfTheMonth), Times.Once);
            daysService.Verify(x => x.FillDaysWithTimeLogsAsync(days, startOfTheMonth), Times.Once);
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
            var days = 
                Enumerable.Range(0, 25)
                .Select(x => DayViewModel.DayFromOtherMonth(zoho.Object))
                .ToList();
            
            auth
                .Setup(x => x.GetAuthenticationData())
                .Returns(data);
            monthPicker
                .Setup(x => x.CurrentDate)
                .Returns(startOfTheMonth);
            daysService
                .Setup(x => x.GetDays(startOfTheMonth))
                .Returns(() => days);
            login
                .Setup(x => x.LoginWithToken(data))
                .ReturnsAsync(true);

            target.ViewReady().Wait();

            Assert.True(target.IsLoggedIn);
            Assert.Equal(data.UserName, target.UserName);
            Assert.True(target.LogoutCommand.CanExecute(null));
            Assert.False(target.LoginCommand.CanExecute(null));
            Assert.Equal(days, target.Days);
            
            login.Verify(x => x.LoginWithPassword(), Times.Never);
            login.Verify(x => x.LoginWithToken(data), Times.Once);
            daysService.Verify(x => x.GetDays(startOfTheMonth), Times.Once);
            daysService.Verify(x => x.FillDaysWithTimeLogsAsync(days, startOfTheMonth), Times.Once);
        }

        [Theory, AutoMoqData]
        public void ViewReady_LoginInformationStoredAndTokenIsNotValid_ShowLoginWithPassword(
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
            var days =
                Enumerable.Range(0, 25)
                .Select(x => DayViewModel.DayFromOtherMonth(zoho.Object))
                .ToList();

            auth
                .Setup(x => x.GetAuthenticationData())
                .Returns(data);
            monthPicker
                .Setup(x => x.CurrentDate)
                .Returns(startOfTheMonth);
            daysService
                .Setup(x => x.GetDays(startOfTheMonth))
                .Returns(() => days);
            login
                .Setup(x => x.LoginWithToken(data))
                .ReturnsAsync(false);

            target.ViewReady().Wait();

            Assert.False(target.IsLoggedIn);
            
            login.Verify(x => x.LoginWithPassword(), Times.Once);
            login.Verify(x => x.LoginWithToken(data), Times.Once);
            auth.Verify(x => x.Clear());
            daysService.Verify(x => x.GetDays(startOfTheMonth), Times.Never);
            daysService.Verify(x => x.FillDaysWithTimeLogsAsync(days, startOfTheMonth), Times.Never);
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
                .Returns(Enumerable.Range(0, 25).Select(x =>
                {
                    var day = DayViewModel.DayFromOtherMonth(zoho.Object);
                    day.IsFilled = true;
                    return day;
                }).ToList());

            target.ViewReady().Wait();
            
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
            [Frozen]Mock<IZohoClient> zoho,
            MainWindowViewModel target)
        {
            var newMonth = new DateTime(2013, 01, 01);
            var days = Enumerable.Range(0, 25).Select(x => DayViewModel.DayFromOtherMonth(zoho.Object)).ToList();
            daysService.Setup(x => x.GetDays(newMonth)).Returns(days);
            
            monthPicker.Raise(x => x.MonthChanged += null, new MonthChangedEventArgs(newMonth));

            daysService.Verify(x => x.GetDays(newMonth), Times.Once);
            Assert.Equal(days, target.Days);
        }

        [Theory, AutoMoqData]
        public void FillTimeCommand_NotFilledDays_DayServiceCalled(
            [Frozen]Mock<IDaysService> daysService,
            [Frozen]Mock<IMonthPickerViewModel> monthPicker,
            [Frozen]Mock<IZohoClient> zoho,
            MainWindowViewModel target)
        {
            var date = new DateTime(2015, 02, 01);
            var days = new List<IDayViewModel> { DayViewModel.DayFromOtherMonth(zoho.Object)};
            days.AddRange(
                Enumerable.Range(1, 23)
                    .Select(x => DayViewModel.DayFromThisMonth(x, date, zoho.Object))
                    .ToList());
            days.Add(DayViewModel.DayFromOtherMonth(zoho.Object));

            target.Days = days;
            monthPicker.Setup(x => x.CurrentDate).Returns(date);
            
            target.FillTimeCommand.Execute(null);

            daysService.Verify(x => x.FillMissingTimeLogsAsync(days));
        }

        [Theory, AutoMoqData]
        public void FillTimeCommand_AllDaysFilled_ShowDialog(
            [Frozen]Mock<IDaysService> daysService,
            [Frozen]Mock<IMonthPickerViewModel> monthPicker,
            [Frozen]Mock<IDialogService> dialog,
            [Frozen]Mock<IZohoClient> zoho,
            MainWindowViewModel target)
        {
            var date = new DateTime(2015, 02, 01);
            var days = new List<IDayViewModel> { DayViewModel.DayFromOtherMonth(zoho.Object) };
            days.AddRange(
                Enumerable.Range(1, 23)
                    .Select(x =>
                    {
                        var day = DayViewModel.DayFromThisMonth(x, date, zoho.Object);
                        day.IsFilled = true;
                        return day;
                    })
                    .ToList());

            days[3].IsFilled = false;
            days[3].MarkAsHoliday("holiday");

            days.Add(DayViewModel.DayFromOtherMonth(zoho.Object));

            target.Days = days;
            monthPicker.Setup(x => x.CurrentDate).Returns(date);

            target.FillTimeCommand.Execute(null);

            daysService.Verify(x => x.FillMissingTimeLogsAsync(days), Times.Never);
            dialog.Verify(x => x.ShowMessageAsync(It.IsAny<string>(), It.IsAny<string>()));
        }

        [Theory, AutoMoqData]
        public void FillSingleDayCommand_FillDayg(
            [Frozen]Mock<IDaysService> daysService,
            [Frozen]Mock<IDayViewModel> day,
            MainWindowViewModel target)
        {
            target.FillSingleDayCommand.Execute(day.Object);

            daysService.Verify(x => x.FillMissingTimeLogsAsync(day.Object), Times.Once);
        }
    }
}
