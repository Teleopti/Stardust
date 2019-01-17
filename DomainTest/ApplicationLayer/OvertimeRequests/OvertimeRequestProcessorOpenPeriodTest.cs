using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.DomainTest.ApplicationLayer.OvertimeRequests
{
	public partial class OvertimeRequestProcessorTest
	{
		[Test]
		public void ShouldDenyWhenThereIsNoOvertimeRequestOpenPeriod()
		{
			setupPerson(8, 21);
			LoggedOnUser.CurrentUser().WorkflowControlSet = new WorkflowControlSet();
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(18, 1);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be(Resources.OvertimeRequestDenyReasonClosedPeriod);
		}

		[Test]
		public void ShouldPendingWhenAutoGrantOfOpenPeriodIsNo()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 25, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsPending.Should().Be.True();
		}

		[Test]
		public void ShouldApproveAndChangeUpdatedByToSystemUserWhenAutoGrantOfOpenPeriodIsYes()
		{
			PersonRepository.Add(PersonFactory.CreatePerson().WithId(SystemUser.Id));

			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 25, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
			UpdatedBy.Person().Id.Value.Should().Be(SystemUser.Id);
		}

		[Test]
		public void ShouldApproveWhenRequestPeriodIsWithinAnOpenPeriod()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 25, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApproveWhenRequestPeriodIsWithinAllOpenPeriods()
		{
			setupPerson(8, 21);
			var now = new DateOnly(Now.UtcDateTime());
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(now, now.AddDays(13))
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(2, 13)
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 22, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldDenyWhenStaffingDataOfRequestPeriodIsNotAvailable()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(27)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 26, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be("Your overtime request has been denied automatically. The valid request period is 7/12/2017 - 7/25/2017.");
		}

		[Test]
		public void ShouldDenyWhenRequestPeriodIsOutsideAnOpenPeriod()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(13)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 26, 8, 0, 0, DateTimeKind.Utc), 1);
			getTarget(27).Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo("Your overtime request has been denied. Some days in the requested period are not open for requests. You can send requests for the following period: 7/12/2017 - 7/25/2017.");
		}

		[Test]
		public void ShouldDenyWhenRequestPeriodIsOutsideAllOpenPeriods()
		{
			setupPerson(8, 21);
			var now = new DateOnly(Now.UtcDateTime());
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(now, now.AddDays(13))
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(now.AddDays(15), now.AddDays(20))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 26, 9, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
		}

		[Test]
		public void ShouldDenyWhenRequestPeriodIsWithinOpenPeriodWithLargerOrderIndexAndAutoGrantIsDeny()
		{
			setupPerson(8, 21);
			var now = new DateOnly(Now.UtcDateTime());
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				Period = new DateOnlyPeriod(now, now.AddDays(8)),
				OrderIndex = 0
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(now.AddDays(5), now.AddDays(9)),
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				Period = new DateOnlyPeriod(now.AddDays(6), now.AddDays(10)),
				OrderIndex = 2
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 19, 9, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(Resources.OvertimeRequestDenyReasonAutodeny);
		}

		[Test]
		public void ShouldApproveWhenRequestPeriodIsWithinOpenPeriodWithLargerOrderIndexAndAutoGrantIsYes()
		{
			setupPerson(8, 21);
			var now = new DateOnly(Now.UtcDateTime());
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				Period = new DateOnlyPeriod(now, now.AddDays(8)),
				OrderIndex = 0
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				Period = new DateOnlyPeriod(now.AddDays(6), now.AddDays(10)),
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(now.AddDays(5), now.AddDays(9)),
				OrderIndex = 2
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 19, 9, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldDenyWhenRequestPeriodIsWithinRollingOpenPeriodWithLargerOrderIndexAndAutoGrantIsDeny()
		{
			setupPerson(8, 21);
			var now = new DateOnly(Now.UtcDateTime());
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				Period = new DateOnlyPeriod(now, now.AddDays(8)),
				OrderIndex = 0
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				Period = new DateOnlyPeriod(now.AddDays(5), now.AddDays(9)),
				OrderIndex = 1
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				BetweenDays = new MinMax<int>(6, 10),
				OrderIndex = 2
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 19, 9, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be.EqualTo(Resources.OvertimeRequestDenyReasonAutodeny);
		}

		[Test]
		public void ShouldApproveWhenRequestPeriodIsWithinRollingOpenPeriodWithLargerOrderIndexAndAutoGrantIsYes()
		{
			setupPerson(8, 21);
			var now = new DateOnly(Now.UtcDateTime());
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				Period = new DateOnlyPeriod(now, now.AddDays(8)),
				OrderIndex = 0
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(6, 10),
				OrderIndex = 2
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenRollingPeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Yes,
				BetweenDays = new MinMax<int>(5, 9),
				OrderIndex = 2
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 19, 9, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldDenyWhenRequestPeriodEndDateIsNotInOpenPeriod()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod()
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(1)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 13, 21, 0, 0, DateTimeKind.Utc), 6);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be("Your overtime request has been denied. Some days in the requested period are not open for requests. You can send requests for the following period: 7/12/2017 - 7/13/2017.");
		}

		[Test]
		public void ShouldSuggestMultiplePeriodsExcludeDenyPeriods()
		{
			setupPerson(0, 24);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(4)))
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.Deny,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()).AddDays(1), new DateOnly(Now.UtcDateTime().AddDays(1)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 17, 21, 0, 0, DateTimeKind.Utc), 6);
			getTarget(27).Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be("Your overtime request has been denied. Some days in the requested period are not open for requests. You can send requests for the following period: 7/12/2017 - 7/12/2017,7/14/2017 - 7/16/2017.");
		}

		[Test]
		public void ShouldSuggestMultiplePeriods()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(2)))
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()).AddDays(4), new DateOnly(Now.UtcDateTime().AddDays(6)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 15, 21, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be("Your overtime request has been denied. Some days in the requested period are not open for requests. You can send requests for the following period: 7/12/2017 - 7/14/2017,7/16/2017 - 7/18/2017.");
		}

		[Test]
		public void ShouldSuggestMultiplePeriodsNoEarlierThanToday()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()).AddDays(-2), new DateOnly(Now.UtcDateTime().AddDays(2)))
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()).AddDays(4), new DateOnly(Now.UtcDateTime().AddDays(6)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 15, 21, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be("Your overtime request has been denied. Some days in the requested period are not open for requests. You can send requests for the following period: 7/12/2017 - 7/14/2017,7/16/2017 - 7/18/2017.");
		}

		[Test]
		public void ShouldNotSuggestPeriodsWhenExpired()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()).AddDays(-4), new DateOnly(Now.UtcDateTime().AddDays(-1)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 15, 21, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be("Your overtime request cannot be granted. Some dates are not open for requests at this time.");
		}

		[Test]
		public void ShouldSuggestMergedPeriods()
		{
			setupPerson(8, 21);
			var workflowControlSet = new WorkflowControlSet();
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(5)))
			});
			workflowControlSet.AddOpenOvertimeRequestPeriod(new OvertimeRequestOpenDatePeriod(new[] { _phoneSkillType })
			{
				AutoGrantType = OvertimeRequestAutoGrantType.No,
				Period = new DateOnlyPeriod(new DateOnly(Now.UtcDateTime()), new DateOnly(Now.UtcDateTime().AddDays(2)))
			});
			LoggedOnUser.CurrentUser().WorkflowControlSet = workflowControlSet;
			setupIntradayStaffingForSkill(setupPersonSkill(new TimePeriod(TimeSpan.Zero, TimeSpan.FromDays(1))), 10d, 5d);

			var personRequest = createOvertimeRequest(new DateTime(2017, 7, 18, 21, 0, 0, DateTimeKind.Utc), 1);
			getTarget().Process(personRequest);

			personRequest.IsDenied.Should().Be.True();
			personRequest.DenyReason.Should().Be("Your overtime request has been denied. Some days in the requested period are not open for requests. You can send requests for the following period: 7/12/2017 - 7/17/2017.");
		}
	}
}