using System;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Rta;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("LongRunning")]
	public class IndexMaintenanceRepositoryTest
	{
		private IIndexMaintenanceRepository _indexMaintenanceRepository;

		[SetUp]
		public void Setup()
		{
			_indexMaintenanceRepository = new IndexMaintenanceRepository(new TestConnectionStrings());
			_indexMaintenanceRepository.SetTimespanBetweenRetries(TimeSpan.FromSeconds(1));
		}

		[Test]
		public void ShouldWorkToRunIndexForAnalytics()
		{
			_indexMaintenanceRepository.PerformIndexMaintenance(DatabaseEnum.Analytics);
		}

		[Test]
		public void ShouldWorkToRunIndexForApplication()
		{
			_indexMaintenanceRepository.PerformIndexMaintenance(DatabaseEnum.Application);
		}

		[Test]
		public void ShouldWorkToRunIndexForAgg()
		{
			_indexMaintenanceRepository.PerformIndexMaintenance(DatabaseEnum.Agg);
		}
	}
}