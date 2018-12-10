using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestWithStaticDependenciesDONOTUSE]
	public class PersonAssignment_MoveActivityTest
	{
		[Test]
		public void ShouldNotMoveTheOtherProjectedLayerWhenMovingLeftPart()
		{
			var agent = new Person().WithId();
			var activity = new Activity("theone").WithId();
			var activityNotBeMoved = new Activity("justanotherone").WithId();
			var layerFirstStart = createDateTime(3);
			var layerFirstEnd = createDateTime(8);
			var layerSecondStart = createDateTime(4);
			var layerSecondEnd = createDateTime(7);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, activity, new DateTimePeriod(layerFirstStart, layerFirstEnd));
			assignment.AddActivity(activityNotBeMoved, new DateTimePeriod(layerSecondStart, layerSecondEnd));

			assignment.MoveActivityAndSetHighestPriority(activity, layerFirstStart,createDateTime(4), TimeSpan.FromHours(1), null);

			var projection = assignment.ProjectionService().CreateProjection();
			projection.Count().Should().Be.EqualTo(3);

			var lastLayerPeriod = projection.Last().Period;
			lastLayerPeriod.StartDateTime.Should().Be.EqualTo(layerSecondEnd);
			lastLayerPeriod.EndDateTime.TimeOfDay.Should().Be.EqualTo(TimeSpan.FromHours(8));
		}

		[Test]
		public void ShouldMoveLayerIfDuplicate()
		{
			var agent = new Person().WithId();
			var activity = new Activity("theone").WithId();
			var layerStart = createDateTime(3);
			var layerEnd = createDateTime(8);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, activity, new DateTimePeriod(layerStart, layerEnd));
			assignment.AddActivity(activity, new DateTimePeriod(layerStart, layerEnd));

			assignment.MoveActivityAndSetHighestPriority(activity, layerStart, createDateTime(4), layerEnd - layerStart, null);

			var projection = assignment.ProjectionService().CreateProjection();
			projection.Single().Period.StartDateTime.Should().Be.EqualTo(createDateTime(4));
			projection.Single().Period.EndDateTime.Should().Be.EqualTo(createDateTime(9));
		}

		[Test]
		public void ShouldNotMoveTheOtherProjectedLayerWhenMovingRightPart()
		{
			var agent = new Person().WithId();
			var activity = new Activity("theone").WithId();
			var activityNotBeMoved = new Activity("justanotherone").WithId();
			var layerFirstStart = createDateTime(3);
			var layerFirstEnd = createDateTime(8);
			var layerSecondStart = createDateTime(4);
			var layerSecondEnd = createDateTime(7);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, activity, new DateTimePeriod(layerFirstStart, layerFirstEnd));
			assignment.AddActivity(activityNotBeMoved, new DateTimePeriod(layerSecondStart, layerSecondEnd));

			assignment.MoveActivityAndSetHighestPriority(activity, layerSecondEnd, createDateTime(6), TimeSpan.FromHours(1), null);

			var projection = assignment.ProjectionService().CreateProjection();
			projection.Count().Should().Be.EqualTo(3);

			var firstLayerPeriod = projection.First().Period;
			firstLayerPeriod.StartDateTime.Should().Be.EqualTo(layerFirstStart);
			firstLayerPeriod.EndDateTime.TimeOfDay.Should().Be.EqualTo(TimeSpan.FromHours(4));
		}

		[Test]
		public void ShouldThrowExceptionIfLayerIsNotFound()
		{
			var personassignment = PersonAssignmentFactory.CreateAssignmentWithOvertimePersonalAndMainshiftLayers();
			Assert.Throws<ArgumentException>(() => personassignment.MoveActivityAndSetHighestPriority(new Activity("_"), DateTime.Now, createDateTime(1), TimeSpan.FromHours(2), null));
		}

		[Test]
		public void ShouldMoveAndResizeALayerIfLayerAndProjectionHaveDifferentDuration()
		{
			var agent = new Person().WithId();
			var activity = new Activity("theone").WithId();
			var activityNotBeMoved = new Activity("justanotherone").WithId();
			var orgStartActivity = createDateTime(2);
			var orgEndActivity = createDateTime(5);
			var orgStartActivityNotBeMoved = createDateTime(4);
			var orgEndActivityNotBeMoved = createDateTime(7);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent,activity, new DateTimePeriod(orgStartActivity, orgEndActivity));
			assignment.AddActivity(activityNotBeMoved, new DateTimePeriod(orgStartActivityNotBeMoved, orgEndActivityNotBeMoved));

			assignment.MoveActivityAndSetHighestPriority(activity, orgStartActivity, createDateTime(4), TimeSpan.FromHours(2), null);

			var projection = assignment.ProjectionService().CreateProjection();
			projection.Count().Should().Be.EqualTo(2);
			projection.First().Period.StartDateTime.TimeOfDay.Should().Be.EqualTo(TimeSpan.FromHours(4));
			projection.First().Period.ElapsedTime().Should().Be.EqualTo(orgStartActivityNotBeMoved.Subtract(orgStartActivity));
		}

		[Test]
		public void ShouldMoveASpecificLayerIfTwoLayersWithSameActivityExist()
		{
			var agent = new Person().WithId();
			var activity = new Activity("theone").WithId();
			var orgStartActivity = createDateTime(2);
			var orgEndActivity = createDateTime(5);
			var orgStartActivityNotBeMoved = createDateTime(11);
			var orgEndActivityNotBeMoved = createDateTime(15);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent,
				activity, new DateTimePeriod(orgStartActivity, orgEndActivity));
			assignment.AddActivity(activity, new DateTimePeriod(orgStartActivityNotBeMoved, orgEndActivityNotBeMoved));
			var newStartTime = createDateTime(1);

			assignment.MoveActivityAndSetHighestPriority(activity, orgStartActivity, newStartTime, TimeSpan.FromHours(3), null);

			var projection = assignment.ProjectionService().CreateProjection();
			projection.Count().Should().Be.EqualTo(2);
			projection.First().Period.StartDateTime.Should().Be.EqualTo(newStartTime);
		}


		[Test]
		public void ShouldMoveLayerWhenItsTwoDifferentActivities()
		{
			var agent = new Person().WithId();
			var activity = new Activity("theone").WithId();
			var activityNotBeMoved = new Activity("justanotherone").WithId();
			var orgStartActivity = createDateTime(3);
			var orgEndActivity = createDateTime(8);
			var orgStartActivityNotBeMoved = createDateTime(5);
			var orgEndActivityNotBeMoved = createDateTime(10);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent,
				activity, new DateTimePeriod(orgStartActivity, orgEndActivity));
			assignment.AddActivity(activityNotBeMoved, new DateTimePeriod(orgStartActivityNotBeMoved, orgEndActivityNotBeMoved));

			var newStartTime = createDateTime(1);

			assignment.MoveActivityAndSetHighestPriority(activity, orgStartActivity, newStartTime, orgEndActivity - orgStartActivity, null);

			var expectedStart = orgEndActivity.Subtract(orgStartActivity.Subtract(newStartTime));
			var projection = assignment.ProjectionService().CreateProjection();
			projection.Count().Should().Be.EqualTo(2);
			projection.First().Period.EndDateTime.Should().Be.EqualTo(expectedStart);
			projection.Last().Period.EndDateTime.Should().Be.EqualTo(orgEndActivityNotBeMoved);
		}

		[Test]
		public void ShouldRemovePartsOfLayersOfSameActivityStartingAfterMainLayersStartTime()
		{
			var agent = new Person().WithId();
			var activity = new Activity("theone").WithId();
			var activityNotBeMoved = new Activity("justanotherone").WithId();
			var layer1start = createDateTime(4);
			var layer1end = createDateTime(8);
			var layer2start = createDateTime(6);
			var layer2end = createDateTime(7);
			var layer3start = createDateTime(3);
			var layer3end = createDateTime(7);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, activity, new DateTimePeriod(layer1start, layer1end));
			assignment.AddActivity(activityNotBeMoved, new DateTimePeriod(layer2start, layer2end));
			assignment.AddActivity(activity, new DateTimePeriod(layer3start, layer3end));

			assignment.MoveActivityAndSetHighestPriority(activity, layer3start, createDateTime(6), layer3end - layer3start, null);
			var projection = assignment.ProjectionService().CreateProjection();
			projection.Count().Should().Be.EqualTo(1);
			projection.First().Period.StartDateTime.Should().Be.EqualTo(createDateTime(6));
			projection.First().Period.EndDateTime.Should().Be.EqualTo(createDateTime(10));
		}

		[Test]
		public void ShouldRaiseActivityMovedEvent()
		{
			var activity = new Activity("theone").WithId();
			var agent = new Person().WithId();
			var layerPeriod = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, activity, layerPeriod);

			var operatedPersonId = Guid.NewGuid();
			var trackId = Guid.NewGuid();
			assignment.MoveActivityAndSetHighestPriority(activity, layerPeriod.StartDateTime, createDateTime(1), layerPeriod.ElapsedTime(), new TrackedCommandInfo
			{
				OperatedPersonId = operatedPersonId,
				TrackId = trackId
			}); 

			var affectedPeriod = layerPeriod.MaximumPeriod(assignment.Period);
			var theEvent = assignment.PopAllEvents().OfType<ActivityMovedEvent>().Single();
			theEvent.PersonId.Should().Be(agent.Id.Value);
			theEvent.StartDateTime.Should().Be(affectedPeriod.StartDateTime);
			theEvent.EndDateTime.Should().Be(affectedPeriod.EndDateTime);
			theEvent.ScenarioId.Should().Be(assignment.Scenario.Id.Value);
			theEvent.InitiatorId.Should().Be(operatedPersonId);
			theEvent.CommandId.Should().Be(trackId);
		}

		[Test]
		public void ShouldOnlyRaiseActivityMovedEvent()
		{
			var activity = new Activity("theone").WithId();
			var agent = new Person().WithId();
			var layerPeriod = new DateTimePeriod(2000, 1, 1, 2000, 1, 2);
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(agent, activity, layerPeriod);

			assignment.PopAllEvents();

			assignment.MoveActivityAndSetHighestPriority(activity, layerPeriod.StartDateTime, createDateTime(1), layerPeriod.ElapsedTime(), null);

			assignment.PopAllEvents().Count()
				.Should().Be.EqualTo(1);
		}

		private static DateTime createDateTime(int hourOnDay)
		{
			return new DateTime(2013, 11, 14, hourOnDay, 0, 0, 0, DateTimeKind.Utc);
		}
	}

}