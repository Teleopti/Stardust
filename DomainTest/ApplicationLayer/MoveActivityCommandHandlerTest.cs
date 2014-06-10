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
		public void ShouldMoveAndResizeALayerIfLayerAndProjectionHaveDifferentDuration()
		{
			var agent = new Person();
			agent.SetId(Guid.NewGuid());
			var activity = new Activity("theone");
			activity.SetId(Guid.NewGuid());
			var activityNotBeMoved = new Activity("justanotherone");
			activityNotBeMoved.SetId(Guid.NewGuid());
			var orgStartActivity = createDateTime(2);
			var orgEndActivity = createDateTime(5);
			var orgStartActivityNotBeMoved = createDateTime(4);
			var orgEndActivityNotBeMoved = createDateTime(7);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent,
				new DateTimePeriod(orgStartActivity, orgEndActivity));
			assignment.AddActivity(activityNotBeMoved, new DateTimePeriod(orgStartActivityNotBeMoved, orgEndActivityNotBeMoved));

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository { assignment };
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
			var personRepository = new FakeWriteSideRepository<IPerson> { agent };
			var target = new MoveActivityCommandHandler(personAssignmentRepository, personRepository, scenario);

			var cmd = new MoveActivityCommand
			{
				AgentId = agent.Id.Value,
				Date = assignment.Date,
				ActivityId = activity.Id.Value,
				NewStartTime = TimeSpan.FromHours(4),
				OldStartTime = orgStartActivity,
				OldProjectionLayerLength = TimeSpan.FromHours(2) //length of layer in projection
			};

			target.Handle(cmd);

			var projection = assignment.ProjectionService().CreateProjection();
			projection.Count().Should().Be.EqualTo(2);
			projection.First().Period.StartDateTime.Should().Be.EqualTo(cmd.Date.Date.Add(cmd.NewStartTime));
			projection.First().Period.ElapsedTime().Should().Be.EqualTo(orgStartActivityNotBeMoved.Subtract(orgStartActivity));
		}


		[Test]
		public void ShouldMoveASpecificLayerIfTwoLayersWithSameActivityExist()
		{
			var agent = new Person();
			agent.SetId(Guid.NewGuid());
			var activity = new Activity("theone");
			activity.SetId(Guid.NewGuid());
			var orgStartActivity = createDateTime(2);
			var orgEndActivity = createDateTime(5);
			var orgStartActivityNotBeMoved = createDateTime(11);
			var orgEndActivityNotBeMoved = createDateTime(15);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent,
				new DateTimePeriod(orgStartActivity, orgEndActivity));
			assignment.AddActivity(activity, new DateTimePeriod(orgStartActivityNotBeMoved, orgEndActivityNotBeMoved));

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository { assignment };
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
			var personRepository = new FakeWriteSideRepository<IPerson> { agent };
			var target = new MoveActivityCommandHandler(personAssignmentRepository, personRepository, scenario);

			var cmd = new MoveActivityCommand
			{
				AgentId = agent.Id.Value,
				Date = assignment.Date,
				ActivityId = activity.Id.Value,
				NewStartTime = TimeSpan.FromHours(1),
				OldStartTime = orgStartActivity,
				OldProjectionLayerLength = orgEndActivity - orgStartActivity
			};

			target.Handle(cmd);

			var projection = assignment.ProjectionService().CreateProjection();
			projection.Count().Should().Be.EqualTo(2);
			projection.First().Period.StartDateTime.Should().Be.EqualTo(cmd.Date.Date.Add(cmd.NewStartTime));
		}

		[Test]
		public void ShouldMoveLayerWhenItsTwoDifferentActivities()
		{
			var agent = new Person();
			agent.SetId(Guid.NewGuid());
			var activity = new Activity("theone");
			var activityNotBeMoved = new Activity("justanotherone");
			activity.SetId(Guid.NewGuid());
			activityNotBeMoved.SetId(Guid.NewGuid());
			var orgStartActivity = createDateTime(3);
			var orgEndActivity = createDateTime(8);
			var orgStartActivityNotBeMoved = createDateTime(5);
			var orgEndActivityNotBeMoved = createDateTime(10);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent,
				new DateTimePeriod(orgStartActivity, orgEndActivity));
			assignment.AddActivity(activityNotBeMoved, new DateTimePeriod(orgStartActivityNotBeMoved, orgEndActivityNotBeMoved));

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository { assignment };
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
			var personRepository = new FakeWriteSideRepository<IPerson> { agent };
			var target = new MoveActivityCommandHandler(personAssignmentRepository, personRepository, scenario);

			var cmd = new MoveActivityCommand
			{
				AgentId = agent.Id.Value,
				Date = assignment.Date,
				ActivityId = activity.Id.Value,
				NewStartTime = TimeSpan.FromHours(1),
				OldStartTime = orgStartActivity,
				OldProjectionLayerLength = orgEndActivity - orgStartActivity
			};

			target.Handle(cmd);

			var expectedStart = orgStartActivity.Date.Add(orgEndActivity - (orgStartActivity - cmd.NewStartTime));
			var projection = assignment.ProjectionService().CreateProjection();
			projection.Count().Should().Be.EqualTo(2);
			projection.First().Period.EndDateTime.Should().Be.EqualTo(expectedStart);
			projection.Last().Period.EndDateTime.Should().Be.EqualTo(orgEndActivityNotBeMoved);
		}

		[Test]
		public void ShouldMoveSingleLayer()
		{
			var agent = new Person();
			agent.SetId(Guid.NewGuid());
			var activity = new Activity("_");
			activity.SetId(Guid.NewGuid());
			var orgStart = createDateTime(6);
			var orgEnd = createDateTime(11);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, new DateTimePeriod(orgStart, orgEnd));

			var personAssignmentRepository = new FakePersonAssignmentWriteSideRepository { assignment };
			var scenario = new ThisCurrentScenario(personAssignmentRepository.Single().Scenario);
			var personRepository = new FakeWriteSideRepository<IPerson> { agent };
			var target = new MoveActivityCommandHandler(personAssignmentRepository, personRepository, scenario);

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