using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class EditorShiftTest
	{
		private MockRepository _mocks;
		private IEditorShift _target;
		private IShiftCategory _shiftCategory;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_shiftCategory = new ShiftCategory("cat");
			_target = new EditorShift(_shiftCategory);
		}

		[Test]
		public void ShouldBeCreatedWithShiftCategory()
		{
			Assert.AreSame(_shiftCategory, _target.ShiftCategory);
		}

		[Test]
		public void ShouldBeAbleToAddLayer()
		{
			ILayer<IActivity> activityLayer = new EditorActivityLayer(new Activity("phone"), new DateTimePeriod());
			_target.LayerCollection.Add(activityLayer);
			Assert.AreEqual(1, _target.LayerCollection.Count);
		}

		[Test]
		public void LayerShouldHaveOrderIndex()
		{
			ILayer<IActivity> activityLayer1 = new EditorActivityLayer(new Activity("phone"), new DateTimePeriod());
			ILayer<IActivity> activityLayer2 = new EditorActivityLayer(new Activity("phone"), new DateTimePeriod());
			_target.LayerCollection.Add(activityLayer1);
			_target.LayerCollection.Add(activityLayer2);
			Assert.AreEqual(0, _target.LayerCollection[0].OrderIndex);
			Assert.AreEqual(1, _target.LayerCollection[1].OrderIndex);
		}

	}
}