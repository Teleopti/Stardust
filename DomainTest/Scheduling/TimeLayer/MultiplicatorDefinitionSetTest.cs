using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;


namespace Teleopti.Ccc.DomainTest.Scheduling.TimeLayer
{
    [TestFixture]
    public class MultiplicatorDefinitionSetTest
    {
        private IMultiplicatorDefinitionSet _target;
        private DayOfWeekMultiplicatorDefinition _d1;
        private DateTimeMultiplicatorDefinition _d2;
        private DayOfWeekMultiplicatorDefinition _d3;
        private IMultiplicator _ob1;
        private IMultiplicator _ob2;
        
        [SetUp]
        public void Setup()
        {
	        _ob1 = new Multiplicator(MultiplicatorType.OBTime)
	        {
		        MultiplicatorValue = 1.5,
		        Description = new Description("ob night")
	        };
	        _ob2 = new Multiplicator(MultiplicatorType.OBTime)
	        {
		        MultiplicatorValue = 2,
		        Description = new Description("ob holiday")
	        };
	        _d1 = new DayOfWeekMultiplicatorDefinition(_ob1, DayOfWeek.Monday, new TimePeriod(TimeSpan.FromHours(18), TimeSpan.FromHours(30)));
            _d2 = new DateTimeMultiplicatorDefinition(_ob2, new DateOnly(2008, 12, 2), new DateOnly(2008, 12, 3), TimeSpan.FromHours(0), TimeSpan.FromHours(6));
            _d3 = new DayOfWeekMultiplicatorDefinition(_ob1, DayOfWeek.Tuesday, new TimePeriod(TimeSpan.FromHours(18), TimeSpan.FromHours(30)));
        }

        [Test]
        public void VerifyConstructor()
        {
            _target = new MultiplicatorDefinitionSet("Hello", MultiplicatorType.OBTime);
            Assert.AreEqual("Hello", _target.Name);
            _target.Name = "Bye";
            Assert.AreEqual("Bye", _target.Name);
            Assert.AreEqual(MultiplicatorType.OBTime, _target.MultiplicatorType);
        }

        [Test]
        public  void VerifyAddDefinition()
        {
            _target = new MultiplicatorDefinitionSet("Hello", MultiplicatorType.OBTime);
            _target.AddDefinition(_d1);
            _target.AddDefinition(_d2);
            Assert.AreEqual(_d2, _target.DefinitionCollection[1]);
            Assert.AreEqual(_target, _target.DefinitionCollection[1].Parent);
        }

        [Test]
        public void VerifyAddDefinitionWhenDifferentMultiplicatorType()
        {
            _target = new MultiplicatorDefinitionSet("Hello", MultiplicatorType.OBTime);
            IMultiplicator mp = new Multiplicator(MultiplicatorType.Overtime);
            _d1 = new DayOfWeekMultiplicatorDefinition(mp, DayOfWeek.Monday, new TimePeriod(TimeSpan.FromHours(18), TimeSpan.FromHours(30)));
			Assert.Throws<ArgumentException>(() => _target.AddDefinition(_d1));
        }

        [Test]
        public void VerifyAddDefinitionAt()
        {
            _target = new MultiplicatorDefinitionSet("Hello", MultiplicatorType.OBTime);
            _target.AddDefinition(_d1);
            _target.AddDefinition(_d2);
            _target.AddDefinitionAt(_d3, 1);

            Assert.AreEqual(_d1, _target.DefinitionCollection[0]);
            Assert.AreEqual(_d3, _target.DefinitionCollection[1]);
            Assert.AreEqual(_d2, _target.DefinitionCollection[2]);
            Assert.AreEqual(3, _target.DefinitionCollection.Count);
            Assert.AreEqual(_target, _target.DefinitionCollection[1].Parent);
        }

        [Test]
        public void VerifyAddDefinitionAtWhenDifferentMultiplicatorType()
        {
            _target = new MultiplicatorDefinitionSet("Hello", MultiplicatorType.OBTime);
            IMultiplicator mp = new Multiplicator(MultiplicatorType.Overtime);
            _d1 = new DayOfWeekMultiplicatorDefinition(mp, DayOfWeek.Monday, new TimePeriod(TimeSpan.FromHours(18), TimeSpan.FromHours(30)));
			Assert.Throws<ArgumentException>(() => _target.AddDefinitionAt(_d1, 0));
        }

        [Test]
        public void VerifyRemoveDefinition()
        {
            _target = new MultiplicatorDefinitionSet("Hello", MultiplicatorType.OBTime);
            _target.AddDefinition(_d1);
            _target.AddDefinition(_d2);
            Assert.AreEqual(2, _target.DefinitionCollection.Count);
            _target.RemoveDefinition(_d1);
            Assert.AreEqual(1, _target.DefinitionCollection.Count);
            Assert.AreEqual(_d2, _target.DefinitionCollection[0]);
        }

        [Test]
        public void VerifyMoveDefinitionUp()
        {
            _target = new MultiplicatorDefinitionSet("Hello", MultiplicatorType.OBTime);
            _target.AddDefinition(_d1);
            _target.AddDefinition(_d2);
            _target.AddDefinition(_d3);
            _target.MoveDefinitionUp(_d3);
            Assert.AreEqual(_d3, _target.DefinitionCollection[1]);
            Assert.AreEqual(_d2, _target.DefinitionCollection[2]);
            _target.MoveDefinitionUp(_d1);
            Assert.AreEqual(_d1, _target.DefinitionCollection[0]);
        }

        [Test]
        public void VerifyMoveDefinitionDown()
        {
            _target = new MultiplicatorDefinitionSet("Hello", MultiplicatorType.OBTime);
            _target.AddDefinition(_d1);
            _target.AddDefinition(_d2);
            _target.AddDefinition(_d3);
            _target.MoveDefinitionDown(_d1);
            Assert.AreEqual(_d2, _target.DefinitionCollection[0]);
            Assert.AreEqual(_d1, _target.DefinitionCollection[1]);
            _target.MoveDefinitionDown(_d3);
            Assert.AreEqual(_d3, _target.DefinitionCollection[2]);
        }

        [Test]
        public void VerifyCreateProjectionForPeriod()
        {
            _d1 = new DayOfWeekMultiplicatorDefinition(_ob1, DayOfWeek.Monday, new TimePeriod(TimeSpan.FromHours(18), TimeSpan.FromHours(30)));
            _d2 = new DateTimeMultiplicatorDefinition(_ob2, new DateOnly(2008, 12, 2), new DateOnly(2008, 12, 3), TimeSpan.FromHours(0), TimeSpan.FromHours(6));
            _d3 = new DayOfWeekMultiplicatorDefinition(_ob1, DayOfWeek.Tuesday, new TimePeriod(TimeSpan.FromHours(18), TimeSpan.FromHours(30)));
            TimeZoneInfo tzInfo = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));          

            _target = new MultiplicatorDefinitionSet("Hello", MultiplicatorType.OBTime);
            _target.AddDefinition(_d1);
            _target.AddDefinition(_d2);
            _target.AddDefinition(_d3);
            IList<IMultiplicatorLayer> result = _target.CreateProjectionForPeriod(new DateOnlyPeriod(2008, 12, 1, 2008, 12, 8), tzInfo);
            Assert.AreEqual(4, result.Count);
        }

        [Test]
        public void VerifyCreateProjectionForPeriod1()
        {
            DayOfWeekMultiplicatorDefinition d1 = new DayOfWeekMultiplicatorDefinition(_ob1, DayOfWeek.Monday, new TimePeriod(TimeSpan.FromHours(18), TimeSpan.FromHours(30)));
            DayOfWeekMultiplicatorDefinition d2 = new DayOfWeekMultiplicatorDefinition(_ob1, DayOfWeek.Tuesday, new TimePeriod(TimeSpan.FromHours(18), TimeSpan.FromHours(30)));
            DateTimeMultiplicatorDefinition d3 = new DateTimeMultiplicatorDefinition(_ob2, new DateOnly(2008, 12, 2), new DateOnly(2008, 12, 3), TimeSpan.FromHours(0), TimeSpan.FromHours(6));
            TimeZoneInfo tzInfo = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            _target = new MultiplicatorDefinitionSet("Hello", MultiplicatorType.OBTime);
            _target.AddDefinition(d1);
            _target.AddDefinition(d2);
            _target.AddDefinition(d3);
            IList<IMultiplicatorLayer> result = _target.CreateProjectionForPeriod(new DateOnlyPeriod(2008, 12, 1, 2008, 12, 8), tzInfo);
            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void VerifyCreateProjectionForPeriod2()
        {
            DayOfWeekMultiplicatorDefinition d1 = new DayOfWeekMultiplicatorDefinition(_ob1, DayOfWeek.Monday, new TimePeriod(TimeSpan.FromHours(18), TimeSpan.FromHours(30)));
            DayOfWeekMultiplicatorDefinition d2 = new DayOfWeekMultiplicatorDefinition(_ob1, DayOfWeek.Tuesday, new TimePeriod(TimeSpan.FromHours(18), TimeSpan.FromHours(30)));
            DateTimeMultiplicatorDefinition d3 = new DateTimeMultiplicatorDefinition(_ob2, new DateOnly(2008, 12, 1), new DateOnly(2008, 12, 3), TimeSpan.FromHours(0), TimeSpan.FromHours(6));
            TimeZoneInfo tzInfo = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            _target = new MultiplicatorDefinitionSet("Hello", MultiplicatorType.OBTime);
            _target.AddDefinition(d1);
            _target.AddDefinition(d2);
            _target.AddDefinition(d3);
            IList<IMultiplicatorLayer> result = _target.CreateProjectionForPeriod(new DateOnlyPeriod(2008, 12, 1, 2008, 12, 8), tzInfo);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void VerifyCreateProjectionForPeriod3()
        {
            DayOfWeekMultiplicatorDefinition d1 = new DayOfWeekMultiplicatorDefinition(_ob1, DayOfWeek.Monday, new TimePeriod(TimeSpan.FromHours(18), TimeSpan.FromHours(30)));
            DayOfWeekMultiplicatorDefinition d2 = new DayOfWeekMultiplicatorDefinition(_ob1, DayOfWeek.Tuesday, new TimePeriod(TimeSpan.FromHours(18), TimeSpan.FromHours(30)));
            DateTimeMultiplicatorDefinition d3 = new DateTimeMultiplicatorDefinition(_ob2, new DateOnly(2008, 12, 1), new DateOnly(2008, 12, 9), TimeSpan.FromHours(0), TimeSpan.FromHours(0));
            TimeZoneInfo tzInfo = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            _target = new MultiplicatorDefinitionSet("Hello", MultiplicatorType.OBTime);
            _target.AddDefinition(d1);
            _target.AddDefinition(d2);
            _target.AddDefinition(d3);
            IList<IMultiplicatorLayer> result = _target.CreateProjectionForPeriod(new DateOnlyPeriod(2008, 12, 1, 2008, 12, 8), tzInfo);
            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void VerifyConvertToLayerWithTwoDefinitions()
        {
            IMultiplicator overtime1 = new Multiplicator(MultiplicatorType.Overtime)
                                           {Description = new Description("OT1")};
            IMultiplicator overtime2 = new Multiplicator(MultiplicatorType.Overtime)
                                           {Description = new Description("OT2")};

            var definition1 = new DayOfWeekMultiplicatorDefinition(overtime1, DayOfWeek.Sunday, new TimePeriod(10,0,12,0));
            var definition2 = new DayOfWeekMultiplicatorDefinition(overtime2, DayOfWeek.Sunday, new TimePeriod(12, 0, 24, 0));
            _target = new MultiplicatorDefinitionSet("Overtime", MultiplicatorType.Overtime);
            _target.AddDefinition(definition1);
            _target.AddDefinition(definition2);

            TimeZoneInfo tzInfo = (TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time"));
            var layers = _target.CreateProjectionForPeriod(new DateOnlyPeriod(2009, 9, 1, 2009, 9, 30), tzInfo);

            Assert.AreEqual(8,layers.Count);
            Assert.AreEqual(
                TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(new DateTime(2009, 9, 6, 10, 0, 0),
                                                                     new DateTime(2009, 9, 6, 12, 0, 0), tzInfo),
                layers.First().Period);
            Assert.AreEqual(
                TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(new DateTime(2009, 9, 27, 12, 0, 0),
                                                                     new DateTime(2009, 9, 28, 0, 0, 0), tzInfo),
                layers.Last().Period);
        }

        [Test]
        public void ShouldDelete()
        {
            _target = new MultiplicatorDefinitionSet("Hello", MultiplicatorType.OBTime);
            
            var target = _target as MultiplicatorDefinitionSet;
            target.SetDeleted();
            target.IsDeleted.Should().Be.True();
        }

        [Test]
        public void ShouldClone()
        {
            _target = new MultiplicatorDefinitionSet("Hello", MultiplicatorType.OBTime);
            _target.AddDefinition(_d1);
            _target.SetId(Guid.NewGuid());

            var clone = _target.EntityClone();
            clone.Should().Not.Be.SameInstanceAs(_target);
            clone.DefinitionCollection.Should().Not.Be.SameInstanceAs(_target.DefinitionCollection);
            clone.DefinitionCollection.Count.Should().Be.EqualTo(_target.DefinitionCollection.Count);
            clone.Id.Should().Be.EqualTo(_target.Id);

            clone = _target.NoneEntityClone();
            clone.Should().Not.Be.SameInstanceAs(_target);
            clone.DefinitionCollection.Should().Not.Be.SameInstanceAs(_target.DefinitionCollection);
            clone.DefinitionCollection.Count.Should().Be.EqualTo(_target.DefinitionCollection.Count);
            clone.Id.HasValue.Should().Be.False();

            clone = (IMultiplicatorDefinitionSet)_target.Clone();
            clone.Should().Not.Be.SameInstanceAs(_target);
            clone.DefinitionCollection.Should().Not.Be.SameInstanceAs(_target.DefinitionCollection);
            clone.DefinitionCollection.Count.Should().Be.EqualTo(_target.DefinitionCollection.Count);
            clone.Id.Should().Be.EqualTo(_target.Id);
        }
    }
}
