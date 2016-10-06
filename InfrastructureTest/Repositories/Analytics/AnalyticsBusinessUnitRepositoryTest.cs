using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{

	[TestFixture]
	[Category("BucketB")]
	[AnalyticsDatabaseTest]
	public class AnalyticsBusinessUnitRepositoryTest
	{
		public IAnalyticsBusinessUnitRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;

		[Test]
		public void ShouldGetBusinessUnitByCode()
		{
			var expected = new AnalyticBusinessUnit
			{
				BusinessUnitCode = Guid.NewGuid(),
				BusinessUnitName = "TestBu",
				DatasourceUpdateDate = DateTime.UtcNow.Truncate(TimeSpan.FromMinutes(1))
			};
			WithAnalyticsUnitOfWork.Do(() => Target.AddOrUpdate(expected));

			var result = WithAnalyticsUnitOfWork.Get(() => Target.Get(expected.BusinessUnitCode));

			result.BusinessUnitId.Should().Be.GreaterThan(0);
			result.DatasourceId.Should().Be.EqualTo(1);
			result.BusinessUnitCode.Should().Be.EqualTo(expected.BusinessUnitCode);
			result.BusinessUnitName.Should().Be.EqualTo(expected.BusinessUnitName);
			result.DatasourceUpdateDate.Should().Be.EqualTo(expected.DatasourceUpdateDate);
		}

		[Test]
		public void ShouldReturnNullForNotExistingBusinessUnit()
		{
			var result = WithAnalyticsUnitOfWork.Get(() => Target.Get(Guid.NewGuid()));

			result.Should().Be.Null();
		}
	}
}