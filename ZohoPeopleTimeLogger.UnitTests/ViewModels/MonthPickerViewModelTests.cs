using System;
using System.CodeDom;
using System.Collections;
using Moq;
using Ploeh.AutoFixture.Xunit2;
using Xunit;
using Xunit.Extensions;
using ZohoPeopleTimeLogger.Services;
using ZohoPeopleTimeLogger.ViewModel;

namespace ZohoPeopleTimeLogger.UnitTests
{
    public class MonthPickerViewModelTests
    {
        [Theory, AutoMoqData]
        public void Ctor_CurrentDateSet(Mock<IDateTimeService> dateTime)
        {
            var currentDate = DateTime.Now;
            dateTime.Setup(x => x.Now).Returns(currentDate);

            var target = new MonthPickerViewModel(dateTime.Object);

            Assert.Equal(currentDate.Month, target.CurrentDate.Month);
            Assert.Equal(currentDate.ToString("MMMM"), target.CurrentDate.ToString("MMMM"));
            Assert.Equal(currentDate.Year, target.CurrentDate.Year);
        }

        public static IEnumerable NextMonthCommand_MonthChangedAndEventRised_Data()
        {
            yield return new object[] { new DateTime(2010, 1, 1), new DateTime(2010, 2, 1), 1 }; 
            yield return new object[] { new DateTime(2010, 1, 1), new DateTime(2010, 12, 1), 11 };
            yield return new object[] { new DateTime(2010, 1, 1), new DateTime(2011, 1, 1), 12 };
            yield return new object[] { new DateTime(2010, 1, 1), new DateTime(2015, 1, 1), 60 }; 
        }

        [Theory]
        [MemberData("NextMonthCommand_MonthChangedAndEventRised_Data")]
        public void NextMonthCommand_MonthChangedAndEventRised(
            DateTime original,
            DateTime expected,
            int runCommandTime)
        {
            Mock<IDateTimeService> dateTime = new Mock<IDateTimeService>();
            dateTime.Setup(x => x.Now).Returns(original);
            var actualEventsAmount = 0;
            var lastMonthFromEvent = 0;
            var lastYearFromEvent = 0; 

            var target = new MonthPickerViewModel(dateTime.Object);
            target.MonthChanged += (sender, args) =>
            {
                actualEventsAmount++;
                lastMonthFromEvent = args.NewMonth.Month;
                lastYearFromEvent = args.NewMonth.Year;
            };

            for (int i = 0; i < runCommandTime; i++)
            {
                target.NextMonthCommand.Execute(null);
            }

            Assert.Equal(expected.Month, target.CurrentDate.Month);
            Assert.Equal(expected.Year, target.CurrentDate.Year);
            Assert.Equal(runCommandTime, actualEventsAmount);
            Assert.Equal(expected.Month, lastMonthFromEvent);
            Assert.Equal(expected.Year, lastYearFromEvent);
        }

        [Fact]
        public void NextMonthCommand_MaximumYear_StayOnMaximum()
        {
            Mock<IDateTimeService> dateTime = new Mock<IDateTimeService>();
            dateTime.Setup(x => x.Now).Returns(DateTime.MaxValue);
            var actualEventsAmount = 0;
            var target = new MonthPickerViewModel(dateTime.Object);
            target.MonthChanged += (sender, args) =>
            {
                actualEventsAmount++;
            };

            target.NextMonthCommand.Execute(null);

            Assert.Equal(DateTime.MaxValue.Month, target.CurrentDate.Month);
            Assert.Equal(DateTime.MaxValue.Year, target.CurrentDate.Year);
            Assert.Equal(0, actualEventsAmount);
        }

        public static IEnumerable PrevMonthCommand_MonthChangedAndEventRised_Data()
        {
            yield return new object[] { new DateTime(2010, 1, 1), new DateTime(2009, 12, 1), 1 };
            yield return new object[] { new DateTime(2010, 12, 1), new DateTime(2010, 1, 1), 11 };
            yield return new object[] { new DateTime(2010, 1, 1), new DateTime(2009, 1, 1), 12 };
            yield return new object[] { new DateTime(2010, 1, 1), new DateTime(2005, 1, 1), 60 };
        }

        [Theory]
        [MemberData("PrevMonthCommand_MonthChangedAndEventRised_Data")]
        public void PrevMonthCommand_MonthChangedAndEventRised(
            DateTime original,
            DateTime expected,
            int runCommandTimes)
        {
            Mock<IDateTimeService> dateTime = new Mock<IDateTimeService>();
            dateTime.Setup(x => x.Now).Returns(original);
            var actualEventsAmount = 0;
            var lastMonthFromEvent = 0;
            var lastYearFromEvent = 0;

            var target = new MonthPickerViewModel(dateTime.Object);
            target.MonthChanged += (sender, args) =>
            {
                actualEventsAmount++;
                lastMonthFromEvent = args.NewMonth.Month;
                lastYearFromEvent = args.NewMonth.Year;
            };

            for (int i = 0; i < runCommandTimes; i++)
            {
                target.PreviousMonthCommand.Execute(null);
            }

            Assert.Equal(expected.Month, target.CurrentDate.Month);
            Assert.Equal(expected.Year, target.CurrentDate.Year);
            Assert.Equal(runCommandTimes, actualEventsAmount);
            Assert.Equal(expected.Month, lastMonthFromEvent);
            Assert.Equal(expected.Year, lastYearFromEvent);
        }

        [Fact]
        public void PrevMonthCommand_MinimumYear_StayOnMinimum()
        {
            Mock<IDateTimeService> dateTime = new Mock<IDateTimeService>();
            dateTime.Setup(x => x.Now).Returns(DateTime.MinValue);
            var actualEventsAmount = 0;

            var target = new MonthPickerViewModel(dateTime.Object);
            target.MonthChanged += (sender, args) =>
            {
                actualEventsAmount++;
            };
            target.PreviousMonthCommand.Execute(null);

            Assert.Equal(DateTime.MinValue.Month, target.CurrentDate.Month);
            Assert.Equal(DateTime.MinValue.Year, target.CurrentDate.Year);
            Assert.Equal(0, actualEventsAmount);
        }
    }
}