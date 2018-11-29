using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Collection
{
    [TestFixture]
    public class IdenticalLayersSpecificationTest
    {
        private IdenticalLayersSpecification _target;
        private IEditableShift _mainShift;
        private IActivity _baseAct;
        private IActivity _lunchAct;
        private IActivity _shbrAct;

        [SetUp]
        public void Setup()
        {
            _baseAct = ActivityFactory.CreateActivity("Tel");
            _lunchAct = ActivityFactory.CreateActivity("Lunch");
            _shbrAct = ActivityFactory.CreateActivity("ShBr");
            _mainShift = EditableShiftFactory.CreateEditorShiftWithLayers(_baseAct, _lunchAct, _shbrAct);
            IVisualLayerCollection originalLayer = _mainShift.ProjectionService().CreateProjection();
            _target = new IdenticalLayersSpecification(originalLayer);
        }

        [Test]
        public void VerifyIsSatisfiedByPeriod()
        {
            var shiftToCheck = _mainShift.MakeCopy();
            Assert.IsTrue(_target.IsSatisfiedBy(shiftToCheck.ProjectionService().CreateProjection()));

	        var oldLayer = shiftToCheck.LayerCollection[3];
	        var newLayer = new EditableShiftLayer(oldLayer.Payload, oldLayer.Period.MovePeriod(TimeSpan.FromMinutes(1)));
	        shiftToCheck.LayerCollection.Remove(oldLayer);
			shiftToCheck.LayerCollection.Add(newLayer);
            Assert.IsFalse(_target.IsSatisfiedBy(shiftToCheck.ProjectionService().CreateProjection()));

        }

        [Test]
        public void VerifyIsSatisfiedByPayload()
        {
            IActivity act1 = ActivityFactory.CreateActivity("hepp");
            IActivity act2 = ActivityFactory.CreateActivity("hopp");
            DateTimePeriod period =
                new DateTimePeriod(new DateTime(2007, 1, 1, 8, 0, 0, DateTimeKind.Utc),
                                   new DateTime(2007, 1, 1, 16, 5, 0, DateTimeKind.Utc));
            IShiftCategory category = ShiftCategoryFactory.CreateShiftCategory("hupp");

            _mainShift = EditableShiftFactory.CreateEditorShift(act1, period, category);
			var shiftToCheck = EditableShiftFactory.CreateEditorShift(act2, period, category);

            _target = new IdenticalLayersSpecification(_mainShift.ProjectionService().CreateProjection());

            Assert.IsFalse(_target.IsSatisfiedBy(shiftToCheck.ProjectionService().CreateProjection()));
        }
    }
}