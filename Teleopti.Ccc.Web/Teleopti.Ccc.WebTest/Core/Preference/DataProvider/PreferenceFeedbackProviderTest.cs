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
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var workTimeMinMax = new WorkTimeMinMax();
			var person = new Person();

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);
			workTimeMinMaxCalculator.Stub(x => x.WorkTimeMinMax(DateOnly.Today, person, scheduleDay)).Return(workTimeMinMax);

			var target = new PreferenceFeedbackProvider(workTimeMinMaxCalculator, loggedOnUser);

			var result = target.WorkTimeMinMaxForDate(DateOnly.Today, scheduleDay);

			result.Should().Be(workTimeMinMax);
		}

	}
}