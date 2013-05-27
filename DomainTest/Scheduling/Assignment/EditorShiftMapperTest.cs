using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;


namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class EditorShiftMapperTest
	{
		//private MockRepository _mocks;
		private IEditorShiftMapper _target;
		private IPersonAssignment _personAssignment;

		[SetUp]
		public void Setup()
		{
			//_mocks = new MockRepository();
			_target = new EditorShiftMapper();
			_personAssignment = new PersonAssignment(new Person(), new Scenario("hej"), new DateOnly(2000, 1, 1));
		}

		[Test]
		public void ShouldReturnNullIfNoMainShiftLayers()
		{
			Assert.IsNull(_target.CreateEditorShift(_personAssignment));
		}

		[Test]
		public void ShouldMapShiftCategory()
		{
			var mainShift = MainShiftFactory.CreateMainShiftWithThreeActivityLayers();
			_personAssignment.SetMainShift(mainShift);
			var editorShift = _target.CreateEditorShift(_personAssignment);
			Assert.AreSame(_personAssignment.ShiftCategory, editorShift.ShiftCategory);
		}

		[Test]
		public void ShouldMapLayers()
		{
			var mainShift = MainShiftFactory.CreateMainShiftWithThreeActivityLayers();
			_personAssignment.SetMainShift(mainShift);
			var editorShift = _target.CreateEditorShift(_personAssignment);
			Assert.AreEqual(3, editorShift.LayerCollection.Count);
			var assignedLayers = _personAssignment.MainShiftActivityLayers.ToList();
			Assert.AreSame(assignedLayers[0].Payload, editorShift.LayerCollection[0].Payload);
			Assert.AreSame(assignedLayers[1].Payload, editorShift.LayerCollection[1].Payload);
			Assert.AreSame(assignedLayers[2].Payload, editorShift.LayerCollection[2].Payload);
			Assert.AreEqual(assignedLayers[0].Period, editorShift.LayerCollection[0].Period);
			Assert.AreEqual(assignedLayers[1].Period, editorShift.LayerCollection[1].Period);
			Assert.AreEqual(assignedLayers[2].Period, editorShift.LayerCollection[2].Period);
		}

		[Test]
		public void ShouldSetMainShiftActivityLayersAndShiftCategory()
		{
			var shiftCategory = new ShiftCategory("hej");
			var activity = new Activity("hopp");
			var editorShift = new EditableShift(shiftCategory);
			var editorActivityLayer = new EditorActivityLayer(activity, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
			editorShift.LayerCollection.Add(editorActivityLayer);
			_target.SetMainShiftLayers(_personAssignment, editorShift);
			Assert.AreEqual(1, _personAssignment.MainShiftActivityLayers.Count());
			var firstMainShiftLayer = _personAssignment.MainShiftActivityLayers.First();
			Assert.AreSame(activity, firstMainShiftLayer.Payload);
			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2000, 1, 2), firstMainShiftLayer.Period);
			Assert.AreSame(shiftCategory, _personAssignment.ShiftCategory);
		}
	}
}