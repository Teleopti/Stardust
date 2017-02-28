using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
		private OverLimitResults _overLimitResults;
		private IScheduleMatrixPro _scheduleMatrixPro;
		
		[SetUp]
		public void SetUp()
		{
			_mock = new MockRepository();
			_overLimitByRestrictionDecider = _mock.StrictMock<IOptimizationOverLimitByRestrictionDecider>();
			_target = new OptimizationLimits(_overLimitByRestrictionDecider);
			_overLimitResults = new OverLimitResults(100, 100, 100, 100, 100);
			_scheduleMatrixPro = _mock.StrictMock<IScheduleMatrixPro>();
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
