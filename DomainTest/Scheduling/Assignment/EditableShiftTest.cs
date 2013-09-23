using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;


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
		public void CloneShouldWork()
		{
			
		}

	}
}