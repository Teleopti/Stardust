using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;


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
            Assert.IsInstanceOf<AggregateRoot_Events_ChangeInfo_Versioned_BusinessUnit>(_target);
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

        [Test]
        public void VerifyPersonNotNull()
        {
	        Assert.Throws<ArgumentNullException>(() => _target = new PersonAvailability(null, _availability, _startDate));
        }

        [Test]
        public void VerifyRotationNotNull()
        {
			Assert.Throws<ArgumentNullException>(() => _target = new PersonAvailability(_person, null, _startDate));
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

        [Test]
        public void VerifyDateIsNotLargerThanStartDate()
        {
            DateOnly date = new DateOnly(2008, 7, 15);
			Assert.Throws<ArgumentOutOfRangeException>(() => _target.GetAvailabilityDay(date));
        }

        [Test]
        public void CanSetDeleted()
        {
            Assert.IsFalse(_target.IsDeleted);
            _target.SetDeleted();
            Assert.IsTrue(_target.IsDeleted);
        }
    }
}
