using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;

using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class MainShiftOptimizeActivitiesSpecificationTest
    {
        private Specification<IEditableShift> _interface;
        private MainShiftOptimizeActivitiesSpecification _target;
        private OptimizerActivitiesPreferences _preferences;
        private IEditableShift _originalMainShift;
        private IActivity _baseAct;
        private IActivity _lunchAct;
        private IActivity _shbrAct;

        [SetUp]
        public void Setup()
        {
            _preferences = new OptimizerActivitiesPreferences();
            _baseAct = ActivityFactory.CreateActivity("Tel");
            _baseAct.InContractTime = true;
            _lunchAct = ActivityFactory.CreateActivity("Lunch");
            _lunchAct.InContractTime = false;
            _shbrAct = ActivityFactory.CreateActivity("ShBr");
            _shbrAct.InContractTime = true;
            _originalMainShift = EditableShiftFactory.CreateEditorShiftWithLayers(_baseAct, _lunchAct, _shbrAct);
            DateOnly dateOnly = new DateOnly(2007, 1, 1);
            _target = new MainShiftOptimizeActivitiesSpecification(new CorrectAlteredBetween(new UtcTimeZone()), _preferences, _originalMainShift, dateOnly);
            _interface = new MainShiftOptimizeActivitiesSpecification(new CorrectAlteredBetween(new UtcTimeZone()), _preferences, _originalMainShift, dateOnly);
		}

        [Test]
        public void VerifyIsSatisfiedBy()
        {
            var shift = _originalMainShift;
            Assert.IsTrue(_interface.IsSatisfiedBy(shift));
        }

        
        [Test]
        public void VerifyCorrectShiftCategory()
        {
            var shift = EditableShiftFactory.CreateEditorShiftWithLayers(_baseAct, _lunchAct, _shbrAct);
            Assert.IsTrue(_target.CorrectShiftCategory(shift));

            _preferences.KeepShiftCategory = true;
            Assert.IsFalse(_target.CorrectShiftCategory(shift));

            shift.ShiftCategory = _originalMainShift.ShiftCategory;
            Assert.IsTrue(_target.CorrectShiftCategory(shift));
        }

        
        [Test]
        public void VerifyAllowAlterBetween()
        {
            var shift = EditableShiftFactory.CreateEditorShiftWithLayers(_baseAct, _lunchAct, _shbrAct);
	        var layer = shift.LayerCollection[3];
	        shift.LayerCollection.Remove(layer);
			var newLayer = new EditableShiftLayer(layer.Payload, layer.Period.MovePeriod(TimeSpan.FromMinutes(1)));
            shift.LayerCollection.Add(newLayer);
            IVisualLayerCollection layers = shift.ProjectionService().CreateProjection();
            Assert.IsTrue(_target.CorrectAlteredBetween(layers));

            _preferences.AllowAlterBetween = new TimePeriod(TimeSpan.FromDays(-10), TimeSpan.FromDays(10));
            Assert.IsTrue(_target.CorrectAlteredBetween(layers));

            TimePeriod period =
                new TimePeriod(new TimeSpan(8, 0, 0),
                                   new TimeSpan(16, 5, 0));
            _preferences.AllowAlterBetween = period;
            Assert.IsFalse(_target.CorrectAlteredBetween(layers));

            //reset shift
			layer = shift.LayerCollection[3];
			shift.LayerCollection.Remove(layer);
			newLayer = new EditableShiftLayer(layer.Payload, layer.Period.MovePeriod(TimeSpan.FromMinutes(-1)));
			shift.LayerCollection.Add(newLayer);

            //lengthen instead of move
			layer = shift.LayerCollection[3];
			shift.LayerCollection.Remove(layer);
			newLayer = new EditableShiftLayer(layer.Payload, layer.Period.ChangeEndTime(TimeSpan.FromMinutes(1)));
			shift.LayerCollection.Add(newLayer);
            layers = shift.ProjectionService().CreateProjection();
            period =
                new TimePeriod(new TimeSpan(16, 10, 0),
                                   new TimeSpan(16, 20, 0));
            _preferences.AllowAlterBetween = period;
            Assert.IsTrue(_target.CorrectAlteredBetween(layers), "Should be OK to change the transition from one layer to another within period");
        }

        [Test]
        public void VerifyLockedActivityNotMoved()
        {
            //Create a mirrored shift, all except lunch is altered
            var projection = createProjectionWithLayers();
						Assert.IsTrue(_target.LockedActivityNotMoved(projection));

            _preferences.SetDoNotMoveActivities(new List<IActivity>{_lunchAct});
						Assert.IsTrue(_target.LockedActivityNotMoved(projection));

            _preferences.SetDoNotMoveActivities(new List<IActivity> { _lunchAct, _shbrAct });
						Assert.IsFalse(_target.LockedActivityNotMoved(projection));

            _preferences.SetDoNotMoveActivities(new List<IActivity> { _lunchAct, _baseAct });
						Assert.IsFalse(_target.LockedActivityNotMoved(projection));

        }

				private IVisualLayerCollection createProjectionWithLayers()
				{

					DateTimePeriod period1 =
							new DateTimePeriod(new DateTime(2007, 1, 1, 8, 0, 0, DateTimeKind.Utc),
																 new DateTime(2007, 1, 1, 18, 0, 0, DateTimeKind.Utc));

					DateTimePeriod period2 =
							new DateTimePeriod(new DateTime(2007, 1, 1, 11, 0, 0, DateTimeKind.Utc),
																 new DateTime(2007, 1, 1, 12, 0, 0, DateTimeKind.Utc));

					DateTimePeriod period3 =
							new DateTimePeriod(new DateTime(2007, 1, 1, 15, 0, 0, DateTimeKind.Utc),
																 new DateTime(2007, 1, 1, 15, 15, 0, DateTimeKind.Utc));

					DateTimePeriod period4 =
							new DateTimePeriod(new DateTime(2007, 1, 1, 16, 0, 0, DateTimeKind.Utc),
																 new DateTime(2007, 1, 1, 16, 15, 0, DateTimeKind.Utc));

					return new[]
						{
							new MainShiftLayer(_shbrAct, period1),
							new MainShiftLayer(_lunchAct, period2),
							new MainShiftLayer(_baseAct, period3),
							new MainShiftLayer(_baseAct, period4)
						}.CreateProjection();
				}

		[Test]
		public void LengthOfSelectedActivityAreNotAllowedToBeChangedIfUserSaySo()
		{
			_preferences.DoNotAlterLengthOfActivity = _lunchAct;

			IEditableShift shift = EditableShiftFactory.CreateEditorShiftWithLayers(_shbrAct, _lunchAct, _baseAct);
			IVisualLayerCollection layers = shift.ProjectionService().CreateProjection();
			Assert.IsTrue(_target.LengthOfActivityEqual(layers));

			shift.LayerCollection[1].Period.MovePeriod(TimeSpan.FromMinutes(1));
			layers = shift.ProjectionService().CreateProjection();
			Assert.IsTrue(_target.LengthOfActivityEqual(layers));

			//shift.LayerCollection[1].Period.ChangeEndTime(TimeSpan.FromMinutes(1));
			//layers = shift.ProjectionService().CreateProjection();
			//Assert.IsFalse(_target.LengthOfActivityEqual(layers));
		}

		[Test]
		public void
			LengthOfSelectedActivityAreNotAllowedToBeChangedIfUserSaySoAlsoMeansThatIfNewShiftHasLayerOfThisActivityAndOriginalHasNotWeShouldReturnFalseAndTheOtherWayAround
			()
		{
			var newActivity = ActivityFactory.CreateActivity("hej");
			_preferences.DoNotAlterLengthOfActivity = newActivity;

			IEditableShift shift = EditableShiftFactory.CreateEditorShiftWithLayers(_shbrAct, newActivity, _baseAct);
			IVisualLayerCollection layers = shift.ProjectionService().CreateProjection();
			Assert.IsFalse(_target.LengthOfActivityEqual(layers));

			_preferences.DoNotAlterLengthOfActivity = _lunchAct;
			Assert.IsFalse(_target.LengthOfActivityEqual(layers));
		}

		
    }
}