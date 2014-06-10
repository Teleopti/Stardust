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

		private static DateTime createDateTime(int hourOnDay)
		{
			return new DateTime(2013, 11, 14, hourOnDay, 0, 0, 0, DateTimeKind.Utc);
		}
	}
}