using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon;


namespace Teleopti.Ccc.DomainTest.Scheduling.TimeLayer
{
    [TestFixture]
    public class DateTimeMultiplicatorDefinitionTest
    {
        private DateTimeMultiplicatorDefinition _target;
        private Multiplicator _multiplicator;
        private DateOnly _startDate;
        private DateOnly _endDate;
        private TimeSpan _startTime;
        private TimeSpan _endTime;

        [SetUp]
        public void Setup()
        {
	        _multiplicator = new Multiplicator(MultiplicatorType.OBTime) {MultiplicatorValue = 3.5};
	        _startDate = new DateOnly(2008, 1, 1);
            _endDate = new DateOnly(2008, 1, 3);
            _startTime = TimeSpan.FromHours(6);
            _endTime = TimeSpan.FromHours(18);

            _target = new DateTimeMultiplicatorDefinition(_multiplicator, _startDate, _endDate, _startTime, _endTime);
        }

        [Test]
        public void VerifyConstructor()
        {
            Assert.AreEqual(3.5, _target.Multiplicator.MultiplicatorValue);
        }

        [Test]
        public void VerifyConstructorWhenValidationFails()
        {
			Assert.Throws<ArgumentOutOfRangeException>(() => _target = new DateTimeMultiplicatorDefinition(_multiplicator, _endDate, _startDate, _startTime, _endTime));
        }

        [Test]
        public void VerifyProperties()
        {
	        _target = new DateTimeMultiplicatorDefinition(_multiplicator, _startDate.AddDays(1), _endDate.AddDays(1),
		        _startTime.Add(TimeSpan.FromHours(1)), _endTime.Add(TimeSpan.FromHours(1)))
	        {
		        StartDate = _startDate,
		        EndDate = _startDate,
		        StartTime = _startTime,
		        EndTime = _startTime
	        };
	        Assert.AreEqual(_startDate, _target.StartDate);
            Assert.AreEqual(_startDate, _target.EndDate);
            Assert.AreEqual(_startTime, _target.StartTime);
            Assert.AreEqual(_startTime, _target.EndTime);
        }
		
        [Test]
		public void VerifyInvalidPeriod()
		{
			_target = new DateTimeMultiplicatorDefinition(_multiplicator, _startDate.AddDays(1), _endDate.AddDays(1), _startTime.Add(TimeSpan.FromHours(1)), _endTime.Add(TimeSpan.FromHours(1)));
			var startDate = _startDate.AddDays(1);
			var endDate = _startDate;
			var startTime = TimeSpan.FromHours(8);
			var endTime = TimeSpan.FromHours(6);

			_target.EndDate = endDate;
			_target.EndTime = endTime;
			_target.StartDate = startDate;
			_target.StartTime = startTime;

			TimeZoneInfo tzInfo = (TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));

			Assert.Throws<ArgumentOutOfRangeException>(() =>_target.ConvertToLayer(tzInfo));
			Assert.Throws<ArgumentOutOfRangeException>(() =>_target.GetLayersForPeriod(new DateOnlyPeriod(startDate, endDate), tzInfo));
		}

        [Test]
        public void ShouldHandleSameDateProperly()
        {
            _target = new DateTimeMultiplicatorDefinition(_multiplicator, _startDate, _startDate, _startTime, _endTime);
            
            TimeZoneInfo tzInfo = (TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));

            _target.ConvertToLayer(tzInfo).Should().Not.Be.Null();
            _target.GetLayersForPeriod(new DateOnlyPeriod(_startDate, _endDate), tzInfo).Count.Should().Be.EqualTo(1);
        }

        [Test]
        public void VerifyConvertToLayer()
        {
            TimeZoneInfo tzInfo = (TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time"));
            DateTime agentLocalStart = new DateTime(2008, 1, 1, 6, 0, 0, DateTimeKind.Unspecified);
            DateTime agentLocalEnd = new DateTime(2008, 1, 3, 18, 0, 0, DateTimeKind.Unspecified);
            DateTimePeriod periodUtc = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(agentLocalStart, agentLocalEnd, tzInfo);
         
            Assert.AreEqual(3.5, _target.Multiplicator.MultiplicatorValue);
            Assert.AreEqual(periodUtc, _target.ConvertToLayer(tzInfo).Period);
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
            _target = new DateTimeMultiplicatorDefinition(_multiplicator, _startDate, _endDate, _startTime, _endTime);
            DateTimeMultiplicatorDefinition target2 = new DateTimeMultiplicatorDefinition(_multiplicator, _startDate, _endDate, _startTime, _endTime);
            set.AddDefinition(_target);
            set.AddDefinition(target2);
            Assert.AreEqual(0, _target.OrderIndex);
            Assert.AreEqual(1, target2.OrderIndex);
        }

        [Test]
        public void VerifyGetLayersForPeriodInside()
        {
            TimeZoneInfo tzInfo = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            DateTime agentLocalStart = new DateTime(2008, 1, 2, 0, 0, 0, DateTimeKind.Unspecified);
            DateTime agentLocalEnd = new DateTime(2008, 1, 3, 0, 0, 0, DateTimeKind.Unspecified);
            DateTimePeriod periodUtc = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(agentLocalStart, agentLocalEnd, tzInfo);
            _target = new DateTimeMultiplicatorDefinition(_multiplicator, new DateOnly(2008, 1, 1), new DateOnly(2008, 1, 4), _startTime, _endTime);
            IList<IMultiplicatorLayer> layers = _target.GetLayersForPeriod(new DateOnlyPeriod(2008, 1, 2, 2008, 1, 2), tzInfo);
            Assert.AreEqual(1, layers.Count);
            Assert.AreEqual(periodUtc, layers[0].Period);
        }

        [Test]
        public void VerifyGetLayersForPeriodOutside()
        {
            TimeZoneInfo tzInfo = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            DateTime agentLocalStart = new DateTime(2008, 1, 2, 0, 0, 0, DateTimeKind.Unspecified);
            DateTime agentLocalEnd = new DateTime(2008, 1, 3, 0, 0, 0, DateTimeKind.Unspecified);
            DateTimePeriod periodUtc = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(agentLocalStart, agentLocalEnd, tzInfo);
            _target = new DateTimeMultiplicatorDefinition(_multiplicator, new DateOnly(2008, 1, 2), new DateOnly(2008, 1, 3), TimeSpan.Zero, TimeSpan.Zero);
            IList<IMultiplicatorLayer> layers = _target.GetLayersForPeriod(new DateOnlyPeriod(2008, 1, 1,2008, 1, 4), tzInfo);
            Assert.AreEqual(1, layers.Count);
            Assert.AreEqual(periodUtc, layers[0].Period);
        }

        [Test]
        public void VerifyGetLayersForPeriodWhenDefinitionIsMovedOutside()
        {
            TimeZoneInfo tzInfo = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            _target = new DateTimeMultiplicatorDefinition(_multiplicator, new DateOnly(2008, 1, 2), new DateOnly(2008, 1, 3), TimeSpan.Zero, TimeSpan.Zero);
            
            //Vad ska hända om man gör så här?
            _target.StartDate = new DateOnly(2007,12,1);
            _target.EndDate = new DateOnly(2007, 12, 2);

            IList<IMultiplicatorLayer> layers = _target.GetLayersForPeriod(new DateOnlyPeriod(2008, 1, 1,2008, 1, 4), tzInfo);
            Assert.AreEqual(0, layers.Count);
        }

		[Test]
		public void ShouldHandleProjectionForNextDay()
		{
			TimeZoneInfo tzInfo = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
			DateTime agentLocalStart = new DateTime(2008, 1, 2, 0, 0, 0, DateTimeKind.Unspecified);
			DateTime agentLocalEnd = new DateTime(2008, 1, 2, 6, 0, 0, DateTimeKind.Unspecified);
			DateTimePeriod periodUtc = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(agentLocalStart, agentLocalEnd, tzInfo);
			_target = new DateTimeMultiplicatorDefinition(_multiplicator, new DateOnly(2008, 1, 2), new DateOnly(2008, 1, 2), TimeSpan.Zero, TimeSpan.FromHours(6));
			IList<IMultiplicatorLayer> layers = _target.GetLayersForPeriod(new DateOnlyPeriod(2007, 12, 31,2008, 1, 2), tzInfo);
			Assert.AreEqual(1, layers.Count);
			Assert.AreEqual(periodUtc, layers[0].Period);
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
