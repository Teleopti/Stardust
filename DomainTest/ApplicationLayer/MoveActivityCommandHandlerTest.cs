using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer
{
	public class MoveActivityCommandHandlerTest
	{
		[Test]
		public void ShouldDoThrowIfPersonAssignmentNotExists()
		{
			var agent = new Person().WithId();
			var activity = new Activity("act").WithId();
			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository();
			var scenario = new ThisCurrentScenario(new Scenario(" "));
			var personRepository = new FakeWriteSideRepository<IPerson> { agent };
			var target = new MoveActivityCommandHandler(personAssignmentRepository, personRepository, new FakeWriteSideRepository<IActivity>{activity}, scenario);

			var cmd = new MoveActivityCommand
			{
				AgentId = agent.Id.Value,
				Date = new DateOnly(DateTime.UtcNow),
				ActivityId = activity.Id.Value,
				NewStartTime = TimeSpan.FromHours(4),
				OldStartTime = new DateTime(2000,1,1),
				OldProjectionLayerLength = TimeSpan.FromHours(2)
			};

			var ex = Assert.Throws<InvalidOperationException>(()=>target.Handle(cmd)).ToString();
			ex.Should().Contain(cmd.AgentId.ToString());
			ex.Should().Contain(cmd.Date.ToString());
			ex.Should().Contain(scenario.Current().Description.ToString());
		}

		[Test]
		public void ShouldChangeState()
		{
			var agent = new Person().WithId();
			var activity = new Activity("_").WithId();
			var orgStart = createDateTime(6);
			var orgEnd = createDateTime(11);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, new DateTimePeriod(orgStart, orgEnd));

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository { assignment };
			var activityRepository = new FakeWriteSideRepository<IActivity> { activity };
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
			var personRepository = new FakeWriteSideRepository<IPerson> { agent };
			var target = new MoveActivityCommandHandler(personAssignmentRepository, personRepository, activityRepository, scenario);

			var cmd = new MoveActivityCommand
			{
				AgentId = agent.Id.Value,
				Date = assignment.Date,
				ActivityId = activity.Id.Value,
				NewStartTime = TimeSpan.FromHours(2),
				OldStartTime = orgStart,
				OldProjectionLayerLength = orgEnd - orgStart
			};

			target.Handle(cmd);

			var expectedStart = orgStart.Date.Add(cmd.NewStartTime);
			var modifiedLayer = personAssignmentRepository.Single().ShiftLayers.Single();
			modifiedLayer.Payload.Should().Be(activity);
			modifiedLayer.Period.StartDateTime.Should().Be(expectedStart);
			modifiedLayer.Period.EndDateTime.Should().Be(expectedStart + (orgEnd - orgStart));
		}

		private DateTime createDateTime(int hourOnDay)
		{
			return new DateTime(2013, 11, 14, hourOnDay, 0, 0, 0, DateTimeKind.Utc);
		}
	}
}