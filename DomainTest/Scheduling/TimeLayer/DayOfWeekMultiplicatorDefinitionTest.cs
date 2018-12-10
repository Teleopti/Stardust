using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.DomainTest.Scheduling.TimeLayer
{
    [TestFixture]
    public class DayOfWeekMultiplicatorDefinitionTest
    {
        private DayOfWeekMultiplicatorDefinition _target;
        private Multiplicator _multiplicator;
        private TimePeriod _tp;
        private TimeZoneInfo _tzInfo;


        [SetUp]
        public void Setup()
        {
	        _multiplicator = new Multiplicator(MultiplicatorType.OBTime) {MultiplicatorValue = 3.5};
	        _tp = new TimePeriod(TimeSpan.FromHours(8), TimeSpan.FromHours(9));
            _tzInfo = (TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
            _target = new DayOfWeekMultiplicatorDefinition(_multiplicator, DayOfWeek.Monday, _tp);
        }

        [Test]
        public void VerifyConstructor()
        {
            _target = new DayOfWeekMultiplicatorDefinition(_multiplicator,DayOfWeek.Monday, _tp);
            Assert.AreEqual(3.5, _target.Multiplicator.MultiplicatorValue);
            Assert.AreEqual(DayOfWeek.Monday, _target.DayOfWeek);
            Assert.AreEqual(_tp, _target.Period);

            _target.DayOfWeek = DayOfWeek.Tuesday;
            _target.Period = new TimePeriod(2,0,4,0);

            _target.DayOfWeek.Should().Be.EqualTo(DayOfWeek.Tuesday);
            _target.Period.Should().Be.EqualTo(new TimePeriod(2, 0, 4, 0));

            _target.Multiplicator = new Multiplicator(MultiplicatorType.OBTime);
            _target.Multiplicator.Should().Not.Be.EqualTo(_multiplicator);
        }

        [Test]
        public void VerifyArgumentExceptionIfWrongDayOfWeek()
        {
			Assert.Throws<ArgumentException>(() =>
			{
				DateTime local = new DateTime(2008, 2, 3, 8, 0, 0, DateTimeKind.Unspecified);
				DateTime utc = TimeZoneHelper.ConvertToUtc(local, _tzInfo);
				DateTimePeriod utcPeriod = new DateTimePeriod(utc, utc.AddHours(1));
				Assert.AreEqual(utcPeriod, _target.ConvertToDateTimePeriod(new DateOnly(2008, 2, 3), _tzInfo));

			});
        }

        [Test]
        public void VerifyConvertToDateTimePeriod()
        {
            DateTime local = new DateTime(2008, 2, 4, 8, 0, 0, DateTimeKind.Unspecified);
            DateTime utc = TimeZoneHelper.ConvertToUtc(local, _tzInfo);
            DateTimePeriod utcPeriod = new DateTimePeriod(utc, utc.AddHours(1));
            Assert.AreEqual(utcPeriod, _target.ConvertToDateTimePeriod(new DateOnly(2008, 2, 4), _tzInfo));
        }

        [Test]
        public void VerifyConvertToLayer()
        {
            DateTime local = new DateTime(2008, 2, 4, 8, 0, 0, DateTimeKind.Unspecified);
            DateTime utc = TimeZoneHelper.ConvertToUtc(local, _tzInfo);
            DateTimePeriod utcPeriod = new DateTimePeriod(utc, utc.AddHours(1));
            Assert.AreEqual(3.5d, _target.ConvertToLayer(new DateOnly(2008, 2, 4), _tzInfo).Payload.MultiplicatorValue);
            Assert.AreEqual(utcPeriod, _target.ConvertToLayer(new DateOnly(2008, 2, 4), _tzInfo).Period);
        }

        [Test]
        public void VerifyOrderIndexThrowsExceptionIfParentNull()
        {
			Assert.Throws<ArgumentNullException>(() =>
			{
				Assert.AreEqual(1, _target.OrderIndex);
			}); 
        }

        [Test]
        public void VerifyOrderIndex()
        {
            IMultiplicatorDefinitionSet set = new MultiplicatorDefinitionSet("Hello", MultiplicatorType.OBTime);
            DayOfWeekMultiplicatorDefinition target2 = new DayOfWeekMultiplicatorDefinition(_multiplicator, DayOfWeek.Monday, _tp);
            set.AddDefinition(_target);
            set.AddDefinition(target2);
            Assert.AreEqual(0, _target.OrderIndex);
            Assert.AreEqual(1, target2.OrderIndex);
        }

        [Test]
        public void ShouldHaveDefaultConstructor()
        {
            ReflectionHelper.HasDefaultConstructor(_target.GetType()).Should().Be.True();
        }

        [Test]
        public void ShouldClone()
        {
            ((IMultiplicatorDefinition)_target).SetId(Guid.NewGuid());

            var clone = _target.EntityClone();
            clone.Should().Not.Be.SameInstanceAs(_target);
            clone.Id.Should().Be.EqualTo(_target.Id);

            clone = _target.NoneEntityClone();
            clone.Should().Not.Be.SameInstanceAs(_target);
            clone.Id.HasValue.Should().Be.False();

            clone = (IMultiplicatorDefinition)_target.Clone();
            clone.Should().Not.Be.SameInstanceAs(_target);
            clone.Id.Should().Be.EqualTo(_target.Id);
        }
    }
}
