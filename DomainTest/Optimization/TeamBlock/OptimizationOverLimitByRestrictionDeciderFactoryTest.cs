using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization.TeamBlock
{
	[TestFixture]
	public class OptimizationOverLimitByRestrictionDeciderFactoryTest
	{
		private IOptimizationOverLimitByRestrictionDeciderFactory _target;
		private MockRepository _mocks;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_target = new OptimizationOverLimitByRestrictionDeciderFactory();
		}

		[Test]
		public void ShouldReturnOptimizationOverLimitByRestrictionDecider()
		{
			var scheduleMatrix = _mocks.StrictMock<IScheduleMatrixPro>();
			var scheduleMatrixOriginalStateContainer = _mocks.StrictMock<IScheduleMatrixOriginalStateContainer>();

			var result = _target.CreateOptimizationOverLimitByRestrictionDecider(scheduleMatrix, new RestrictionChecker(),
			                                                                     new OptimizationPreferences(),
			                                                                     scheduleMatrixOriginalStateContainer);
			Assert.That(result, Is.TypeOf(typeof (OptimizationOverLimitByRestrictionDecider)));
		}
	}
}
