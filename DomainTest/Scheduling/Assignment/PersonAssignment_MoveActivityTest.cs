using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
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
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, new DateTimePeriod(layerFirstStart, layerFirstEnd));
			assignment.AddActivity(activityNotBeMoved, new DateTimePeriod(layerSecondStart, layerSecondEnd));

			assignment.MoveActivityAndSetHighestPriority(activity, layerFirstStart, TimeSpan.FromHours(4), TimeSpan.FromHours(1));

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
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, new DateTimePeriod(layerStart, layerEnd));
			assignment.AddActivity(activity, new DateTimePeriod(layerStart, layerEnd));

			assignment.MoveActivityAndSetHighestPriority(activity, layerStart, TimeSpan.FromHours(4), layerEnd-layerStart);

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
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, new DateTimePeriod(layerFirstStart, layerFirstEnd));
			assignment.AddActivity(activityNotBeMoved, new DateTimePeriod(layerSecondStart, layerSecondEnd));

			assignment.MoveActivityAndSetHighestPriority(activity, layerSecondEnd, TimeSpan.FromHours(6), TimeSpan.FromHours(1));

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
			Assert.Throws<ArgumentException>(() => personassignment.MoveActivityAndSetHighestPriority(new Activity("_"), DateTime.Now, TimeSpan.FromHours(1), TimeSpan.FromHours(2)));
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
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent,new DateTimePeriod(orgStartActivity, orgEndActivity));
			assignment.AddActivity(activityNotBeMoved, new DateTimePeriod(orgStartActivityNotBeMoved, orgEndActivityNotBeMoved));

			assignment.MoveActivityAndSetHighestPriority(activity, orgStartActivity, TimeSpan.FromHours(4), TimeSpan.FromHours(2));

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
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent,
				new DateTimePeriod(orgStartActivity, orgEndActivity));
			assignment.AddActivity(activity, new DateTimePeriod(orgStartActivityNotBeMoved, orgEndActivityNotBeMoved));
			var newStartTime = TimeSpan.FromHours(1);
		
			assignment.MoveActivityAndSetHighestPriority(activity, orgStartActivity, newStartTime, TimeSpan.FromHours(3));

			var projection = assignment.ProjectionService().CreateProjection();
			projection.Count().Should().Be.EqualTo(2);
			projection.First().Period.StartDateTime.TimeOfDay.Should().Be.EqualTo(newStartTime);
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
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent,
				new DateTimePeriod(orgStartActivity, orgEndActivity));
			assignment.AddActivity(activityNotBeMoved, new DateTimePeriod(orgStartActivityNotBeMoved, orgEndActivityNotBeMoved));

			var newStartTime = TimeSpan.FromHours(1);

			assignment.MoveActivityAndSetHighestPriority(activity, orgStartActivity, newStartTime, orgEndActivity - orgStartActivity);

			var expectedStart = orgStartActivity.Date.Add(orgEndActivity - (orgStartActivity - newStartTime));
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
			var assignment = PersonAssignmentFactory.CreateAssignmentWithMainShift(activity, agent, new DateTimePeriod(layer1start, layer1end));
			assignment.AddActivity(activityNotBeMoved, new DateTimePeriod(layer2start, layer2end));
			assignment.AddActivity(activity, new DateTimePeriod(layer3start, layer3end));

			assignment.MoveActivityAndSetHighestPriority(activity, layer3start, TimeSpan.FromHours(6), layer3end - layer3start);
			var projection = assignment.ProjectionService().CreateProjection();
			projection.Count().Should().Be.EqualTo(1);
			projection.First().Period.StartDateTime.Should().Be.EqualTo(createDateTime(6));
			projection.First().Period.EndDateTime.Should().Be.EqualTo(createDateTime(10));
		}

		private static DateTime createDateTime(int hourOnDay)
		{
			return new DateTime(2013, 11, 14, hourOnDay, 0, 0, 0, DateTimeKind.Utc);
		}
	}
}