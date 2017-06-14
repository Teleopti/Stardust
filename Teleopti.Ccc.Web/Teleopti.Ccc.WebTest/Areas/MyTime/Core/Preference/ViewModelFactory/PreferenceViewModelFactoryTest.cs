using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Preference;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.ViewModelFactory
{
	[TestFixture]
	public class PreferenceViewModelFactoryTest
	{
		[Test]
		public void CreatePreferenceWeeklyWorkTimeViewModel()
		{
			 var weeklyWorkTimeProvider = MockRepository.GenerateMock<IPreferenceWeeklyWorkTimeSettingProvider>();
			 var target = new PreferenceViewModelFactory(null, null, null, weeklyWorkTimeProvider, null,null,null,null,null, null);
			 var weeklyWorkTimeSetting = new WeeklyWorkTimeSetting();
			 weeklyWorkTimeSetting.MinWorkTimePerWeekMinutes = 120;
			 weeklyWorkTimeSetting.MaxWorkTimePerWeekMinutes = 360;

			 weeklyWorkTimeProvider.Stub(x => x.RetrieveSetting(DateOnly.Today))
				 .Return(weeklyWorkTimeSetting);
			 var result = target.CreatePreferenceWeeklyWorkTimeViewModel(DateOnly.Today);

			 result.MaxWorkTimePerWeekMinutes.Should().Be.EqualTo(weeklyWorkTimeSetting.MaxWorkTimePerWeekMinutes);
			 result.MinWorkTimePerWeekMinutes.Should().Be.EqualTo(weeklyWorkTimeSetting.MinWorkTimePerWeekMinutes);
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

		private static void getPreferenceWeeklyWorkTimeVmWithDatesCommonTests(DateOnly date, IEnumerable<DateOnly> dates,
			int expectedResultCount)
		{
			var weeklyWorkTimeProvider = MockRepository.GenerateMock<IPreferenceWeeklyWorkTimeSettingProvider>();
			var target = new PreferenceViewModelFactory(null, null, null, weeklyWorkTimeProvider, null, null, null, null, null,
				null);

			var weeklyWorkTimeSetting = new WeeklyWorkTimeSetting
			{
				MinWorkTimePerWeekMinutes = 120,
				MaxWorkTimePerWeekMinutes = 360
			};
			weeklyWorkTimeProvider.Stub(x => x.RetrieveSetting(date)).IgnoreArguments().Return(weeklyWorkTimeSetting);

			var result = target.CreatePreferenceWeeklyWorkTimeViewModels(dates);

			result.Count.Should().Be.EqualTo(expectedResultCount);
			result[date].MaxWorkTimePerWeekMinutes.Should().Be.EqualTo(360);
			result[date].MinWorkTimePerWeekMinutes.Should().Be.EqualTo(120);
		}

		[Test]
		public void ShouldNotThrownExceptionWhenGetPreferenceWeeklyWorkTimeVMWithDuplicateDates()
		{
			var date = new DateOnly(2017, 4, 1);
			var dates = new List<DateOnly> {date, date.AddDays(7), date.AddDays(14), date};
			var weeklyWorkTimeProvider = MockRepository.GenerateMock<IPreferenceWeeklyWorkTimeSettingProvider>();
			var target = new PreferenceViewModelFactory(null, null, null, weeklyWorkTimeProvider, null, null, null, null, null, null);

			var weeklyWorkTimeSetting = new WeeklyWorkTimeSetting
			{
				MinWorkTimePerWeekMinutes = 120,
				MaxWorkTimePerWeekMinutes = 360
			};
			weeklyWorkTimeProvider.Stub(x => x.RetrieveSetting(date)).IgnoreArguments().Return(weeklyWorkTimeSetting);

			IDictionary<DateOnly, PreferenceWeeklyWorkTimeViewModel> result = null;
			Assert.DoesNotThrow(() => {
				result = target.CreatePreferenceWeeklyWorkTimeViewModels(dates);
			});

			result[date].MaxWorkTimePerWeekMinutes.Should().Be.EqualTo(360);
			result[date].MinWorkTimePerWeekMinutes.Should().Be.EqualTo(120);

		}
	}
}
