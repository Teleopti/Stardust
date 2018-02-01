using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	public partial class OvertimeRequestProcessorTest
	{
		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldUseActivityOfMostUnderstaffedSkillAsOvertimeActivity()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var criticalUnderStaffedActivity = createActivity("activity1");
			var moreCriticalUnderStaffedActivity = createActivity("activity2");

			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var criticalUnderStaffedSkill1 = createSkill("criticalUnderStaffedSkill1", null, timeZone);
			var criticalUnderStaffedSkill2 = createSkill("criticalUnderStaffedSkill2", null, timeZone);

			var personSkill1 = createPersonSkill(criticalUnderStaffedActivity, criticalUnderStaffedSkill1);
			var personSkill2 = createPersonSkill(moreCriticalUnderStaffedActivity, criticalUnderStaffedSkill2);

			setupIntradayStaffingForSkill(criticalUnderStaffedSkill1, 10d, 2d);
			setupIntradayStaffingForSkill(criticalUnderStaffedSkill2, 10d, 1d);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequest(11, 1);
			getTarget().Process(personRequest);

			var schedule = ScheduleStorage.FindSchedulesForPersonOnlyInGivenPeriod(LoggedOnUser.CurrentUser(),
				new ScheduleDictionaryLoadOptions(false, false), personRequest.Request.Period, Scenario.Current());
			var personAssignment = schedule[LoggedOnUser.CurrentUser()].ScheduledDay(new DateOnly(personRequest.Request.Period.StartDateTime))
				.PersonAssignment();

			personRequest.IsApproved.Should().Be.True();

			personAssignment.OvertimeActivities().First().Payload.Should().Be(moreCriticalUnderStaffedActivity);
		}
	}
}