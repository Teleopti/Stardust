using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	public partial class OvertimeRequestProcessorTest
	{
		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestAtLeastOneCriticalUnderStaffedSkill_74944)]
		public void ShouldApproveWhenAtLeastOneSkillIsCriticalUnderStaffed()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new Interfaces.Domain.MinMax<int>(0, 24)
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var activity1 = createActivity("activity1");
			var activity2 = createActivity("activity2");
			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var notUnderStaffingSkill = createSkill("notUnderStaffingSkill", null, timeZone);
			var criticalUnderStaffingSkill = createSkill("criticalUnderStaffingSkill", null, timeZone);

			var personSkill1 = createPersonSkill(activity1, notUnderStaffingSkill);
			var personSkill2 = createPersonSkill(activity2, criticalUnderStaffingSkill);

			setupIntradayStaffingForSkill(notUnderStaffingSkill, 10d, 15d);
			setupIntradayStaffingForSkill(criticalUnderStaffingSkill, 10d, 6d);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequest(11, 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestAtLeastOneCriticalUnderStaffedSkill_74944)]
		public void ShouldDenyWhenOnlyUnderStaffingButNoCriticalSkillAndToggle74944IsOn()
		{
			setupPerson();

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new Interfaces.Domain.MinMax<int>(0, 24)
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var activity1 = createActivity("activity1");
			var activity2 = createActivity("activity2");
			var timeZone = LoggedOnUser.CurrentUser().PermissionInformation.DefaultTimeZone();
			var notUnderStaffingSkill = createSkill("notUnderStaffingSkill", null, timeZone);
			var underStaffedButNotCriticalSkill = createSkill("criticalUnderStaffingSkill", null, timeZone);

			var personSkill1 = createPersonSkill(activity1, notUnderStaffingSkill);
			var personSkill2 = createPersonSkill(activity2, underStaffedButNotCriticalSkill);

			setupIntradayStaffingForSkill(notUnderStaffingSkill, 10d, 20d);
			setupIntradayStaffingForSkill(underStaffedButNotCriticalSkill, 10d, 15d);

			addPersonSkillsToPersonPeriod(personSkill1, personSkill2);

			var personRequest = createOvertimeRequest(11, 1);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
		}
	}
}