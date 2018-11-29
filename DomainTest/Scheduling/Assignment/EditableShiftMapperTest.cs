using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;



namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class EditableShiftMapperTest
	{
		private IEditableShiftMapper _target;
		private IPersonAssignment _personAssignment;

		[SetUp]
		public void Setup()
		{
			_target = new EditableShiftMapper();
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
			var mainShift = EditableShiftFactory.CreateEditorShiftWithThreeActivityLayers();
			_target.SetMainShiftLayers(_personAssignment, mainShift);
			var editorShift = _target.CreateEditorShift(_personAssignment);
			Assert.AreSame(_personAssignment.ShiftCategory, editorShift.ShiftCategory);
		}

		[Test]
		public void ShouldMapLayers()
		{
			var mainShift = EditableShiftFactory.CreateEditorShiftWithThreeActivityLayers();
			_target.SetMainShiftLayers(_personAssignment, mainShift);
			var editorShift = _target.CreateEditorShift(_personAssignment);
			Assert.AreEqual(3, editorShift.LayerCollection.Count);
			var assignedLayers = _personAssignment.MainActivities().ToList();
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
			var editorActivityLayer = new EditableShiftLayer(activity, new DateTimePeriod(2000, 1, 1, 2000, 1, 2));
			editorShift.LayerCollection.Add(editorActivityLayer);
			_target.SetMainShiftLayers(_personAssignment, editorShift);
			Assert.AreEqual(1, _personAssignment.MainActivities().Count());
			var firstMainShiftLayer = _personAssignment.MainActivities().First();
			Assert.AreSame(activity, firstMainShiftLayer.Payload);
			Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2000, 1, 2), firstMainShiftLayer.Period);
			Assert.AreSame(shiftCategory, _personAssignment.ShiftCategory);
		}
	}
}