using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class MainShiftOptimizeActivitiesSpecificationTest
    {
        private Specification<IMainShift> _interface;
        private MainShiftOptimizeActivitiesSpecification _target;
        private OptimizerActivitiesPreferences _preferences;
        private IMainShift _originalMainShift;
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
            _originalMainShift = MainShiftFactory.CreateMainShiftWithLayers(_baseAct, _lunchAct, _shbrAct);
            DateOnly dateOnly = new DateOnly(2007, 1, 1);
            TimeZoneInfo timeZoneInfo = (TimeZoneInfo.Utc);
            _target = new MainShiftOptimizeActivitiesSpecification(_preferences, _originalMainShift, dateOnly, timeZoneInfo);
            _interface = new MainShiftOptimizeActivitiesSpecification(_preferences, _originalMainShift, dateOnly, timeZoneInfo);
        }

        [Test]
        public void VerifyIsSatisfiedBy()
        {
            IMainShift shift = _originalMainShift;
            Assert.IsTrue(_interface.IsSatisfiedBy(shift));
        }

        
        [Test]
        public void VerifyCorrectShiftCategory()
        {
            IMainShift shift = MainShiftFactory.CreateMainShiftWithLayers(_baseAct, _lunchAct, _shbrAct);
            Assert.IsTrue(_target.CorrectShiftCategory(shift));

            _preferences.KeepShiftCategory = true;
            Assert.IsFalse(_target.CorrectShiftCategory(shift));

            shift.ShiftCategory = _originalMainShift.ShiftCategory;
            Assert.IsTrue(_target.CorrectShiftCategory(shift));
        }

        [Test]
        public void VerifyCorrectStartTime()
        {
            IMainShift shift = MainShiftFactory.CreateMainShiftWithLayers(_baseAct, _lunchAct, _shbrAct);   
            shift.LayerCollection[0].ChangeLayerPeriodStart(TimeSpan.FromMinutes(1));
            IVisualLayerCollection layers = shift.ProjectionService().CreateProjection();
            Assert.IsTrue(_target.CorrectStart(layers));

            _preferences.KeepStartTime = true;
            Assert.IsFalse(_target.CorrectStart(layers));

            shift.LayerCollection[0].ChangeLayerPeriodStart(TimeSpan.FromMinutes(-1));
            layers = shift.ProjectionService().CreateProjection();
            Assert.IsTrue(_target.CorrectStart(layers));
        }

        [Test]
        public void VerifyCorrectEndTime()
        {
            IMainShift shift = MainShiftFactory.CreateMainShiftWithLayers(_baseAct, _lunchAct, _shbrAct);
            shift.LayerCollection[0].ChangeLayerPeriodEnd(TimeSpan.FromMinutes(1));
            IVisualLayerCollection layers = shift.ProjectionService().CreateProjection();
            Assert.IsTrue(_target.CorrectEnd(layers));

            _preferences.KeepEndTime = true;
            Assert.IsFalse(_target.CorrectEnd(layers));

            shift.LayerCollection[0].ChangeLayerPeriodEnd(TimeSpan.FromMinutes(-1));
            layers = shift.ProjectionService().CreateProjection();
            Assert.IsTrue(_target.CorrectEnd(layers));
        }

        [Test]
        public void VerifyAllowAlterBetween()
        {
            IMainShift shift = MainShiftFactory.CreateMainShiftWithLayers(_baseAct, _lunchAct, _shbrAct);
            shift.LayerCollection[3].MoveLayer(TimeSpan.FromMinutes(1));
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
            shift.LayerCollection[3].MoveLayer(TimeSpan.FromMinutes(-1));
            //lengthen instead of move
            shift.LayerCollection[3].ChangeLayerPeriodEnd(TimeSpan.FromMinutes(1));
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
            IMainShift shift = MainShiftFactory.CreateMainShiftWithLayers(_shbrAct, _lunchAct, _baseAct);
            IVisualLayerCollection layers = shift.ProjectionService().CreateProjection();
            Assert.IsTrue(_target.LockedActivityNotMoved(layers));

            _preferences.SetDoNotMoveActivities(new List<IActivity>{_lunchAct});
            Assert.IsTrue(_target.LockedActivityNotMoved(layers));

            _preferences.SetDoNotMoveActivities(new List<IActivity> { _lunchAct, _shbrAct });
            Assert.IsFalse(_target.LockedActivityNotMoved(layers));

            _preferences.SetDoNotMoveActivities(new List<IActivity> { _lunchAct, _baseAct });
            Assert.IsFalse(_target.LockedActivityNotMoved(layers));

        }
    }
}