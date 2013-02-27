using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Optimization
{
	[TestFixture]
	public class LockableDataProviderTest
	{
		private ILockableDataProvider _target;
		private MockRepository _mocks;
		private IScheduleMatrixPro _matrix;
		private IOptimizationPreferences _optimizationPreferences;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_matrix = _mocks.StrictMock<IScheduleMatrixPro>();
			_optimizationPreferences = _mocks.StrictMock<IOptimizationPreferences>();
			_target = new LockableDataProvider(new List<IScheduleMatrixPro> { _matrix }, _optimizationPreferences);
		}

		[Test]
		public void ShouldProvideLockableData()
		{
			var lockableData = _target.Provide();

			Assert.That(lockableData.Data.Count, Is.EqualTo(1));
		}
	}
}
