using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.ViewModelFactory;
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
	}
}
