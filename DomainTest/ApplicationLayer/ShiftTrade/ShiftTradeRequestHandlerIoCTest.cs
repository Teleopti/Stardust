﻿using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.ShiftTrade;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ShiftTrade
{
	[DomainTest]
	[DefaultData]
	[Toggle(Toggles.MyTimeWeb_ShiftTradeRequest_MaximumWorkdayCheck_74889)]
	public class ShiftTradeRequestHandlerIoCTest
	{
		public ShiftTradeRequestHandler Target;
		public FakePersonRequestRepository PersonRequestRepository;
		public FakePersonRepository PersonRepository;
		public FakePersonAssignmentRepository PersonAssignmentRepository;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldDenyWhenPersonFromBreakMaximumWorkdayRule()
		{
			Now.Is("2018-05-09 06:00");
			var personFromId = Guid.NewGuid();
			var personToId = Guid.NewGuid();
			Database
				.WithShiftTradeWorkflow(2)
				.WithBusinessRuleForShiftTrade()
				.WithAgent(personFromId)
				.WithSchedule("2018-05-09 8:00", "2018-05-09 16:00")
				.WithSchedule("2018-05-10 8:00", "2018-05-10 16:00")
				.WithScheduleDayOff("2018-05-11")
				.WithAgent(personToId)
				.WithSchedule("2018-05-11 8:00", "2018-05-11 16:00")
				.WithShiftTradeRequest(personFromId, personToId, "2018-05-11")
				;
			Target.Handle(createAcceptShiftTradeEvent(personToId));

			var handledRequest = PersonRequestRepository.Get(Database.CurrentPersonRequestId());
			var personBreakRule = PersonRepository.Get(personFromId);
			handledRequest.IsDenied.Should().Be.True();
			handledRequest.DenyReason.Should().Be.EqualTo(String.Format(Resources.BusinessRuleMaximumWorkdayErrorMessage, personBreakRule.Name, 3, 2));
		}

		[Test]
		public void ShouldApproveWithMaximumWorkdayRule()
		{
			Now.Is("2018-05-09 06:00");
			var personFromId = Guid.NewGuid();
			var personToId = Guid.NewGuid();
			Database
				.WithShiftTradeWorkflow(3)
				.WithBusinessRuleForShiftTrade()
				.WithAgent(personFromId)
				.WithSchedule("2018-05-09 8:00", "2018-05-09 16:00")
				.WithSchedule("2018-05-10 8:00", "2018-05-10 16:00")
				.WithScheduleDayOff("2018-05-11")
				.WithAgent(personToId)
				.WithSchedule("2018-05-11 8:00", "2018-05-11 16:00")
				.WithShiftTradeRequest(personFromId, personToId, "2018-05-11")
				;

			Target.Handle(createAcceptShiftTradeEvent(personToId));

			var handledRequest = PersonRequestRepository.Get(Database.CurrentPersonRequestId());
			handledRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApproveWhenNotUseMaximumWorkday()
		{
			Now.Is("2018-05-09 06:00");
			var personFromId = Guid.NewGuid();
			var personToId = Guid.NewGuid();
			Database
				.WithShiftTradeWorkflow(2)
				.WithBusinessRuleForShiftTrade()
				.WithAgent(personFromId)
				.WithSchedule("2018-05-09 8:00", "2018-05-09 16:00")
				.WithSchedule("2018-05-10 8:00", "2018-05-10 16:00")
				.WithScheduleDayOff("2018-05-11")
				.WithAgent(personToId)
				.WithSchedule("2018-05-11 8:00", "2018-05-11 16:00")
				.WithShiftTradeRequest(personFromId, personToId, "2018-05-11")
				;

			Target.Handle(createAcceptShiftTradeEvent(personToId, false));

			var handledRequest = PersonRequestRepository.Get(Database.CurrentPersonRequestId());
			handledRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldApprovedWhenMaximumWorkdaySettingDisabled()
		{
			Now.Is("2018-05-09 06:00");
			var personFromId = Guid.NewGuid();
			var personToId = Guid.NewGuid();
			Database
				.WithShiftTradeWorkflow(2)
				.WithBusinessRuleForShiftTrade(false)
				.WithAgent(personFromId)
				.WithSchedule("2018-05-09 8:00", "2018-05-09 16:00")
				.WithSchedule("2018-05-10 8:00", "2018-05-10 16:00")
				.WithScheduleDayOff("2018-05-11")
				.WithAgent(personToId)
				.WithSchedule("2018-05-11 8:00", "2018-05-11 16:00")
				.WithShiftTradeRequest(personFromId, personToId, "2018-05-11")
				;

			Target.Handle(createAcceptShiftTradeEvent(personToId));

			var handledRequest = PersonRequestRepository.Get(Database.CurrentPersonRequestId());
			handledRequest.IsApproved.Should().Be.True();
		}

		[Test]
		public void ShouldPendingWhenMaximumWorkdayRuleIsBroken()
		{
			Now.Is("2018-05-09 06:00");
			var personFromId = Guid.NewGuid();
			var personToId = Guid.NewGuid();
			Database
				.WithShiftTradeWorkflow(2)
				.WithBusinessRuleForShiftTrade(true, RequestHandleOption.Pending)
				.WithAgent(personFromId)
				.WithSchedule("2018-05-09 8:00", "2018-05-09 16:00")
				.WithSchedule("2018-05-10 8:00", "2018-05-10 16:00")
				.WithScheduleDayOff("2018-05-11")
				.WithAgent(personToId)
				.WithSchedule("2018-05-11 8:00", "2018-05-11 16:00")
				.WithShiftTradeRequest(personFromId, personToId, "2018-05-11")
				;

			Target.Handle(createAcceptShiftTradeEvent(personToId));

			var handledRequest = PersonRequestRepository.Get(Database.CurrentPersonRequestId());
			handledRequest.IsPending.Should().Be.True();
		}

		[Test]
		public void ShouldDenyWhenToPersonBreakMaximumWorkdayRule()
		{
			Now.Is("2018-05-09 06:00");
			var personFromId = Guid.NewGuid();
			var personToId = Guid.NewGuid();
			Database
				.WithShiftTradeWorkflow(1)
				.WithBusinessRuleForShiftTrade()
				.WithAgent(personToId)
				.WithSchedule("2018-05-10 8:00", "2018-05-10 16:00")
				.WithScheduleDayOff("2018-05-11")
				.WithAgent(personFromId)
				.WithSchedule("2018-05-11 8:00", "2018-05-11 16:00")
				.WithShiftTradeRequest(personFromId, personToId, "2018-05-11")
				;

			Target.Handle(createAcceptShiftTradeEvent(personToId));

			var handledRequest = PersonRequestRepository.Get(Database.CurrentPersonRequestId());
			var personBreakRule = PersonRepository.Get(personToId);
			handledRequest.IsDenied.Should().Be.True();
			handledRequest.DenyReason.Should().Be.EqualTo(String.Format(Resources.BusinessRuleMaximumWorkdayErrorMessage, personBreakRule.Name, 2, 1));
		}

		[Test]
		public void ShouldApproveWithNotConsecutiveWorkday()
		{
			Now.Is("2018-05-09 06:00");
			var personFromId = Guid.NewGuid();
			var personToId = Guid.NewGuid();
			Database
				.WithShiftTradeWorkflow(2)
				.WithBusinessRuleForShiftTrade()
				.WithAgent(personFromId)
				.WithSchedule("2018-05-09 8:00", "2018-05-09 16:00")
				.WithScheduleDayOff("2018-05-10")
				.WithScheduleDayOff("2018-05-11")
				.WithSchedule("2018-05-12 8:00", "2018-05-12 16:00")
				.WithAgent(personToId, "agentTo", TimeZoneInfoFactory.StockholmTimeZoneInfo())
				.WithSchedule("2018-05-11 8:00", "2018-05-11 16:00")
				.WithShiftTradeRequest(personFromId, personToId, "2018-05-11")
				;

			Target.Handle(createAcceptShiftTradeEvent(personToId));

			var handledRequest = PersonRequestRepository.Get(Database.CurrentPersonRequestId());
			handledRequest.IsApproved.Should().Be.True();
		}

		private AcceptShiftTradeEvent createAcceptShiftTradeEvent(Guid personToId, bool useMaximumWorkday = true)
		{
			return new AcceptShiftTradeEvent
			{
				LogOnDatasource = Database.TenantName(),
				LogOnBusinessUnitId = Database.CurrentBusinessUnitId(),
				AcceptingPersonId = personToId,
				PersonRequestId = Database.CurrentPersonRequestId(),
				UseMaximumWorkday = useMaximumWorkday
			};
		}
	}
}