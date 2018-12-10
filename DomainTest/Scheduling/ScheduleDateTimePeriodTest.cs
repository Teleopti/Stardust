using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Ccc.DomainTest.Scheduling
{
    [TestFixture]
    public class ScheduleDateTimePeriodTest
    {
        private IScheduleDateTimePeriod target;
        private IList<IPerson> persons;
        private DateTimePeriod period;
        private ISchedulerRangeToLoadCalculator calc;
        private MockRepository mocks;
        private IPerson person1;
        private IPerson person2;

        [SetUp]
        public void Setup()
        {
            person1 = new Person();
            person2 = new Person();
            mocks = new MockRepository();
            calc = mocks.StrictMock<ISchedulerRangeToLoadCalculator>();
            persons = new List<IPerson> { person1, person2 };
            period = new DateTimePeriod(2000,1,1,2001,1,1);
            target= new ScheduleDateTimePeriod(period, persons, calc);
        }

        [Test]
        public void VerifyProperties()
        {
            Assert.AreEqual(period, target.VisiblePeriod);
            Assert.AreSame(calc, target.RangeToLoadCalculator);
        }

        [Test]
        public void VerifyConstructorWithNoPerson()
        {
            target = new ScheduleDateTimePeriod(period);
            Assert.AreEqual(period, target.VisiblePeriod);
            Assert.AreEqual(period, target.LoadedPeriod());
            Assert.IsInstanceOf<SchedulerRangeToLoadCalculator>(target.RangeToLoadCalculator);
            Assert.AreEqual(period, target.RangeToLoadCalculator.RequestedPeriod);
        }

        [Test]
        public void VerifyDefaultConstructor()
        {
            target = new ScheduleDateTimePeriod(period, persons);
            Assert.AreEqual(period, target.VisiblePeriod);
            Assert.AreEqual(period, target.LoadedPeriod());
            Assert.IsInstanceOf<SchedulerRangeToLoadCalculator>(target.RangeToLoadCalculator);
            Assert.AreEqual(period, target.RangeToLoadCalculator.RequestedPeriod);
        }

        [Test]
        public void VerifyMaxPeriodWhenNoPersonExists()
        {
            target = new ScheduleDateTimePeriod(period, new List<IPerson>());
            Assert.AreEqual(period, target.LoadedPeriod());
        }

        [Test]
        public void VerifyMaxPeriodWhenOutsidePeriodAndVerifyCalcOnlyCalledOnce()
        {
            using(mocks.Record())
            {
                Expect.Call(calc.SchedulerRangeToLoad(person1))
                    .Return(new DateTimePeriod(1900, 1, 1, 1900, 1, 1));
                Expect.Call(calc.SchedulerRangeToLoad(person2))
                                    .Return(new DateTimePeriod(2000, 1, 1, 2100, 1, 1));
            }
            using(mocks.Playback())
            {
                Assert.AreEqual(new DateTimePeriod(1900, 1, 1, 2100, 1, 1), target.LoadedPeriod());
                Assert.AreEqual(new DateTimePeriod(1900, 1, 1, 2100, 1, 1), target.LoadedPeriod());
            }
        }

        [Test]
        public void VerifyMaxPeriodWhenPartlyInside()
        {
            using (mocks.Record())
            {
                Expect.Call(calc.SchedulerRangeToLoad(person1))
                    .Return(new DateTimePeriod(1900, 1, 1, 1900, 1, 1));
                Expect.Call(calc.SchedulerRangeToLoad(person2))
                                    .Return(new DateTimePeriod(2000, 1, 1, 2000, 2, 1));
            }
            using (mocks.Playback())
            {
                Assert.AreEqual(new DateTimePeriod(1900, 1, 1, 2001, 1, 1), target.LoadedPeriod());
            }
        }

        [Test]
        public void VerifyMaxPeriodWhenFullyInside()
        {
            using (mocks.Record())
            {
                Expect.Call(calc.SchedulerRangeToLoad(person1))
                    .Return(new DateTimePeriod(2000, 4, 1, 2000, 8, 1));
                Expect.Call(calc.SchedulerRangeToLoad(person2))
                                    .Return(new DateTimePeriod(2000, 1, 1, 2000, 2, 1));
            }
            using (mocks.Playback())
            {
                Assert.AreEqual(new DateTimePeriod(2000, 1, 1, 2001, 1, 1), target.LoadedPeriod());
            }
        }
    }

    
}
