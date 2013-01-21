using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization.ShiftCategoryFairness;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.ShiftCategoryFairness
{
	[TestFixture]
	public class ShiftCategoryFairnessPersonsSwappableCheckerTest
	{
		private MockRepository _mocks;
		private ShiftCategoryFairnessPersonsSwappableChecker _target;
		private IShiftCategoryFairnessPersonsSkillChecker _sameSkillChecker;
	    private IShiftCategoryFairnessRuleSetChecker _ruleSetChecker;
	    private IShiftCategoryFairnessContractTimeChecker _contractTimeChecker;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_sameSkillChecker = _mocks.DynamicMock<IShiftCategoryFairnessPersonsSkillChecker>();
		    _ruleSetChecker = _mocks.DynamicMock<IShiftCategoryFairnessRuleSetChecker>();
		    _contractTimeChecker = _mocks.DynamicMock<IShiftCategoryFairnessContractTimeChecker>();
			_target = new ShiftCategoryFairnessPersonsSwappableChecker(_sameSkillChecker, _ruleSetChecker, _contractTimeChecker);
		}

		[Test]
		public void ShouldReturnFalseIfNoPersonPeriod()
		{
			var onDate = new DateOnly(2012, 10, 3);
			var person1 = _mocks.DynamicMock<IPerson>();
			var person2 = _mocks.DynamicMock<IPerson>();
			var period = _mocks.DynamicMock<IPersonPeriod>();
		    var scheduleDays = _mocks.DynamicMock<List<IScheduleDay>>();
			Expect.Call(person1.Period(onDate)).Return(period);
			Expect.Call(person2.Period(onDate)).Return(null);
			_mocks.ReplayAll();
			Assert.That(_target.PersonsAreSwappable(person1, person2, onDate, scheduleDays, true), Is.False);
			_mocks.VerifyAll();
		}

		[Test]
		public void ShouldReturnFalseIfNotSameSkills()
		{
			var onDate = new DateOnly(2012, 10, 3);
			var person1 = _mocks.DynamicMock<IPerson>();
			var person2 = _mocks.DynamicMock<IPerson>();
            var period = _mocks.DynamicMock<IPersonPeriod>();
            var scheduleDays = _mocks.DynamicMock<List<IScheduleDay>>();
			Expect.Call(person1.Period(onDate)).Return(period);
			Expect.Call(person2.Period(onDate)).Return(period);
			Expect.Call(_sameSkillChecker.PersonsHaveSameSkills(period, period)).Return(false);
			_mocks.ReplayAll();
			Assert.That(_target.PersonsAreSwappable(person1, person2, onDate, scheduleDays, true), Is.False);
			_mocks.VerifyAll();
		}

        [Test]
        public void ShouldReturnFalseIfNoShiftRuleSetInCommon()
        {
            var onDate = new DateOnly(2012, 10, 3);
            var person1 = _mocks.DynamicMock<IPerson>();
            var person2 = _mocks.DynamicMock<IPerson>();
            var period = _mocks.DynamicMock<IPersonPeriod>();
            var scheduleDays = _mocks.DynamicMock<List<IScheduleDay>>();
            Expect.Call(person1.Period(onDate)).Return(period);
            Expect.Call(person2.Period(onDate)).Return(period);
            Expect.Call(_sameSkillChecker.PersonsHaveSameSkills(period, period)).Return(true);
            Expect.Call(_ruleSetChecker.Check(period, period)).Return(false);
            _mocks.ReplayAll();
            Assert.That(_target.PersonsAreSwappable(person1, person2, onDate, scheduleDays, true), Is.False);
            _mocks.VerifyAll();
        }

        [Test]
        public void ShouldReturnFalseIfNotSameContractTime()
        {
            var onDate = new DateOnly(2012, 10, 3);
            var person1 = _mocks.DynamicMock<IPerson>();
            var person2 = _mocks.DynamicMock<IPerson>();
            var period = _mocks.DynamicMock<IPersonPeriod>();
            var scheduleDayOne = _mocks.DynamicMock<IScheduleDay>();
            var scheduleDayTwo = _mocks.DynamicMock<IScheduleDay>();
            var scheduleDays = new List<IScheduleDay>{scheduleDayOne, scheduleDayTwo};

            Expect.Call(person1.Period(onDate)).Return(period);
            Expect.Call(person2.Period(onDate)).Return(period);
            Expect.Call(_sameSkillChecker.PersonsHaveSameSkills(period, period)).Return(true);
            Expect.Call(_ruleSetChecker.Check(period, period)).Return(true);
            Expect.Call(_contractTimeChecker.Check(scheduleDays.First(), scheduleDays.Last())).Return(false);
            _mocks.ReplayAll();
            Assert.That(_target.PersonsAreSwappable(person1, person2, onDate, scheduleDays, true), Is.False);
            _mocks.VerifyAll();
        }
	}
}