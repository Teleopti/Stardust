using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.SeatPlanning;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	[TestFixture]
	internal class AddSeatPlanCommandHandlerTest
	{
		//RobTodo: Reenable when Handler creates seatplan
		//[Test, Ignore]
		//public void ShouldSetupEntityState()
		//{
		//	var seatPlanRepository = new FakeWriteSideRepository<ISeatPlan>();
		//	var target = new AddSeatPlanCommandHandler(seatPlanRepository, new FakeCurrentScenario());
		//	var operatedPersonId = Guid.NewGuid();
		//	var trackId = Guid.NewGuid();
			
		//	var command = new AddSeatPlanCommand()
		//	{
		//		StartDate = new DateTime(2013, 3, 25),
		//		EndDate = new DateTime(2013, 3, 25),
		//		TrackedCommandInfo = new TrackedCommandInfo()
		//		{
		//			OperatedPersonId = operatedPersonId,
		//			TrackId = trackId
		//		}
		//	};

		//	target.Handle(command);

		//	//Robtodo: Add values here to test
		//	var seatPlan = seatPlanRepository.Single() as SeatPlan;
		//	seatPlan.Period.StartDate.Date.Should().Be(command.StartDate.Date);
		//	seatPlan.Period.EndDate.Date.Should().Be(command.EndDate.Date);
		//}


		//RobTodo: Reenable when Handler creates seatplan
		//[Test, Ignore]
		//public void ShouldRaiseAddSeatPlanCommandEvent()
		//{
		//	var currentScenario = new FakeCurrentScenario();
		//	var seatPlanRepository = new FakeWriteSideRepository<ISeatPlan>();
		//	var target = new AddSeatPlanCommandHandler(seatPlanRepository, currentScenario);
		//	var operatedPersonId = Guid.NewGuid();
		//	var trackId = Guid.NewGuid();
	
		//	var command = new AddSeatPlanCommand()
		//	{
		//		StartDate = new DateTime (2013, 3, 25),
		//		EndDate = new DateTime (2013, 3, 25),
		//		TrackedCommandInfo = new TrackedCommandInfo()
		//		{
		//			OperatedPersonId = operatedPersonId,
		//			TrackId = trackId
		//		}
		//	};

		//	target.Handle (command);

		//	//Robtodo: Add values here to test
		//	var @event = seatPlanRepository.Single().PopAllEvents().Single() as SeatPlanAddedEvent;
		//	@event.ScenarioId.Should().Be (currentScenario.Current().Id.Value);
		//	@event.StartDate.Should().Be (command.StartDate);
		//	@event.EndDate.Should().Be (command.EndDate);
		//	@event.InitiatorId.Should().Be (operatedPersonId);
		//	@event.TrackId.Should().Be (trackId);
		//	@event.BusinessUnitId.Should().Be (currentScenario.Current().BusinessUnit.Id.GetValueOrDefault());
		//}
	}
}