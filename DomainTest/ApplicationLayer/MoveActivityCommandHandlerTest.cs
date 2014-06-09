using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	public class MoveActivityCommandHandlerTest
	{
		[Test]
		public void ShouldMoveSingleLayer()
		{
			var agent = new Person();
			agent.SetId(Guid.NewGuid());
			var activity = new Activity("_");
			activity.SetId(Guid.NewGuid());
			var orgStart = new DateTime(2013, 11, 14, 6, 0, 0, 0, DateTimeKind.Utc);
			var orgEnd = new DateTime(2013, 11, 14, 11, 0, 0, 0, DateTimeKind.Utc);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(
				activity, agent,
				new DateTimePeriod(orgStart, orgEnd));

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository { assignment };
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
			var personRepository = new FakeWriteSideRepository<IPerson> { agent };
			var target = new MoveActivityCommandHandler(personAssignmentRepository, personRepository, scenario);

			var cmd = new MoveActivityCommand
			{
				AgentId = agent.Id.Value,
				Date = assignment.Date,
				ActivityId = assignment.ShiftLayers.First().Payload.Id.Value,
				NewStartTime = TimeSpan.FromHours(2)
			};

			target.Handle(cmd);

			var expectedStart = orgStart.Date.Add(cmd.NewStartTime);
			var modifiedLayer = personAssignmentRepository.Single().ShiftLayers.Single();
			modifiedLayer.Payload.Should().Be(activity);
			modifiedLayer.Period.StartDateTime.Should().Be(expectedStart);
			modifiedLayer.Period.EndDateTime.Should().Be(expectedStart + (orgEnd - orgStart));
		}
	}
}