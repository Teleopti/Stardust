using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Core.Extensions;
using Teleopti.Ccc.WebTest.Core.IoC;


namespace Teleopti.Ccc.WebTest.Areas.MyTime.Controllers
{
	[TestFixture]
	[MyTimeWebTest]
	[SetCulture("sv-SE")]
	public class ScheduleApiControllerGetIntradayScheduleEdgeTimeTest
	{
		public ScheduleApiController Target;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public IScheduleStorage ScheduleData;
		public ICurrentScenario Scenario;
		public MutableNow Now;
		public ILoggedOnUser User;
		private string nowDateTimeStr = "2015-10-25 08:00";

		[Test]
		public void ShouldGetStartTime()
		{
			Now.Is(nowDateTimeStr);
			var date = new DateOnly(Now.UtcDateTime());
			var timePeriod = new DateTimePeriod(nowDateTimeStr.Utc(), nowDateTimeStr.Utc().AddHours(9));

			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, timePeriod);
			PersonAssignmentRepository.Has(personAssignment);

			var result = Target.GetIntradayScheduleEdgeTime(date);
			result.StartDateTime.Should().Be.EqualTo(timePeriod.StartDateTime.ToString(DateTimeFormatExtensions.FixedDateTimeFormat));
		}

		[Test]
		public void ShouldGetEndTime()
		{
			Now.Is(nowDateTimeStr);
			var date = new DateOnly(Now.UtcDateTime());
			var timePeriod = new DateTimePeriod(nowDateTimeStr.Utc(), nowDateTimeStr.Utc().AddHours(9));

			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, timePeriod);
			PersonAssignmentRepository.Has(personAssignment);

			var result = Target.GetIntradayScheduleEdgeTime(date);
			result.EndDateTime.Should().Be.EqualTo(timePeriod.EndDateTime.ToString(DateTimeFormatExtensions.FixedDateTimeFormat));
		}

		[Test]
		public void ShouldGetSchedulePeriod()
		{
			Now.Is(nowDateTimeStr);
			var date = new DateOnly(Now.UtcDateTime());
			var timePeriod = new DateTimePeriod(nowDateTimeStr.Utc(), nowDateTimeStr.Utc().AddHours(9));

			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);
			var phone = new Activity("p");
			personAssignment.AddActivity(phone, timePeriod);
			PersonAssignmentRepository.Has(personAssignment);

			var result = Target.GetIntradayScheduleEdgeTime(date);
			result.StartDateTime.Should().Be.EqualTo(timePeriod.StartDateTime.ToString(DateTimeFormatExtensions.FixedDateTimeFormat));
			result.EndDateTime.Should().Be.EqualTo(timePeriod.EndDateTime.ToString(DateTimeFormatExtensions.FixedDateTimeFormat));
		}
		
		[Test]
		public void ShouldGetDefaultOneUpcomingHourStartTimeAndEndTimeWhenAgentHasNoScheduleOnCurrentDay()
		{
			Now.Is(nowDateTimeStr);
			var date = new DateOnly(Now.UtcDateTime());
			var defaultDateTimePeriod = new DateTimePeriod(nowDateTimeStr.Utc().AddHours(1), nowDateTimeStr.Utc().AddHours(1));
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);

			PersonAssignmentRepository.Has(personAssignment);

			var result = Target.GetIntradayScheduleEdgeTime(date);
			result.StartDateTime.Should().Be.EqualTo(defaultDateTimePeriod.StartDateTime.ToString(DateTimeFormatExtensions.FixedDateTimeFormat));
			result.EndDateTime.Should().Be.EqualTo(defaultDateTimePeriod.EndDateTime.ToString(DateTimeFormatExtensions.FixedDateTimeFormat));
		}

		[Test]
		public void ShouldGetDefaultStartTimeAndEndTimeWhenAgentHasNoScheduleOnOtherDay()
		{
			Now.Is(nowDateTimeStr);
			var date = new DateOnly(Now.UtcDateTime().AddDays(1));
			var defaultDateTimePeriod = new DateTimePeriod(nowDateTimeStr.Utc().AddHours(1), nowDateTimeStr.Utc().AddHours(1));
			var personAssignment = new PersonAssignment(User.CurrentUser(), Scenario.Current(), date);

			PersonAssignmentRepository.Has(personAssignment);

			var result = Target.GetIntradayScheduleEdgeTime(date);
			result.StartDateTime.Should().Be.EqualTo(defaultDateTimePeriod.StartDateTime.ToString(DateTimeFormatExtensions.FixedDateTimeFormat));
			result.EndDateTime.Should().Be.EqualTo(defaultDateTimePeriod.EndDateTime.ToString(DateTimeFormatExtensions.FixedDateTimeFormat));
		}
	}
}