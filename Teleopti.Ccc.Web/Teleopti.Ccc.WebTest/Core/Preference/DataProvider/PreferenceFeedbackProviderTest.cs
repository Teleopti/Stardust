using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.ShiftCreator;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.Mapping;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Core.Preference.DataProvider
{
	[TestFixture]
	public class PreferenceFeedbackProviderTest
	{
		[Test]
		public void ShouldReturnWorkTimeMinMaxForDate()
		{
			var workTimeMinMaxCalculator = MockRepository.GenerateMock<IWorkTimeMinMaxCalculator>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var workTimeMinMax = new WorkTimeMinMax();

			workTimeMinMaxCalculator.Stub(x => x.WorkTimeMinMax(scheduleDay)).Return(workTimeMinMax);

			var target = new PreferenceFeedbackProvider(workTimeMinMaxCalculator);

			var result = target.WorkTimeMinMaxForDate(scheduleDay);

			result.Should().Be(workTimeMinMax);
		}

	}
}