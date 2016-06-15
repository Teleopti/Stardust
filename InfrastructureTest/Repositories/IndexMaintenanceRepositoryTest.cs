using System;
using System.Data.SqlClient;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Rta;
using Teleopti.Ccc.TestCommon;

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

		[Test]
		public void ShouldRetry()
		{
			var mock = MockRepository.GenerateMock<IConnectionStrings>();
			mock.Stub(x => x.Application()).Return(InfraTestConfigReader.InvalidConnectionString);
			var repository = new IndexMaintenanceRepository(mock);
			repository.SetTimespanBetweenRetries(TimeSpan.FromSeconds(1));
			try
			{
				repository.PerformIndexMaintenance(DatabaseEnum.Application);
			}
			catch
			{
				// ignored
			}

			repository.Retries.Should().Be.EqualTo(2);
		}

		[Test, ExpectedException(typeof(SqlException))]
		public void ShouldThrowExeption()
		{
			var mock = MockRepository.GenerateMock<IConnectionStrings>();
			mock.Stub(x => x.Application()).Return(InfraTestConfigReader.InvalidConnectionString);
			var repository = new IndexMaintenanceRepository(mock);
			repository.SetTimespanBetweenRetries(TimeSpan.FromSeconds(1));
			repository.PerformIndexMaintenance(DatabaseEnum.Application);
		}
	}
}