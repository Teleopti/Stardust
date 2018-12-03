using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Ccc.WebTest.Core.IoC;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.ViewModelFactory
{
	[MyTimeWebTest]
	[TestFixture]
	public class PreferenceViewModelFactoryTest
	{
		public IPreferenceViewModelFactory Target;
		public FakeLoggedOnUser LoggedOnUser;

		[Test]
		public void CreatePreferenceWeeklyWorkTimeViewModel()
		{
			setupPersonPeriod(DateOnly.Today.AddDays(-1));

			 var result = Target.CreatePreferenceWeeklyWorkTimeViewModel(DateOnly.Today);
			 result.MaxWorkTimePerWeekMinutes.Should().Be.EqualTo(360);
			 result.MinWorkTimePerWeekMinutes.Should().Be.EqualTo(120);
		}

		[Test]
		public void ShouldGetPreferenceWeeklyWorkTimeVmWithDates()
		{
			var date = new DateOnly(2017, 4, 1);
			var dates = new List<DateOnly>
			{
				date,
				date.AddDays(7),
				date.AddDays(14)
			};
			getPreferenceWeeklyWorkTimeVmWithDatesCommonTests(date, dates, 3);
		}

		[Test]
		public void ShouldGetPreferenceWeeklyWorkTimeVmWithDuplicateDates()
		{
			var date = new DateOnly(2017, 4, 1);
			var dates = new List<DateOnly>
			{
				date,
				date,
				date,
				date.AddDays(7),
				date.AddDays(7),
				date.AddDays(14),
				date.AddDays(14),
				date.AddDays(15)
			};
			getPreferenceWeeklyWorkTimeVmWithDatesCommonTests(date, dates, 4);
		}

		[Test]
		public void ShouldNotThrowExceptionWhenGetPreferenceWeeklyWorkTimeVmWithDuplicateDates()
		{
			var date = new DateOnly(2017, 4, 1);
			var dates = new List<DateOnly> {date, date.AddDays(7), date.AddDays(14), date};

			setupPersonPeriod(date.AddDays(-1));

			IDictionary<DateOnly, PreferenceWeeklyWorkTimeViewModel> result = null;
			Assert.DoesNotThrow(() => {
				result = Target.CreatePreferenceWeeklyWorkTimeViewModels(dates);
			});

			result[date].MaxWorkTimePerWeekMinutes.Should().Be.EqualTo(360);
			result[date].MinWorkTimePerWeekMinutes.Should().Be.EqualTo(120);

		}

		[Test]
		public void ShouldNotThrowExceptionWhenDateArrayIsNull()
		{
			var date = new DateOnly(2017, 4, 1);

			setupPersonPeriod(date.AddDays(-1));

			IDictionary<DateOnly, PreferenceWeeklyWorkTimeViewModel> result = null;
			Assert.DoesNotThrow(() => {
				result = Target.CreatePreferenceWeeklyWorkTimeViewModels(null);
			});

			result.Count.Should().Be.EqualTo(0);
		}

		private void getPreferenceWeeklyWorkTimeVmWithDatesCommonTests(DateOnly date, IEnumerable<DateOnly> dates,
			int expectedResultCount)
		{
			setupPersonPeriod(date.AddDays(-1));

			var result = Target.CreatePreferenceWeeklyWorkTimeViewModels(dates);

			result.Count.Should().Be.EqualTo(expectedResultCount);
			result[date].MaxWorkTimePerWeekMinutes.Should().Be.EqualTo(360);
			result[date].MinWorkTimePerWeekMinutes.Should().Be.EqualTo(120);
		}

		private void setupPersonPeriod(DateOnly startDate)
		{
			var person = LoggedOnUser.CurrentUser();
			var personContract = PersonContractFactory.CreatePersonContract(new Contract("test")
			{
				WorkTimeDirective = new Domain.InterfaceLegacy.Domain.WorkTimeDirective(TimeSpan.FromMinutes(120),
					TimeSpan.FromMinutes(360), TimeSpan.Zero, TimeSpan.Zero)
			});
			var personPeriod = PersonPeriodFactory.CreatePersonPeriod(startDate, personContract, new Team());
			person.AddPersonPeriod(personPeriod);
			person.AddSchedulePeriod(SchedulePeriodFactory.CreateSchedulePeriod(startDate));
		}
	}
}
