using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.DomainTest.Forecasting
{
    [TestFixture]
    public class UnsavedDaysInfoTest
    {
        private IUnsavedDaysInfo _target;
        private readonly IScenario _scenario1 = new Scenario("Sort of");
        private readonly IScenario _scenario2 = new Scenario("Chop Suey");
        private readonly IScenario _scenario3 = new Scenario("What if");

        [SetUp]
        public void Setup()
        {
            _target = new UnsavedDaysInfo();
            _target.Add(new UnsavedDayInfo(new DateOnly(2011, 5, 9), _scenario1));
            _target.Add(new UnsavedDayInfo(new DateOnly(2011, 5, 11), _scenario2));
            _target.Add(new UnsavedDayInfo(new DateOnly(2011, 5, 10), _scenario3));
        }

        [Test]
        public void ShouldGetUnsavedDays()
        {
            var unsavedDays1 = new UnsavedDaysInfo();
            unsavedDays1.Add(new UnsavedDayInfo(new DateOnly(2011, 5, 9), _scenario1));
            unsavedDays1.Add(new UnsavedDayInfo(new DateOnly(2011, 5, 11), _scenario2));
            unsavedDays1.Add(new UnsavedDayInfo(new DateOnly(2011, 5, 10), _scenario3));
            Assert.IsTrue(unsavedDays1.UnsavedDays.SequenceEqual(_target.UnsavedDays));
        }

        [Test]
        public void ShouldVerifyEquality()
        {
            var unsavedDays1 = new UnsavedDaysInfo();
            unsavedDays1.Add(new UnsavedDayInfo(new DateOnly(2011, 5, 9), _scenario1));
            unsavedDays1.Add(new UnsavedDayInfo(new DateOnly(2011, 5, 11), _scenario2));
            unsavedDays1.Add(new UnsavedDayInfo(new DateOnly(2011, 5, 10), _scenario3));
            Assert.IsTrue(unsavedDays1.Equals(_target));
        }

        [Test]
        public void ShouldHasRightCount()
        {
            Assert.AreEqual(3, _target.Count);
        }

        [Test]
        public void ShouldCheckIfItemExists()
        {
            Assert.IsTrue(_target.Contains(new UnsavedDayInfo(new DateOnly(2011, 5, 9), _scenario1)));
            Assert.IsFalse(_target.Contains(new UnsavedDayInfo(new DateOnly(2011, 5, 9), _scenario2)));
        }

        [Test]
        public void ShouldCheckIfItemExistsByDateTimeSpecified()
        {
            Assert.IsTrue(_target.ContainsDateTime(new DateOnly(2011, 5, 9)));
            Assert.IsFalse(_target.ContainsDateTime(new DateOnly(2011, 4, 9)));
        }

        [Test]
        public void ShouldAddItem()
        {
            var unsavedDay = new UnsavedDayInfo(new DateOnly(2011, 6, 9), _scenario1);
            _target.Add(unsavedDay);
            Assert.AreEqual(4, _target.Count);
            Assert.IsTrue(_target.Contains(unsavedDay));
        }

        [Test]
        public void ShouldGetOrderedDays()
        {
            Assert.IsFalse(_target.UnsavedDays.SequenceEqual(_target.UnsavedDaysOrderedByDate));
            var unsavedDays2 = new UnsavedDaysInfo();
            unsavedDays2.Add(new UnsavedDayInfo(new DateOnly(2011, 5, 9), _scenario1));
            unsavedDays2.Add(new UnsavedDayInfo(new DateOnly(2011, 5, 10), _scenario3));
            unsavedDays2.Add(new UnsavedDayInfo(new DateOnly(2011, 5, 11), _scenario2));
            Assert.IsTrue(unsavedDays2.UnsavedDays.SequenceEqual(_target.UnsavedDaysOrderedByDate));
        }

    }
}
