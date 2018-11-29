using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Optimization
{
    [TestFixture, SetCulture("sv-SE")]
    public class FullWeekOuterWeekPeriodCreatorTest
    {
        private FullWeekOuterWeekPeriodCreator _target;
        private DateOnlyPeriod _period;
        private DateOnly _startDate;
        private DateOnly _endDate;
        private IPerson _person;

        [SetUp]
        public void Setup()
        {
            _startDate = new DateOnly(2010, 1, 8);
            _endDate = new DateOnly(2010, 1, 15);
            _period = new DateOnlyPeriod(_startDate, _endDate);
            _person = PersonFactory.CreatePerson();
            _person.FirstDayOfWeek = DayOfWeek.Monday;
            _target = new FullWeekOuterWeekPeriodCreator(_period, _person);
        }

        [Test]
        public void VerifyEffectivePeriod()
        {
            DateOnlyPeriod effectivePeriod = _target.EffectivePeriod();
            Assert.AreEqual(effectivePeriod, _period);
        }

        [Test]
        public void VerifyPerson()
        {
            IPerson person = _target.Person;
            Assert.AreEqual(_person, person);
        }

        [Test]
        public void VerifyExtendedPeriodIsRight()
        {
            var expected = new DateOnlyPeriod(new DateOnly(2010, 1, 4), new DateOnly(2010, 1, 17));
            DateOnlyPeriod extendedPeriod = _target.FullWeekPeriod();
            Assert.AreEqual(expected, extendedPeriod);
        }

        [Test]
        public void VerifyExtendedPeriodIsFullWeek()
        {
            DateOnlyPeriod extendedPeriod = _target.FullWeekPeriod();
            Assert.IsTrue(extendedPeriod.DayCount() % 7 == 0);
        }

        [Test]
        public void VerifyOuterPeriodIsRight()
        {
            var expected = new DateOnlyPeriod(new DateOnly(2009, 12, 28), new DateOnly(2010, 1, 24));
            DateOnlyPeriod outerPeriod = _target.OuterWeekPeriod();
            Assert.AreEqual(expected, outerPeriod);
        }

        [Test]
        public void VerifyOuterPeriodIsFullWeek()
        {
            DateOnlyPeriod outerPeriod = _target.OuterWeekPeriod();
            Assert.IsTrue(outerPeriod.DayCount() % 7 == 0);

        }
    }
}