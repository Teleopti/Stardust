using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.DataProvider
{
	[TestFixture]
	public class PreferenceWeeklyWorkTimeSettingProviderTest
	{
		[Test]
		public void ShouldRetrieveSetting()
		{
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			var virtualSchedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			var contract = MockRepository.GenerateMock<IContract>();

			var weeklyWorkTimeSetting = new WeeklyWorkTimeSetting();
			weeklyWorkTimeSetting.MaxWorkTimePerWeekMinutes = 360;
			weeklyWorkTimeSetting.MinWorkTimePerWeekMinutes = 120;
			var workDirective = new WorkTimeDirective(TimeSpan.FromMinutes(weeklyWorkTimeSetting.MinWorkTimePerWeekMinutes),
				TimeSpan.FromMinutes(weeklyWorkTimeSetting.MaxWorkTimePerWeekMinutes), new TimeSpan(), new TimeSpan());

			virtualSchedulePeriodProvider.Stub(x => x.VirtualSchedulePeriodForDate(DateOnly.Today)).Return(virtualSchedulePeriod);
			virtualSchedulePeriod.Stub(x => x.IsValid).Return(true);
			virtualSchedulePeriod.Stub(x => x.Contract).Return(contract);
			contract.Stub(x => x.WorkTimeDirective).Return(workDirective);

			var target = new PreferenceWeeklyWorkTimeSettingProvider(virtualSchedulePeriodProvider);

			var result = target.RetrieveSetting(DateOnly.Today);

			result.MinWorkTimePerWeekMinutes.Should().Be.EqualTo(weeklyWorkTimeSetting.MinWorkTimePerWeekMinutes);
			result.MaxWorkTimePerWeekMinutes.Should().Be.EqualTo(weeklyWorkTimeSetting.MaxWorkTimePerWeekMinutes);
		}
	}
}