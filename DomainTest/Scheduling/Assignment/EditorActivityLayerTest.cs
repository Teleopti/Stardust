using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;



namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class EditorActivityLayerTest
	{
		private EditableShiftLayer _target;
		private IActivity _activity;

		[SetUp]
		public void Setup()
		{
			_activity = new Activity("hej");
			_target = new EditableShiftLayer(_activity, new DateTimePeriod(2013, 05, 21, 2013, 05, 21));
		}

		[Test]
		public void ShouldExposeActivityAndPeriod()
		{
			Assert.AreSame(_activity, _target.Payload);
			Assert.AreEqual(new DateTimePeriod(2013, 05, 21, 2013, 05, 21), _target.Period);
		}

	}
}