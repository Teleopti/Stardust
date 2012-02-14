using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture]
    public class SchedulePartExtractorTest
    {
        private SchedulePartExtractor _target;
        private MockRepository _mockRepository;
        private DateTime _dateTime;
        private DateOnly _startDate;
        private IVirtualSchedulePeriod _schedulePeriod;
        private IPerson _person;
        private IScheduleDay _schedulePart;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository();
            _dateTime = new DateTime(2009, 2, 2, 0, 0, 0, DateTimeKind.Utc);
            _startDate = new DateOnly(_dateTime);
            _schedulePeriod = _mockRepository.StrictMock<IVirtualSchedulePeriod>();
            _person = _mockRepository.StrictMock<IPerson>();
            _schedulePart = _mockRepository.StrictMock<IScheduleDay>();
        }

        [Test]
        public void VerifyAnotherTypeIsNotConsideredEquals()
        {
            //för svårt att skapa en schedulepartextractor så jag kör metoden nedan
            VerifyConstructor();
            Assert.IsFalse(_target.Equals(new object()));
        }

        [Test]
        public void VerifyConstructor()
        {
            IPermissionInformation permissionInformation = _mockRepository.StrictMock<IPermissionInformation>();
            ICccTimeZoneInfo timeZoneInfo = _mockRepository.StrictMock<ICccTimeZoneInfo>();

            DateTimePeriod dateTimePeriod  = new DateTimePeriod(_dateTime, _dateTime.AddDays(1));
            DateOnlyPeriod dateOnlyPeriod = new DateOnlyPeriod(_startDate, _startDate.AddDays(1));
            
            _mockRepository.BackToRecordAll();

            Expect.Call(_schedulePart.Person).Return(_person).Repeat.Once();
            Expect.Call(_schedulePart.Period).Return(dateTimePeriod).Repeat.Once();
            Expect.Call(_person.PermissionInformation).Return(permissionInformation).Repeat.Once();
            Expect.Call(permissionInformation.DefaultTimeZone()).Return(timeZoneInfo).Repeat.Once();
            Expect.Call(timeZoneInfo.ConvertTimeFromUtc(_dateTime, timeZoneInfo)).Return(_dateTime).Repeat.Once();
            Expect.Call(_schedulePart.Person).Return(_person).Repeat.Once();
            Expect.Call(_person.VirtualSchedulePeriod(_startDate)).Return(_schedulePeriod).Repeat.Once();
            Expect.Call(_schedulePeriod.DateOnlyPeriod).Return(dateOnlyPeriod).Repeat.Once();
            Expect.Call(_schedulePeriod.IsValid).Return(true);
            _mockRepository.ReplayAll();

            _target = new SchedulePartExtractor(_schedulePart);
            Assert.IsNotNull(_target);

            Assert.AreSame(_schedulePeriod, _target.SchedulePeriod);
            Assert.AreEqual(_person, _target.Person);
            Assert.AreSame(_schedulePart, _target.SchedulePart);
            Assert.AreEqual(dateOnlyPeriod, _target.ActualSchedulePeriod);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyConstructorExceptionNullSchedulePart()
        {
            _target = new SchedulePartExtractor(null);
            Assert.IsNotNull(_target);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void VerifyConstructorExceptionNullPerson()
        {
            IScheduleDay schedulePart = _mockRepository.StrictMock<IScheduleDay>();
            _mockRepository.BackToRecordAll();

            Expect.Call(schedulePart.Person).Return(null).Repeat.Once();

            _mockRepository.ReplayAll();

            _target = new SchedulePartExtractor(schedulePart);
            Assert.IsNotNull(_target);
        }

        [Test]
        public void VerifyConstructorSchedulePeriod()
        {
            IPermissionInformation permissionInformation = _mockRepository.StrictMock<IPermissionInformation>();
            ICccTimeZoneInfo timeZoneInfo = _mockRepository.StrictMock<ICccTimeZoneInfo>();
            IVirtualSchedulePeriod schedulePeriod = _mockRepository.StrictMock<IVirtualSchedulePeriod>();

            DateTimePeriod dateTimePeriod = new DateTimePeriod(_dateTime, _dateTime.AddDays(1));
            //DateOnlyPeriod? dateOnlyPeriod = null;

            _mockRepository.BackToRecordAll();

            Expect.Call(_schedulePart.Person).Return(_person).Repeat.Once();
            Expect.Call(_schedulePart.Period).Return(dateTimePeriod).Repeat.Once();
            Expect.Call(_person.PermissionInformation).Return(permissionInformation).Repeat.Once();
            Expect.Call(permissionInformation.DefaultTimeZone()).Return(timeZoneInfo).Repeat.Once();
            Expect.Call(timeZoneInfo.ConvertTimeFromUtc(_dateTime, timeZoneInfo)).Return(_dateTime).Repeat.Once();
            Expect.Call(_schedulePart.Person).Return(_person).Repeat.Once();
            Expect.Call(_person.VirtualSchedulePeriod(_startDate)).Return(schedulePeriod).Repeat.Once();
            Expect.Call(schedulePeriod.IsValid).Return(false).Repeat.Once().Repeat.Any();
            

            _mockRepository.ReplayAll();

            _target = new SchedulePartExtractor(_schedulePart);
            Assert.IsNotNull(_target);
            Assert.IsFalse(_target.SchedulePeriod.IsValid);
        }

        [Test]
        public void VerifyNotValidSchedulePeriod()
        {
            DateTime dateTime = new DateTime(2009, 2, 2, 0, 0, 0, DateTimeKind.Utc);
            DateOnly dateOnly = new DateOnly(dateTime);
            IScheduleDay schedulePart = _mockRepository.StrictMock<IScheduleDay>();
            IPermissionInformation permissionInformation = _mockRepository.StrictMock<IPermissionInformation>();
            ICccTimeZoneInfo timeZoneInfo = _mockRepository.StrictMock<ICccTimeZoneInfo>();


            DateTimePeriod dateTimePeriod = new DateTimePeriod(dateTime, dateTime.AddDays(1));


            _mockRepository.BackToRecordAll();

            Expect.Call(schedulePart.Person).Return(_person).Repeat.Once();
            Expect.Call(schedulePart.Period).Return(dateTimePeriod).Repeat.Once();
            Expect.Call(_person.PermissionInformation).Return(permissionInformation).Repeat.Once();
            Expect.Call(permissionInformation.DefaultTimeZone()).Return(timeZoneInfo).Repeat.Once();
            Expect.Call(timeZoneInfo.ConvertTimeFromUtc(dateTime, timeZoneInfo)).Return(dateTime).Repeat.Once();
            Expect.Call(schedulePart.Person).Return(_person).Repeat.Once();
            Expect.Call(_person.VirtualSchedulePeriod(dateOnly)).Return(_schedulePeriod).Repeat.Once();
            Expect.Call(_schedulePeriod.IsValid).Return(false);

            _mockRepository.ReplayAll();

            _target = new SchedulePartExtractor(schedulePart);
            
        }

        //[Test]
        //public void VerifyProperties()
        //{
        //    Assert.AreEqual(_startDate, _target.StartDate);
        //    Assert.AreSame(_schedulePeriod, _target.SchedulePeriod);
        //    Assert.AreEqual(_person, _target.Person);
        //}

        //[Test]
        //[ExpectedException(typeof(ArgumentException))]
        //public void VerifyInvalidPersonInConstructor()
        //{
        //    _mockRepository.BackToRecordAll();

        //    Expect.Call(_schedulePeriod.Parent).Return(null).Repeat.Once();

        //    _mockRepository.ReplayAll();

        //    _target = new SchedulePartExtractor(_schedulePart);

        //    _mockRepository.VerifyAll();

        //}

        //[Test]
        //[ExpectedException(typeof(ArgumentOutOfRangeException))]
        //public void VerifyInvalidPeriodInConstructor()
        //{
        //    _mockRepository.BackToRecordAll();

        //    Expect.Call(_schedulePeriod.Parent).Return(_person).Repeat.Once();
        //    Expect.Call(_schedulePeriod.GetSchedulePeriod(_startDate)).Return(null).Repeat.Once();
        //    Expect.Call(_schedulePeriod.DateFrom).Return(_startDate).Repeat.Once();

        //    _mockRepository.ReplayAll();

        //    _target = new SchedulePartExtractor(_schedulePart);
        //    Assert.IsNotNull(_target);
        //}
    }
}
