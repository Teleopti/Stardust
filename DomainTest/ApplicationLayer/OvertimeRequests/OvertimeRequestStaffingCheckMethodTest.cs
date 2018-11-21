using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	public partial class OvertimeRequestProcessorTest
	{
		[Test]
		public void ShouldUseIntradayWithShrinkageCheckMethodByDefault()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);

			var personRequest = createOvertimeRequest(18, 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldUseIntradayCheckMethod()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);
			LoggedOnUser.CurrentUser().WorkflowControlSet.OvertimeRequestStaffingCheckMethod =
				OvertimeRequestStaffingCheckMethod.Intraday;

			var personRequest = createOvertimeRequest(18, 1);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be(Resources.NoUnderStaffingSkill);
		}

		[Test]
		[Toggle(Toggles.OvertimeRequestUseMostUnderStaffedSkill_47853)]
		public void ShouldUseIntradayCheckMethodWhenToggle47853IsOn()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);
			LoggedOnUser.CurrentUser().WorkflowControlSet.OvertimeRequestStaffingCheckMethod =
				OvertimeRequestStaffingCheckMethod.Intraday;

			var personRequest = createOvertimeRequest(18, 1);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be(Resources.NoUnderStaffingSkill);
		}

		[Test]
		public void ShouldUseIntradayCheckMethodWhenToggle74944IsOn()
		{
			setupPerson(8, 21);
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 8d);
			LoggedOnUser.CurrentUser().WorkflowControlSet.OvertimeRequestStaffingCheckMethod =
				OvertimeRequestStaffingCheckMethod.Intraday;

			var personRequest = createOvertimeRequest(18, 1);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be(Resources.NoUnderStaffingSkill);
		}
	}
}