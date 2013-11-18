using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.AgentInfo
{
    [TestFixture]
    public class PersonAvailabilityTest
    {
        private PersonAvailability _target;
        private IPerson _person;
        private AvailabilityRotation _availability;
        private DateOnly _startDate;

        [SetUp]
        public void Setup()
        {
            _availability = new AvailabilityRotation("My avilability", 2* 7);
            _startDate = new DateOnly(2008, 7, 16);
            _person = PersonFactory.CreatePerson();
            _target = new PersonAvailability(_person, _availability, _startDate);
            _target.StartDay = 0;
        }

        [Test]
        public void VerifyCreateInstance()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType(), true));
        }

        [Test]
        public void VerifyInheritance()
        {
            Assert.IsInstanceOf<VersionedAggregateRootWithBusinessUnit>(_target);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(_person, _target.Person);
            Assert.AreEqual(_availability, _target.Availability);
            Assert.AreEqual(_startDate, _target.StartDate);
            Assert.AreEqual(0, _target.StartDay);

            AvailabilityRotation another = new AvailabilityRotation("My other avail", 7);
            _target.Availability = another;
            Assert.AreEqual(another, _target.Availability);
            _target.StartDate = new DateOnly(2008, 8, 8);
            Assert.AreEqual(new DateOnly(2008, 8, 8), _target.StartDate);
            _target.StartDay = 0;
            Assert.AreEqual(0, _target.StartDay);

        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void VerifyPersonNotNull()
        {
            _target = new PersonAvailability(null, _availability, _startDate);
        }

        [Test, ExpectedException(typeof(ArgumentNullException))]
        public void VerifyRotationNotNull()
        {
            _target = new PersonAvailability(_person, null, _startDate);
        }

        //[Test]
        //public void VerifyCanGetSpecificRotationDay()
        //{
        //    DateTime date = new DateTime(2008, 7, 27, 0, 0, 0, DateTimeKind.Utc);
        //    IRotationDayBase<IAvailabilityDayRestriction> rotationDay = _target.Get(date);
        //    IRotationDayBase<IAvailabilityDayRestriction> expectedRotationDay = _availability.RotationDays[11];

        //    Assert.AreEqual(expectedRotationDay, rotationDay);
        //}

        [Test]
        public void VerifyCanGetCorrectRotationDayWhenUsingSameDateAsStartDate()
        {
            IAvailabilityDay availabilityDay = _target.GetAvailabilityDay(_startDate);
            IAvailabilityDay expectedavailabilityDay = _availability.AvailabilityDays[0];

            Assert.AreEqual(expectedavailabilityDay, availabilityDay);
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void VerifyDateIsNotLargerThanStartDate()
        {
            DateOnly date = new DateOnly(2008, 7, 15);
            _target.GetAvailabilityDay(date);
        }

        [Test]
        public void CanSetDeleted()
        {
            Assert.IsFalse(_target.IsDeleted);
            _target.SetDeleted();
            Assert.IsTrue(_target.IsDeleted);
        }

        [Test]
        public void VerifyFilterPersonAvailability()
        {
            IPerson person2 = PersonFactory.CreatePerson();
            DateOnly startDate2 = new DateOnly(2008, 9, 16);
            IAvailabilityRotation availability2 = new AvailabilityRotation("MyAval2", 4 * 7);
            PersonAvailability persAvail2 = new PersonAvailability(_person, availability2, startDate2);

            PersonAvailability persAvail3 = new PersonAvailability(person2, availability2, startDate2);

            IList<IPersonAvailability> lst = new List<IPersonAvailability>();
            lst.Add(_target);
            lst.Add(persAvail2);
            lst.Add(persAvail3);

            DateOnly d = new DateOnly(2008, 9, 17);

            IAvailabilityRestriction rest = _person.GetPersonAvailabilityDayRestriction(lst, d);
            Assert.IsNotNull(rest);



        }
    }
}
