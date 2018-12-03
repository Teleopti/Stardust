using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.DomainTest.Scheduling.Restriction
{
    [TestFixture]
    public class OvertimeAvailabilityTest
    {
        private IOvertimeAvailability _target;
        private IPerson _person;
        private DateOnly _dateOnly;
        private IDateOnlyAsDateTimePeriod _dateAndPeriod;
        private DateOnlyPeriod _dateOnlyPeriod;
        private TimeSpan? _startTime;
        private TimeSpan? _endTime;


        [SetUp]
        public void SetUp()
        {
            _person = new Person().WithName(new Name("Test", "test2"));
            _dateOnly = DateOnly.Today ;
            _dateOnlyPeriod = new DateOnlyPeriod(_dateOnly,_dateOnly );
            _dateAndPeriod = new DateOnlyAsDateTimePeriod(_dateOnly, TimeZoneInfo.Local);
            _startTime = TimeSpan.FromHours(8);
            _endTime = TimeSpan.FromHours(10);
            _target = new OvertimeAvailability(_person, _dateOnly,_startTime,_endTime);
        }

        [Test]
        public void TestPersonBeingPopulated()
        {
           Assert.AreEqual(_target.Person ,_person );
        }
      
        [Test]
        public void TestClone()
        {
            IOvertimeAvailability  targetClone = (IOvertimeAvailability) _target.Clone();
            Assert.AreEqual(targetClone.Person,_target.Person  );
            Assert.AreEqual(targetClone.Period ,_target.Period );
        }

        [Test]
        public void TestBelongsToPeriodDateAndPeriod()
        {
            Assert.IsTrue(_target.BelongsToPeriod(_dateAndPeriod));
        }
        
        [Test]
        public void TestBelongsToPeriodDateOnlyPeriod()
        {
            Assert.IsTrue(_target.BelongsToPeriod(_dateOnlyPeriod));
        }

        [Test]
        public void TestMainRoot()
        {
            Assert.AreEqual( _target.MainRoot,_person );

        }

        [Test]
        public void TestCreateTransient()
        {
            Assert.IsNull( _target.CreateTransient().Id , null);

        }

        [Test]
        public void TestNotAvailable()
        {
            _target.NotAvailable = false;
            Assert.IsFalse( _target.NotAvailable );

        }

		[Test]
		public void ShouldDelete()
		{
			(_target as OvertimeAvailability).SetDeleted();
			_target.IsDeleted.Should().Be.True();
		}

    }
}
