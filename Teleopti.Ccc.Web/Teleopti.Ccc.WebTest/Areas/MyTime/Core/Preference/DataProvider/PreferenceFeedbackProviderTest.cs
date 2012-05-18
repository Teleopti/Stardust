using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.MyTime.Core.Preference.DataProvider
{
	[TestFixture]
	public class PreferenceFeedbackProviderTest
	{
		[Test]
		public void ShouldRetrieveScheduleDayForDate()
		{
			var workTimeMinMaxCalculator = MockRepository.GenerateMock<IWorkTimeMinMaxCalculator>();
			var scheduleProvider = MockRepository.GenerateMock<IScheduleProvider>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();

			scheduleProvider.Stub(x => x.GetScheduleForPeriod(new DateOnlyPeriod(DateOnly.Today, DateOnly.Today))).Return(new[] {scheduleDay});

			var target = new PreferenceFeedbackProvider(workTimeMinMaxCalculator, MockRepository.GenerateMock<ILoggedOnUser>(), scheduleProvider);

			PreferenceType? preferenceType; 
			target.WorkTimeMinMaxForDate(DateOnly.Today, out preferenceType);

			workTimeMinMaxCalculator.AssertWasCalled(x => x.WorkTimeMinMax(DateOnly.Today, null, scheduleDay, out preferenceType));
		}

		[Test]
		public void ShouldReturnWorkTimeMinMaxForScheduleDay()
		{
			var workTimeMinMaxCalculator = MockRepository.GenerateMock<IWorkTimeMinMaxCalculator>();
			var loggedOnUser = MockRepository.GenerateMock<ILoggedOnUser>();
			var scheduleDay = MockRepository.GenerateMock<IScheduleDay>();
			var workTimeMinMax = new WorkTimeMinMax();
			var person = new Person();

			loggedOnUser.Stub(x => x.CurrentUser()).Return(person);

			PreferenceType? preferenceType;
			workTimeMinMaxCalculator.Stub(x => x.WorkTimeMinMax(DateOnly.Today, person, scheduleDay, out preferenceType)).Return(workTimeMinMax);

			var target = new PreferenceFeedbackProvider(workTimeMinMaxCalculator, loggedOnUser, null);

			var result = target.WorkTimeMinMaxForDate(DateOnly.Today, scheduleDay, out preferenceType);

			result.Should().Be(workTimeMinMax);
		}

	}
}