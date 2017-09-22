using System;
using System.Data.SqlClient;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Infrastructure;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture]
	[Category("BucketB")]
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

		[Test, Ignore("Not correct permission to do this on build server")]
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

		[Test]
		public void ShouldThrowException()
		{
			var mock = MockRepository.GenerateMock<IConnectionStrings>();
			mock.Stub(x => x.Application()).Return(InfraTestConfigReader.InvalidConnectionString);
			var repository = new IndexMaintenanceRepository(mock);
			repository.SetTimespanBetweenRetries(TimeSpan.FromSeconds(1));
			Assert.Throws<SqlException>(() => repository.PerformIndexMaintenance(DatabaseEnum.Application));
		}

		[TestCase("Data Source=.;Initial Catalog=TeleoptiAnalyics", ExpectedResult = "Data Source=.;Initial Catalog=AggName")]
		[TestCase("Data Source=.;Initial Catalog=A", ExpectedResult = "Data Source=.;Initial Catalog=AggName")]
		[TestCase("Data Source=.;Initial Catalog=TeleoptiAnalyics;", ExpectedResult = "Data Source=.;Initial Catalog=AggName;")]
		[TestCase("Initial Catalog=TeleoptiAnalyics;Data Source=.;", ExpectedResult = "Initial Catalog=AggName;Data Source=.;")]
		[TestCase("Data Source=tssccdv,1533;User Id=teleopticcc;Password=123;Initial Catalog=TeleoptiAnalyics;Application Name=Teleopti.wfm.etl.service", ExpectedResult = "Data Source=tssccdv,1533;User Id=teleopticcc;Password=123;Initial Catalog=AggName;Application Name=Teleopti.wfm.etl.service")]
		public string ShoudTransformConnectionStringCorrectly(string connectionString)
		{
			return IndexMaintenanceRepository.GetAggConnectionString("AggName", connectionString);
		}
	}
}