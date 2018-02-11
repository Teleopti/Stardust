using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	public partial class OvertimeRequestProcessorTest
	{
		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSkillTypeSetting_47290)]
		public void ShouldDenyWhenNoSkillTypeIsMatched()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13))),
				SkillType = new SkillTypeEmail(new Description(SkillTypeIdentifier.Email), ForecastSource.Email).WithId()
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 25, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Trim().Should().Contain("There is no available skill for overtime");
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSkillTypeSetting_47290)]
		public void ShouldDenyWhenNoSkillTypeIsMatchedWithTwoOpenPeriods()
		{
			Now.Is(new DateTime(2018, 2, 1, 8, 0, 0, DateTimeKind.Utc));

			var phoneSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony).WithId();
			var chatSkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Chat), ForecastSource.Chat).WithId();

			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(0, 48),
				SkillType = phoneSkillType,
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(5))),
				SkillType = chatSkillType,
				OrderIndex = 2
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2018, 2, 6, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Trim().Should().Contain("There is no available skill for overtime");
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSkillTypeSetting_47290)]
		public void ShouldApproveWhenSkillTypeIsMatched()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13))),
				SkillType = new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony)
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 25, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSetting_46417)]
		[Toggle(Domain.FeatureFlags.Toggles.OvertimeRequestPeriodSkillTypeSetting_47290)]
		public void ShouldApproveWhenSkillTypeIsNotSet()
		{
			setupPerson(8, 21);

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});

			SkillTypeRepository.Add(new SkillTypePhone(new Description(SkillTypeIdentifier.Phone), ForecastSource.InboundTelephony));

			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 25, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}
	}
}