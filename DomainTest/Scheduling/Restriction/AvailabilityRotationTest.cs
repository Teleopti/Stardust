using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Restriction
{
    [TestFixture]
    public class AvailabilityRotationTest
    {
        private AvailabilityRotation _availabilityRotation;
        
        [SetUp]
        public void Setup()
        {
            _availabilityRotation = new AvailabilityRotation("en availabilityrotation", 14);    
        }

        [Test]
        public void CanCreateNewRotation()
        {
            Assert.AreEqual("en availabilityrotation", _availabilityRotation.Name);
            Assert.AreEqual(14,_availabilityRotation.AvailabilityDays.Count);
            _availabilityRotation.Name = "Annat namn";
            Assert.AreEqual("Annat namn", _availabilityRotation.Name);
        }

        [Test]
        public void CanGetIndexOfRotationDay()
        {
            Assert.AreEqual(10, _availabilityRotation.AvailabilityDays[10].Index);
        }

        [Test]
        public void CanGetFindCorrectRotationDay()
        {
            IAvailabilityDay day = _availabilityRotation.FindAvailabilityDay(14);
            Assert.AreEqual(0, day.Index);
            day = _availabilityRotation.FindAvailabilityDay(20);
            Assert.AreEqual(6, day.Index);
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void NegativeDayProducesError()
        {
            _availabilityRotation.FindAvailabilityDay(-1);
        }

        [Test]
        public void CanAddMoreDayToRotation()
        {
            _availabilityRotation.AddDays(7);
            Assert.AreEqual(21, _availabilityRotation.AvailabilityDays.Count);
        }
 
        [Test]
        public void CanRemoveDaysFromRotation()
        {
            _availabilityRotation.RemoveDays(7);
            Assert.AreEqual(7, _availabilityRotation.AvailabilityDays.Count);
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void CannotRemoveMoreDaysThanThereAre()
        {
            _availabilityRotation.RemoveDays(15);
        }

        [Test]
        public void CanReadRestriction()
        {
            Assert.IsNotNull(_availabilityRotation.AvailabilityDays[10].Restriction);
        }

        

        [Test]
        public void CanSetAvailableOnRestriction()
        {
            Assert.IsFalse(_availabilityRotation.AvailabilityDays[10].Restriction.NotAvailable);
            _availabilityRotation.AvailabilityDays[10].Restriction.NotAvailable = true;
            Assert.IsTrue(_availabilityRotation.AvailabilityDays[10].Restriction.NotAvailable);
        }

        [Test]
        public void CanSetStartTimeLimitationOnRestriction()
        {
            StartTimeLimitation startTimeLimitation = new StartTimeLimitation(new TimeSpan(8,15,0), null);
            Assert.IsNull(_availabilityRotation.AvailabilityDays[10].Restriction.StartTimeLimitation.StartTime);
            _availabilityRotation.AvailabilityDays[10].Restriction.StartTimeLimitation = startTimeLimitation;
            Assert.AreEqual(_availabilityRotation.AvailabilityDays[10].Restriction.StartTimeLimitation, startTimeLimitation);
        }

        [Test]
        public void CanSetEndTimeLimitationOnRestriction()
        {
            EndTimeLimitation endTimeLimitation = new EndTimeLimitation(null,new TimeSpan(18, 45, 0));
            Assert.IsNull(_availabilityRotation.AvailabilityDays[10].Restriction.EndTimeLimitation.EndTime);
            _availabilityRotation.AvailabilityDays[10].Restriction.EndTimeLimitation = endTimeLimitation;
            Assert.AreEqual(_availabilityRotation.AvailabilityDays[10].Restriction.EndTimeLimitation, endTimeLimitation);
        }

        [Test]
        public void CanSetWorkTimeLimitationOnRestriction()
        {
            WorkTimeLimitation workTimeLimitation = new WorkTimeLimitation(null, new TimeSpan(4, 0, 0));
            Assert.IsNull(_availabilityRotation.AvailabilityDays[10].Restriction.WorkTimeLimitation.EndTime);
            _availabilityRotation.AvailabilityDays[10].Restriction.WorkTimeLimitation = workTimeLimitation;
            Assert.AreEqual(_availabilityRotation.AvailabilityDays[10].Restriction.WorkTimeLimitation, workTimeLimitation);
        }
        
        [Test]
        public void CanSetRestrictionOnDay()
        {
            IAvailabilityRestriction restriction = new AvailabilityRestriction();
            _availabilityRotation.AvailabilityDays[10].Restriction = restriction;
            Assert.AreSame(_availabilityRotation.AvailabilityDays[10].Restriction , restriction);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Choosable"), Test]
        public void VerifyDeletedIsNotChoosable()
        {
            Assert.IsTrue(_availabilityRotation.IsChoosable);
            _availabilityRotation.SetDeleted();
            Assert.IsFalse(_availabilityRotation.IsChoosable);
        }

        [Test]
        public void VerifyCanCheckIsAvailabilityDay()
        {
            IAvailabilityDay day = _availabilityRotation.AvailabilityDays[0];
            Assert.IsFalse(day.IsAvailabilityDay());

            day.Restriction.NotAvailable = true;
            Assert.IsTrue(day.IsAvailabilityDay());
            day.Restriction.NotAvailable = false;
            Assert.IsFalse(day.IsAvailabilityDay());



            day.Restriction.StartTimeLimitation = new StartTimeLimitation(new TimeSpan(6, 0, 0), null);
            Assert.IsTrue(day.IsAvailabilityDay());
            day.Restriction.StartTimeLimitation = new StartTimeLimitation(null, null); ;
            Assert.IsFalse(day.IsAvailabilityDay());

            day.Restriction.StartTimeLimitation = new StartTimeLimitation(null, new TimeSpan(9, 0, 0));
            Assert.IsTrue(day.IsAvailabilityDay());
            day.Restriction.StartTimeLimitation = new StartTimeLimitation(null, null); ;
            Assert.IsFalse(day.IsAvailabilityDay());

            day.Restriction.EndTimeLimitation = new EndTimeLimitation(new TimeSpan(17, 0, 0), null);
            Assert.IsTrue(day.IsAvailabilityDay());
            day.Restriction.EndTimeLimitation = new EndTimeLimitation(null, null); ;
            Assert.IsFalse(day.IsAvailabilityDay());

            day.Restriction.EndTimeLimitation = new EndTimeLimitation(null, new TimeSpan(19, 0, 0));
            Assert.IsTrue(day.IsAvailabilityDay());
            day.Restriction.EndTimeLimitation = new EndTimeLimitation(null, null); ;
            Assert.IsFalse(day.IsAvailabilityDay());

            day.Restriction.WorkTimeLimitation = new WorkTimeLimitation(new TimeSpan(17, 0, 0), null);
            Assert.IsTrue(day.IsAvailabilityDay());
            day.Restriction.WorkTimeLimitation = new WorkTimeLimitation(null, null); ;
            Assert.IsFalse(day.IsAvailabilityDay());

            day.Restriction.WorkTimeLimitation = new WorkTimeLimitation(null, new TimeSpan(19, 0, 0));
            Assert.IsTrue(day.IsAvailabilityDay());
            day.Restriction.WorkTimeLimitation = new WorkTimeLimitation(null, null); ;
            Assert.IsFalse(day.IsAvailabilityDay());
        }
    }

}
