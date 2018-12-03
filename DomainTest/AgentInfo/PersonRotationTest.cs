using System;
using System.Collections.Generic;
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
    public class PersonRotationTest
    {
        private PersonRotation _target;
        private IPerson _person;
        private IRotation _rotation;
        private DateOnly _startDate;
        private int _startRow;
        private TimeZoneInfo _timeZone;

        [SetUp]
        public void Setup()
        {
            _rotation = new Rotation("My rotation", 14);
            _startRow = 0;
            _startDate = new DateOnly(2008,7,16);
            _timeZone = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            _person = PersonFactory.CreatePerson();
            _person.PermissionInformation.SetDefaultTimeZone(_timeZone);
            _target = new PersonRotation(_person, _rotation, _startDate, _startRow);
        }

        [Test]
        public void VerifyCreateInstance()
        {
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyEmptyConstructor()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType(),true));
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
            Assert.AreEqual(_rotation, _target.Rotation);
            Assert.AreEqual(_startDate, _target.StartDate);
            Assert.AreEqual(_startRow,_target.StartDay);
            Assert.AreEqual(
				_timeZone.SafeConvertTimeToUtc(DateTime.SpecifyKind(_startDate.Date, DateTimeKind.Unspecified)),
                _target.StartDateAsUtc);

            IRotation another = new Rotation("My other rotation", 7);
            _target.Rotation = another;
            Assert.AreEqual(another, _target.Rotation);
            _target.StartDate = new DateOnly(2008,8,8);
            Assert.AreEqual(new DateOnly(2008, 8, 8),_target.StartDate);
            Assert.AreEqual(
				_timeZone.SafeConvertTimeToUtc(
                    DateTime.SpecifyKind(new DateOnly(2008, 8, 8).Date, DateTimeKind.Unspecified)),
                _target.StartDateAsUtc);
            _target.StartDay = 0;
            Assert.AreEqual(0, _target.StartDay);
        }

        [Test]
        public void VerifyPersonNotNull()
        {
            Assert.Throws<ArgumentNullException>(() => _target = new PersonRotation(null, _rotation, _startDate, _startRow));
        }

        [Test]
        public void VerifyRotationNotNull()
        {
			Assert.Throws<ArgumentNullException>(() => _target = new PersonRotation(_person, null, _startDate, _startRow));
        }

        [Test]
        public void VerifyStartDayIsPositive()
        {
			Assert.Throws<ArgumentOutOfRangeException>(() => _target = new PersonRotation(_person, _rotation, _startDate, -1));
        }


        [Test]
        public void VerifyCanGetSpecificRotationDay()
        {
            DateOnly date = new DateOnly(2008, 7, 27).AddDays(28);
            IRotationDay rotationDay = _target.GetRotationDay(date);
            IRotationDay expectedRotationDay = _rotation.RotationDays[11];
            
            Assert.AreEqual(expectedRotationDay, rotationDay);
        }

        [Test]
        public void VerifyCanGetSpecificRotationDayEight()
        {
            _target = new PersonRotation(_person, _rotation, _startDate, 7);

            DateOnly date = new DateOnly(2008, 7, 16);
            IRotationDay rotationDay = _target.GetRotationDay(date);
            IRotationDay expectedRotationDay = _rotation.RotationDays[7];

            Assert.AreEqual(expectedRotationDay, rotationDay);
        }

        [Test]
        public void VerifyCanGetCorrectRotationDayWhenUsingSameDateAsStartDate()
        {
            DateOnly d = new DateOnly(2008, 7, 16);
            IRotationDay rotationDay = _target.GetRotationDay(d);
            IRotationDay expectedRotationDay = _rotation.RotationDays[0];

            Assert.AreEqual(expectedRotationDay, rotationDay);
        }
        
        [Test]
        public void VerifyDateIsNotLargerThanStartDate()
        {
            DateOnly date = new DateOnly(2008, 7, 15);
            Assert.Throws<ArgumentOutOfRangeException>(() => _target.GetRotationDay(date));
        }

       
        [Test]
        public void VerifyFilterPersonRotation()
        {
            IPerson person2 = PersonFactory.CreatePerson();
            DateOnly startDate2 = new DateOnly(2008, 9, 16);
            IRotation rotation2 = new Rotation("My rotation2", 4 * 7);
            PersonRotation persRotation2 = new PersonRotation(_person, rotation2, startDate2, 1);

            PersonRotation persRotation3 = new PersonRotation(person2, rotation2, startDate2, 1);

            IList<IPersonRotation> lst = new List<IPersonRotation>();
            lst.Add(_target);
            lst.Add(persRotation2);
            lst.Add(persRotation3);

            DateOnly d = new DateOnly(2008, 7, 20);
            IList<IRotationRestriction> rest = _person.GetPersonRotationDayRestrictions(lst, d);
            Assert.AreEqual(1, rest.Count, "Empty restriction will be considered a restriction");


            _rotation.RotationDays[4].RestrictionCollection[0].ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("hej");

            rest = _person.GetPersonRotationDayRestrictions(lst, d);
            Assert.AreEqual(1, rest.Count, "Shiftcategory");

				ILimitation limitation = new StartTimeLimitation(TimeSpan.FromHours(1), null);
            _rotation.RotationDays[4].RestrictionCollection[0].StartTimeLimitation = (StartTimeLimitation)limitation;
            
            rest = _person.GetPersonRotationDayRestrictions(lst, d);
            Assert.AreEqual(1, rest.Count, "StartTimeLimitation");

            WorkTimeLimitation workTime = new WorkTimeLimitation(TimeSpan.FromHours(1), TimeSpan.FromHours(2));
            _rotation.RotationDays[4].RestrictionCollection[0].WorkTimeLimitation = workTime;

            rest = _person.GetPersonRotationDayRestrictions(lst, d);
            Assert.AreEqual(1, rest.Count, "WorkTimeLimitation");
        }

        [Test]
        public void VerifyFilterPersonRotationAtFirstDay()
        {
            IPerson person2 = PersonFactory.CreatePerson();
            person2.PermissionInformation.SetDefaultTimeZone(_timeZone);
            DateOnly startDate2 = new DateOnly(2008, 9, 16);
            IRotation rotation2 = new Rotation("My rotation2", 4 * 7);
            PersonRotation persRotation2 = new PersonRotation(_person, rotation2, startDate2, 1);
            PersonRotation persRotation3 = new PersonRotation(person2, rotation2, startDate2, 1);

            IList<IPersonRotation> lst = new List<IPersonRotation>();
            lst.Add(_target);
            lst.Add(persRotation2);
            lst.Add(persRotation3);

            rotation2.RotationDays[1].RestrictionCollection[0].ShiftCategory = ShiftCategoryFactory.CreateShiftCategory("hej");

            DateOnly d = new DateOnly(2008, 9, 16);

            IList<IRotationRestriction> rest = person2.GetPersonRotationDayRestrictions(lst, d);
            Assert.AreEqual(1, rest.Count);
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
