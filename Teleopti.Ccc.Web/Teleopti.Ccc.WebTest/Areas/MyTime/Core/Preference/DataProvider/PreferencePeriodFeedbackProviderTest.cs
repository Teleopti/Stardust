using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.DataProvider
{
	[TestFixture]
	public class PreferencePeriodFeedbackProviderTest
	{
		[Test]
		public void ShouldReturnShouldHaveDaysOff()
		{
			var virtualSchedulePeriod = MockRepository.GenerateMock<IVirtualSchedulePeriod>();
			var virtualSchedulePeriodProvider = MockRepository.GenerateMock<IVirtualSchedulePeriodProvider>();
			virtualSchedulePeriod.Stub(x => x.DaysOff()).Return(5);
			virtualSchedulePeriodProvider.Stub(x => x.VirtualSchedulePeriodForDate(DateOnly.Today)).Return(virtualSchedulePeriod);

			var target = new PreferencePeriodFeedbackProvider(virtualSchedulePeriodProvider);

			var result = target.ShouldHaveDaysOff(DateOnly.Today);

			result.Should().Be(5);
		}
	}
}