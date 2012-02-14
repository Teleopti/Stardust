using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restriction
{
    [TestFixture]
    public class RotationTest
    {
        private Rotation _rotation;
        
        [SetUp]
        public void Setup()
        {
            _rotation = new Rotation("olas rotation", 14);    
        }

        [Test]
        public void CanCreateNewRotation()
        {
            Assert.AreEqual("olas rotation", _rotation.Name);
            Assert.AreEqual(14,_rotation.RotationDays.Count);
            _rotation.Name = "Annat namn";
            Assert.AreEqual("Annat namn", _rotation.Name);
        }

        [Test]
        public void CanGetIndexOfRotationDay()
        {
            Assert.AreEqual(10, _rotation.RotationDays[10].Index);
        }

        [Test]
        public void CanGetFindCorrectRotationDay()
        {
            IRotationDay day = _rotation.FindRotationDay(14);
            Assert.AreEqual(0, day.Index);
            day = _rotation.FindRotationDay(20);
            Assert.AreEqual(6, day.Index);
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void NegativeDayProducesError()
        {
            _rotation.FindRotationDay(-1);
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CannotRemoveMoreDaysThanThereAre()
        {
            _rotation.RemoveDays(50);
        }

        [Test]
        public void CanAddMoreDayToRotation()
        {
            _rotation.AddDays(7);
            Assert.AreEqual(21, _rotation.RotationDays.Count);
        }
 
        [Test]
        public void CanRemoveDaysFromRotation()
        {
            _rotation.RemoveDays(7);
            Assert.AreEqual(7, _rotation.RotationDays.Count);
        }

        

        [Test]
        public void CanReadSignificantRestriction()
        {
            Assert.IsNotNull(_rotation.RotationDays[10].SignificantRestriction());
        }

        [Test]
        public void RotationDayRestrictionCollectionIsOne()
        {
            Assert.AreEqual(1, _rotation.RotationDays[10].RestrictionCollection.Count);
        }

        [Test]
        public void CanSetShiftCategoryOnRestriction()
        {
            IShiftCategory shiftCategory = new ShiftCategory("katt");
            Assert.IsNull(_rotation.RotationDays[10].SignificantRestriction().ShiftCategory);
            _rotation.RotationDays[10].SignificantRestriction().ShiftCategory = shiftCategory;
            Assert.AreEqual(_rotation.RotationDays[10].SignificantRestriction().ShiftCategory , shiftCategory);
        }

        [Test]
        public void CanSetDayOffOnRestriction()
        {
            IDayOffTemplate dayOffTemplate = new DayOffTemplate(new Description("simon template"));
            Assert.IsNull(_rotation.RotationDays[10].SignificantRestriction().ShiftCategory);
            _rotation.RotationDays[10].SignificantRestriction().DayOffTemplate = dayOffTemplate;
            Assert.AreEqual(_rotation.RotationDays[10].SignificantRestriction().DayOffTemplate, dayOffTemplate);
        }

        [Test]
        public void CanSetStartTimeLimitationOnRestriction()
        {
            StartTimeLimitation startTimeLimitation = new StartTimeLimitation(new TimeSpan(8,15,0), null);
            Assert.IsNull(_rotation.RotationDays[10].SignificantRestriction().StartTimeLimitation.StartTime);
            _rotation.RotationDays[10].SignificantRestriction().StartTimeLimitation = startTimeLimitation;
            Assert.AreEqual(_rotation.RotationDays[10].SignificantRestriction().StartTimeLimitation, startTimeLimitation);
        }

        [Test]
        public void CanSetEndTimeLimitationOnRestriction()
        {
            EndTimeLimitation endTimeLimitation = new EndTimeLimitation(null,new TimeSpan(18, 45, 0));
            Assert.IsNull(_rotation.RotationDays[10].SignificantRestriction().EndTimeLimitation.EndTime);
            _rotation.RotationDays[10].SignificantRestriction().EndTimeLimitation = endTimeLimitation;
            Assert.AreEqual(_rotation.RotationDays[10].SignificantRestriction().EndTimeLimitation, endTimeLimitation);
        }

        [Test]
        public void CanSetWorkTimeLimitationOnRestriction()
        {
            WorkTimeLimitation workTimeLimitation = new WorkTimeLimitation(null, new TimeSpan(4, 0, 0));
            Assert.IsNull(_rotation.RotationDays[10].SignificantRestriction().WorkTimeLimitation.EndTime);
            _rotation.RotationDays[10].SignificantRestriction().WorkTimeLimitation = workTimeLimitation;
            Assert.AreEqual(_rotation.RotationDays[10].SignificantRestriction().WorkTimeLimitation, workTimeLimitation);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Choosable"), Test]
        public void VerifyDeletedIsNotChoosable()
        {
            Assert.IsTrue(_rotation.IsChoosable);
            _rotation.SetDeleted();
            Assert.IsFalse(_rotation.IsChoosable);
        }

        [Test]
        public void VerifyCanCheckIsRotationDay()
        {
            IRotationDay day = _rotation.RotationDays[0];
            Assert.IsFalse(day.IsRotationDay());
            
            day.SignificantRestriction().ShiftCategory = new ShiftCategory("öskö");
            Assert.IsTrue(day.IsRotationDay());
            day.SignificantRestriction().ShiftCategory = null;
            Assert.IsFalse(day.IsRotationDay());

            day.SignificantRestriction().DayOffTemplate = new DayOffTemplate(new Description("öa"));
            Assert.IsTrue(day.IsRotationDay());
            day.SignificantRestriction().DayOffTemplate = null;
            Assert.IsFalse(day.IsRotationDay());

            day.SignificantRestriction().StartTimeLimitation = new StartTimeLimitation(new TimeSpan(6,0,0),null); 
            Assert.IsTrue(day.IsRotationDay());
            day.SignificantRestriction().StartTimeLimitation = new StartTimeLimitation(null, null); ;
            Assert.IsFalse(day.IsRotationDay());

            day.SignificantRestriction().StartTimeLimitation = new StartTimeLimitation( null,new TimeSpan(9, 0, 0));
            Assert.IsTrue(day.IsRotationDay());
            day.SignificantRestriction().StartTimeLimitation = new StartTimeLimitation(null, null); ;
            Assert.IsFalse(day.IsRotationDay());

            day.SignificantRestriction().EndTimeLimitation = new EndTimeLimitation(new TimeSpan(17, 0, 0), null);
            Assert.IsTrue(day.IsRotationDay());
            day.SignificantRestriction().EndTimeLimitation = new EndTimeLimitation(null, null); ;
            Assert.IsFalse(day.IsRotationDay());

            day.SignificantRestriction().EndTimeLimitation = new EndTimeLimitation(null, new TimeSpan(19, 0, 0));
            Assert.IsTrue(day.IsRotationDay());
            day.SignificantRestriction().EndTimeLimitation = new EndTimeLimitation(null, null); ;
            Assert.IsFalse(day.IsRotationDay());

            day.SignificantRestriction().WorkTimeLimitation = new WorkTimeLimitation(new TimeSpan(17, 0, 0), null);
            Assert.IsTrue(day.IsRotationDay());
            day.SignificantRestriction().WorkTimeLimitation = new WorkTimeLimitation(null, null); ;
            Assert.IsFalse(day.IsRotationDay());

            day.SignificantRestriction().WorkTimeLimitation = new WorkTimeLimitation(null, new TimeSpan(19, 0, 0));
            Assert.IsTrue(day.IsRotationDay());
            day.SignificantRestriction().WorkTimeLimitation = new WorkTimeLimitation(null, null); ;
            Assert.IsFalse(day.IsRotationDay());
        }
    }

}
