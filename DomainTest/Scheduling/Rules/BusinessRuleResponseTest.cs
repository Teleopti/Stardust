using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.DomainTest.Scheduling.Rules
{
    /// <summary>
    /// Tests for the BusinessRuleResponse class
    /// </summary>
    /// <remarks>
    /// Created by: robink
    /// Created date: 2007-11-08
    /// </remarks>
    [TestFixture]
    public class BusinessRuleResponseTest
    {
        private BusinessRuleResponse _target;
        private const string Message = "An error has occurred!";
        private DateOnly _dateOnly;
        private DateOnlyPeriod _dateOnlyPeriod;
        private IPerson _person;   
        /// <summary>
        /// Setups this instance.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-08
        /// </remarks>
        [SetUp]
        public void Setup()
        {
            _person = new Person();
            var start = new DateTime(2007,1,1,0,0,0,DateTimeKind.Utc);
            _dateOnly = new DateOnly(2007, 1, 1);
            _dateOnlyPeriod = new DateOnlyPeriod(_dateOnly, _dateOnly);
            _target = new BusinessRuleResponse(typeof(string), Message, true, false, new DateTimePeriod(start, start), _person, _dateOnlyPeriod, "tjillevippen");
            
        }

        /// <summary>
        /// Verifies the default properties are set.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-08
        /// </remarks>
        [Test]
        public void VerifyDefaultPropertiesAreSet()
        {
            Assert.IsNotNull(_target);
            Assert.AreEqual(typeof (string), _target.TypeOfRule);
            Assert.AreEqual(Message, _target.Message);
            Assert.IsTrue(_target.Error);
            Assert.IsFalse(_target.Mandatory);
            Assert.IsFalse(_target.Overridden);
            Assert.IsNotNull(_target.Period);
            Assert.IsNotNull(_target.Person);
        }

        /// <summary>
        /// Verifies the business rule response can be overriden.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2007-11-08
        /// </remarks>
        [Test]
        public void VerifyBusinessRuleResponseCanBeOverridden()
        {
            Assert.IsFalse(_target.Overridden);
            _target.Overridden = true;
            Assert.IsTrue(_target.Overridden);
        }

        [Test]
        public void VerifyCanBeUsedInHashSet()
        {
            var start = new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _target = new BusinessRuleResponse(typeof(string), Message, true, false, new DateTimePeriod(start, start), _person, _dateOnlyPeriod, "tjillevippen");
            IBusinessRuleResponse same = new BusinessRuleResponse(typeof(string), Message, true, false, new DateTimePeriod(start, start), _person, _dateOnlyPeriod, "tjillevippen");
            var list = new HashSet<IBusinessRuleResponse> {_target, same};
            Assert.AreEqual(1, list.Count);
        }

        [Test]
        public void VerifyDifferentTypeBreaksEqual()
        {
            var start = new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _target = new BusinessRuleResponse(typeof(string), Message, true, false, new DateTimePeriod(start, start), _person, _dateOnlyPeriod, "tjillevippen");
            IBusinessRuleResponse another = new BusinessRuleResponse(typeof(int), Message, true, false, new DateTimePeriod(start, start), _person, _dateOnlyPeriod, "tjillevippen");
            Assert.AreNotEqual(_target, another);
        }

        [Test]
        public void VerifyDifferentPersonBreaksEqual()
        {
            var start = new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _target = new BusinessRuleResponse(typeof(string), Message, true, false, new DateTimePeriod(start, start), _person, _dateOnlyPeriod, "tjillevippen");
            IBusinessRuleResponse another = new BusinessRuleResponse(typeof(string), Message, true, false, new DateTimePeriod(start, start), PersonFactory.CreatePerson(), _dateOnlyPeriod, "tjillevippen");
            Assert.AreNotEqual(_target, another);
        }

        [Test]
        public void VerifyDifferentPeriodBreaksEqual()
        {
            var start = new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _target = new BusinessRuleResponse(typeof(string), Message, true, false, new DateTimePeriod(start, start), _person, _dateOnlyPeriod, "tjillevippen");
            IBusinessRuleResponse another = new BusinessRuleResponse(typeof(string), Message, true, false, new DateTimePeriod(start, start.AddHours(1)), _person, _dateOnlyPeriod, "tjillevippen");
            Assert.AreNotEqual(_target, another);
        }

        [Test]
        public void VerifyCanBeUsedWithContains()
        {
            var start = new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _target = new BusinessRuleResponse(typeof(string), Message, true, false, new DateTimePeriod(start, start), _person, _dateOnlyPeriod, "tjillevippen");
            IBusinessRuleResponse another = new BusinessRuleResponse(typeof(string), Message, true, false, new DateTimePeriod(start, start.AddHours(1)), _person, _dateOnlyPeriod, "tjillevippen");
            IList<IBusinessRuleResponse> list = new List<IBusinessRuleResponse>{_target, another};
            _target = new BusinessRuleResponse(typeof(string), Message, true, false, new DateTimePeriod(start, start), _person, _dateOnlyPeriod, "tjillevippen");
            another = new BusinessRuleResponse(typeof(int), Message, true, false, new DateTimePeriod(start, start.AddHours(1)), _person, _dateOnlyPeriod, "tjillevippen");
            Assert.IsTrue(list.Contains(_target));
            Assert.IsFalse(list.Contains(another));
        }

        [Test]
        public void VerifyCanBeUsedWithRemove()
        {
            var start = new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _target = new BusinessRuleResponse(typeof(NewShiftCategoryLimitationRule), Message, true, false, new DateTimePeriod(start, start), _person, _dateOnlyPeriod, "tjillevippen");
            IBusinessRuleResponse another = new BusinessRuleResponse(typeof(NewNightlyRestRule), Message, true, false, new DateTimePeriod(start, start), _person, _dateOnlyPeriod, "tjillevippen");
            IList<IBusinessRuleResponse> list = new List<IBusinessRuleResponse> { _target, another };
            IBusinessRuleResponse shouldBeSameAsTarget = new BusinessRuleResponse(typeof(NewShiftCategoryLimitationRule), Message, true, false, new DateTimePeriod(start, start), _person, _dateOnlyPeriod, "tjillevippen");

            list.Remove(shouldBeSameAsTarget);

            Assert.AreEqual(1, list.Count);
            Assert.IsTrue(list.Contains(another));
        }

        [Test]
        public void ShouldNotRemoveWhenDateOnlyPeriodDiffer()
        {
            var start = new DateTime(2007, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            _target = new BusinessRuleResponse(typeof(NewNightlyRestRule), Message, true, false, new DateTimePeriod(start, start), _person, _dateOnlyPeriod, "tjillevippen");
            IBusinessRuleResponse another = new BusinessRuleResponse(typeof(NewNightlyRestRule), Message, true, false, new DateTimePeriod(start, start), _person,_dateOnlyPeriod, "tjillevippen");
            IList<IBusinessRuleResponse> list = new List<IBusinessRuleResponse> { _target, another };
            IBusinessRuleResponse shouldNotBeSameAsTarget = new BusinessRuleResponse(typeof(NewNightlyRestRule), Message, true, false, new DateTimePeriod(start, start), _person, new DateOnlyPeriod(_dateOnly.AddDays(1), _dateOnly.AddDays(1)), "tjillevippen");

            list.Remove(shouldNotBeSameAsTarget);

            Assert.AreEqual(2, list.Count);
            Assert.IsTrue(list.Contains(another));
        }
    }
}