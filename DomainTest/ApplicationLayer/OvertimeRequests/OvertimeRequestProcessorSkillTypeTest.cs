using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	public partial class OvertimeRequestProcessorTest
	{
		[Test]
		public void ShouldDenyWhenNoSkillTypeIsMatched()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { _emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
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
		public void ShouldDenyWhenNoSkillTypeIsMatchedWithMultiSkillTypes()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new []{ _emailSkillType , _chatSkillType})
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
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
		public void ShouldDenyWhenNoSkillTypeIsMatchedWithTwoOpenPeriods()
		{
			Now.Is(new DateTime(2018, 2, 1, 8, 0, 0, DateTimeKind.Utc));
			
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new[] { _chatSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				BetweenDays = new MinMax<int>(0, 48),
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { _chatSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(5))),
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
		public void ShouldDenyWhenNoSkillTypeIsMatchedWithTwoOpenPeriodsWithMultiSkillTypes()
		{
			Now.Is(new DateTime(2018, 2, 1, 8, 0, 0, DateTimeKind.Utc));
			
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod(new [] { _chatSkillType ,_emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				BetweenDays = new MinMax<int>(0, 48),
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new [] { _chatSkillType, _emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(5))),
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
		public void ShouldApproveWhenSkillTypeIsMatched()
		{
			setupPerson(8, 21);

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(skillType: _phoneSkillType), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 25, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApproveWhenAtLeastOneSkillTypeIsMatched()
		{
			setupPerson(8, 21);

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new [] { _phoneSkillType , _emailSkillType})
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(skillType: _phoneSkillType), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 25, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldGetAutoDenyReasonWhenOneSkillTypeIsMatchedButAutoGrantIsDeny()
		{
			setupPerson(8, 21);

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});

			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(skillType: _phoneSkillType), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 25, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be(Resources.OvertimeRequestDenyReasonAutodeny);
		}

		[Test]
		public void ShouldGetAutoDenyReasonWhenMultipleSkillTypesAreMatchedButAllAutoGrantIsDeny()
		{
			setupPerson(8, 21);

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new [] { _phoneSkillType , _chatSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13))),
				OrderIndex = 1
			});

			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { _emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13))),
				OrderIndex = 2
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var activityPhone = createActivity("activity phone");
			var skillPhone = createSkill("skill phone", null, TimeZoneInfo.Utc);
			skillPhone.SkillType = _phoneSkillType;
			var personSkillPhone = createPersonSkill(activityPhone, skillPhone);

			var activityEmail = createActivity("activity email");
			var skillEmail = createSkill("skill email", null, TimeZoneInfo.Utc);
			skillEmail.SkillType = _emailSkillType;
			skillEmail.AddWorkload(new Domain.Forecasting.Workload(skillEmail));
			var personSkillEmail = createPersonSkill(activityEmail, skillEmail);

			addPersonSkillsToPersonPeriod(personSkillPhone, personSkillEmail);

			setupIntradayStaffingForSkill(skillPhone, 10d, 5d);
			setupIntradayStaffingForSkill(skillEmail, 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 25, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be(Resources.OvertimeRequestDenyReasonAutodeny);
		}

		[Test]
		public void ShouldApproveWhenExistsASkillTypeWithAutoGrantIsYes()
		{
			setupPerson(8, 21);

			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13))),
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { _emailSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13))),
				OrderIndex = 2
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;

			var activityPhone = createActivity("activity phone");
			var skillPhone = createSkill("skill phone", null, TimeZoneInfo.Utc);
			skillPhone.SkillType = _phoneSkillType;
			var personSkillPhone = createPersonSkill(activityPhone, skillPhone);

			var activityEmail = createActivity("activity email");
			var skillEmail = createSkill("skill email", null, TimeZoneInfo.Utc);
			skillEmail.SkillType = _emailSkillType;
			skillEmail.AddWorkload(new Domain.Forecasting.Workload(skillEmail));
			var personSkillEmail = createPersonSkill(activityEmail, skillEmail);

			addPersonSkillsToPersonPeriod(personSkillPhone, personSkillEmail);

			setupIntradayStaffingForSkill(skillPhone, 10d, 1d);
			setupIntradayStaffingForSkill(skillEmail, 10d, 1d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 25, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApproveWhenSkillTypeIsNotSet()
		{
			setupPerson(8, 21);

			LoggedOnUser.CurrentUser().WorkflowControlSet.OvertimeRequestOpenPeriods.First().ClearPeriodSkillType();
			setupIntradayStaffingForSkill(setupPersonSkill(skillType: _phoneSkillType), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 25, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldSuggestCorrectPeriodWithMultipleSkillTypesWhenRequestDateIsOutOfRange()
		{
			Now.Is(new DateTime(2018, 4, 20, 8, 0, 0, DateTimeKind.Utc));
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(
				new OvertimeRequestOpenRollingPeriod(new[] {_emailSkillType, _chatSkillType, _phoneSkillType})
				{
					AutoGrantType = OvertimeRequestAutoGrantType.Yes,
					BetweenDays = new MinMax<int>(0, 9)
				});
			workflowControlSet.AddOpenOvertimeRequestPeriod(
				new OvertimeRequestOpenRollingPeriod(new[] {_emailSkillType, _chatSkillType})
				{
					AutoGrantType = OvertimeRequestAutoGrantType.Deny,
					BetweenDays = new MinMax<int>(0, 2)
				});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2018, 4, 30, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Trim().Should()
				.Be(string.Format(Resources.OvertimeRequestDenyReasonNoPeriod, "4/20/2018 - 4/29/2018"));
		}

		[Test]
		public void ShouldSuggestMergedPeriodWithMultipleSkillTypesWhenRequestDateIsOutOfRange()
		{
			Now.Is(new DateTime(2018, 4, 20, 8, 0, 0, DateTimeKind.Utc));
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(
				new OvertimeRequestOpenRollingPeriod(new[] { _emailSkillType })
				{
					AutoGrantType = OvertimeRequestAutoGrantType.Yes,
					BetweenDays = new MinMax<int>(0, 2)
				});
			workflowControlSet.AddOpenOvertimeRequestPeriod(
				new OvertimeRequestOpenRollingPeriod(new[] { _chatSkillType })
				{
					AutoGrantType = OvertimeRequestAutoGrantType.Yes,
					BetweenDays = new MinMax<int>(3, 4)
				});
			workflowControlSet.AddOpenOvertimeRequestPeriod(
				new OvertimeRequestOpenRollingPeriod(new[] { _phoneSkillType })
				{
					AutoGrantType = OvertimeRequestAutoGrantType.Yes,
					BetweenDays = new MinMax<int>(5, 6)
				});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2018, 4, 30, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Trim().Should()
				.Be(string.Format(Resources.OvertimeRequestDenyReasonNoPeriod, "4/20/2018 - 4/26/2018"));
		}

		[Test]
		public void ShouldSuggestMergedPeriodWithMultipleSkillTypesWhenRequestDateIsPartlyOutOfRange()
		{
			Now.Is(new DateTime(2018, 4, 20, 8, 0, 0, DateTimeKind.Utc));
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(
				new OvertimeRequestOpenRollingPeriod(new[] { _emailSkillType })
				{
					AutoGrantType = OvertimeRequestAutoGrantType.Yes,
					BetweenDays = new MinMax<int>(0, 2)
				});
			workflowControlSet.AddOpenOvertimeRequestPeriod(
				new OvertimeRequestOpenRollingPeriod(new[] { _chatSkillType })
				{
					AutoGrantType = OvertimeRequestAutoGrantType.Yes,
					BetweenDays = new MinMax<int>(3, 4)
				});
			workflowControlSet.AddOpenOvertimeRequestPeriod(
				new OvertimeRequestOpenRollingPeriod(new[] { _phoneSkillType })
				{
					AutoGrantType = OvertimeRequestAutoGrantType.Deny,
					BetweenDays = new MinMax<int>(5, 5)
				});
			workflowControlSet.AddOpenOvertimeRequestPeriod(
				new OvertimeRequestOpenRollingPeriod(new[] { _phoneSkillType })
				{
					AutoGrantType = OvertimeRequestAutoGrantType.Yes,
					BetweenDays = new MinMax<int>(6, 7)
				});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2018, 4, 25, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.False();
			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Trim().Should()
				.Be(string.Format(Resources.OvertimeRequestDenyReasonNoPeriod, "4/20/2018 - 4/24/2018,4/26/2018 - 4/27/2018"));
		}
	}
}