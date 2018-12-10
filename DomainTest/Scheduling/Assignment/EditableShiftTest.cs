using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;



namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class EditableShiftTest
	{
		private IEditableShift _target;
		private IShiftCategory _shiftCategory;

		[SetUp]
		public void Setup()
		{
			_shiftCategory = new ShiftCategory("cat");
			_target = new EditableShift(_shiftCategory);
		}

		[Test]
		public void ShouldBeCreatedWithShiftCategory()
		{
			Assert.AreSame(_shiftCategory, _target.ShiftCategory);
		}

		[Test]
		public void ShouldBeAbleToAddLayer()
		{
			var activityLayer = new EditableShiftLayer(new Activity("phone"), new DateTimePeriod());
			_target.LayerCollection.Add(activityLayer);
			Assert.AreEqual(1, _target.LayerCollection.Count);
		}

		[Test]
		public void ShouldMoveToDestinationDate()
		{
			var startTime = new DateTime(2014, 07, 03, 23, 0, 0, DateTimeKind.Utc);
			var endTime = new DateTime(2014, 07, 04, 02, 0, 0, DateTimeKind.Utc);
			var activityLayer = new EditableShiftLayer(new Activity("phone"), new DateTimePeriod(startTime, endTime));
			_target.LayerCollection.Add(activityLayer);
			var movedShift = _target.MoveTo(new DateOnly(2014, 07, 03), new DateOnly(2014, 06, 30));
			startTime = new DateTime(2014, 06, 30, 23, 0, 0, DateTimeKind.Utc);
			endTime = new DateTime(2014, 07, 01, 02, 0, 0, DateTimeKind.Utc);
			Assert.AreEqual(new DateTimePeriod(startTime, endTime), movedShift.ProjectionService().CreateProjection().Period());
		}

	}
}