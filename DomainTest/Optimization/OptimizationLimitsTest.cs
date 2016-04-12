using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class OptimizationLimitsTest
	{
		private MockRepository _mock;
		private OptimizationLimits _target;
		private IOptimizationOverLimitByRestrictionDecider _overLimitByRestrictionDecider;
		private INewBusinessRule _minWeekWorkTimeRule;
		private OverLimitResults _overLimitResults;
		private IDictionary<IPerson, IScheduleRange> _dictionary;
		private IScheduleMatrixPro _scheduleMatrixPro;
		private IPerson _person;
		private IScheduleRange _scheduleRange;
		private ReadOnlyCollection<IScheduleDayPro> _effectiveDays;
		private IScheduleDayPro _scheduleDayPro;
		private IScheduleDay _scheduleDay;
		private IBusinessRuleResponse _businessRuleResponse;
		
		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_overLimitByRestrictionDecider = _mock.StrictMock<IOptimizationOverLimitByRestrictionDecider>();
			_minWeekWorkTimeRule = _mock.StrictMock<INewBusinessRule>();
			_target = new OptimizationLimits(_overLimitByRestrictionDecider);
			_overLimitResults = new OverLimitResults(100, 100, 100, 100, 100);
			_scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
			_person = _mock.StrictMock<IPerson>();
			_scheduleRange = _mock.StrictMock<IScheduleRange>();
			_dictionary = new Dictionary<IPerson, IScheduleRange> { { _person, _scheduleRange } };
			_scheduleDayPro = _mock.StrictMock<IScheduleDayPro>();
			_effectiveDays = new ReadOnlyCollection<IScheduleDayPro>(new List<IScheduleDayPro>{_scheduleDayPro});
			_scheduleDay = _mock.StrictMock<IScheduleDay>();
			_businessRuleResponse = _mock.StrictMock<IBusinessRuleResponse>();
		}

		[Test]
		public void ShouldReturnTrueWhenOverLimitIncreased()
		{
			using (_mock.Record())
			{
				Expect.Call(_overLimitByRestrictionDecider.HasOverLimitIncreased(_overLimitResults, _scheduleMatrixPro)).Return(true);
			}

			using (_mock.Playback())
			{
				var result = _target.HasOverLimitExceeded(_overLimitResults, _scheduleMatrixPro);
				Assert.IsTrue(result);
			}	
		}

		[Test]
		public void ShouldReturnFalseWhenOverLimitDecreased()
		{
			using (_mock.Record())
			{
				Expect.Call(_overLimitByRestrictionDecider.HasOverLimitIncreased(_overLimitResults, _scheduleMatrixPro)).Return(false);
			}

			using (_mock.Playback())
			{
				var result = _target.HasOverLimitExceeded(_overLimitResults, _scheduleMatrixPro);
				Assert.IsFalse(result);
			}
		}

		[Test]
		public void ShouldReturnOverLimitsResult()
		{
			using (_mock.Record())
			{
				Expect.Call(_overLimitByRestrictionDecider.OverLimitsCounts(_scheduleMatrixPro)).Return(_overLimitResults);
			}

			using (_mock.Playback())
			{
				var result = _target.OverLimitsCounts(_scheduleMatrixPro);
				Assert.AreEqual(_overLimitResults, result);
			}
		}

		[Test]
		public void ShouldMoveMaxDaysOverLimit()
		{
			using (_mock.Record())
			{
				Expect.Call(_overLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(true);
			}

			using (_mock.Playback())
			{
				var result = _target.MoveMaxDaysOverLimit();
				Assert.IsTrue(result);
			}
		}

		[Test]
		public void ShouldNotMoveMaxDaysOverLimit()
		{
			using (_mock.Record())
			{
				Expect.Call(_overLimitByRestrictionDecider.MoveMaxDaysOverLimit()).Return(false);
			}

			using (_mock.Playback())
			{
				var result = _target.MoveMaxDaysOverLimit();
				Assert.IsFalse(result);
			}
		}
	}
}
