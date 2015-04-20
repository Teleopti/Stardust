using System;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
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
            var dateOnlyPeriod = new DateOnlyPeriod(_startDate, _startDate.AddDays(1));
            
            _mockRepository.BackToRecordAll();

            Expect.Call(_schedulePart.Person).Return(_person).Repeat.Once();
            Expect.Call(_schedulePart.Person).Return(_person).Repeat.Once();
			Expect.Call(_schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_startDate, TimeZoneInfo.Utc)).Repeat.Once();
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
			
			_mockRepository.VerifyAll();
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
			
			_mockRepository.VerifyAll();
        }

        [Test]
        public void VerifyConstructorSchedulePeriod()
        {
            var schedulePeriod = _mockRepository.StrictMock<IVirtualSchedulePeriod>();

            _mockRepository.BackToRecordAll();

            Expect.Call(_schedulePart.Person).Return(_person).Repeat.Once();
            Expect.Call(_schedulePart.Person).Return(_person).Repeat.Once(); 
			Expect.Call(_schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(_startDate, TimeZoneInfo.Utc)).Repeat.Once();
            Expect.Call(_person.VirtualSchedulePeriod(_startDate)).Return(schedulePeriod).Repeat.Once();
            Expect.Call(schedulePeriod.IsValid).Return(false).Repeat.Once().Repeat.Any();
            

            _mockRepository.ReplayAll();

            _target = new SchedulePartExtractor(_schedulePart);
            Assert.IsNotNull(_target);
            Assert.IsFalse(_target.SchedulePeriod.IsValid);

			_mockRepository.VerifyAll();
        }

        [Test]
        public void VerifyNotValidSchedulePeriod()
        {
            var dateTime = new DateTime(2009, 2, 2, 0, 0, 0, DateTimeKind.Utc);
            var dateOnly = new DateOnly(dateTime);
            
            _mockRepository.BackToRecordAll();

            Expect.Call(_schedulePart.Person).Return(_person).Repeat.Once();
            Expect.Call(_schedulePart.Person).Return(_person).Repeat.Once();
            Expect.Call(_schedulePart.DateOnlyAsPeriod).Return(new DateOnlyAsDateTimePeriod(dateOnly,TimeZoneInfo.Utc)).Repeat.Once();
            Expect.Call(_person.VirtualSchedulePeriod(dateOnly)).Return(_schedulePeriod).Repeat.Once();
            Expect.Call(_schedulePeriod.IsValid).Return(false);

            _mockRepository.ReplayAll();

            _target = new SchedulePartExtractor(_schedulePart);

			_mockRepository.VerifyAll();
        }
    }
}
