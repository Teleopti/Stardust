using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[Category("BucketB")]
	[TestFixture]
	[AnalyticsDatabaseTest]
	public class AnalyticsTimeZoneRepositoryTest
	{
		public IAnalyticsTimeZoneRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;

		[SetUp]
		public void Setup()
		{
			var timeZones = new UtcAndCetTimeZones();
			var analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(timeZones);
			analyticsDataFactory.Persist();
		}

		[Test]
		public void ShouldGet()
		{
			
			var result = WithAnalyticsUnitOfWork.Get(() => Target.Get("W. Europe Standard Time"));
			result.TimeZoneId.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldGetAll()
		{
			var result = WithAnalyticsUnitOfWork.Get(() => Target.GetAll());
			result.Should().Not.Be.Empty();
			result.Where(x => x.TimeZoneCode == "W. Europe Standard Time").Should().Not.Be.Empty();
			result.Where(x => x.TimeZoneCode == "UTC").Should().Not.Be.Empty();
		}
	}
}