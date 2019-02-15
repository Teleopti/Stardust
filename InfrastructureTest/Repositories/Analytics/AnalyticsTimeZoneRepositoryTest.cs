using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.IoC;
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
			var gmtTimeZone = new SpecificTimeZone(TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"), 3);
			var brasilTimeZone = new ATimeZone("E. South America Standard Time") { TimeZoneId = 2 };
			var analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(brasilTimeZone);
			analyticsDataFactory.Setup(new DataSource { DataSourceId = 2, TimeZoneId = brasilTimeZone.TimeZoneId });
			analyticsDataFactory.Setup(new SysConfiguration("TimeZoneCode", "GMT Standard Time"));
			analyticsDataFactory.Setup(timeZones);
			analyticsDataFactory.Setup(gmtTimeZone);
			analyticsDataFactory.Persist();
		}
		
		[Test]
		public void ShouldGet()
		{
			var result = WithAnalyticsUnitOfWork.Get(() => Target.Get("W. Europe Standard Time"));
			result.TimeZoneId.Should().Be.EqualTo(1);
			result.IsUtcInUse.Should().Be.False();
			result.ToBeDeleted.Should().Be.False();
		}
		
		[Test]
		public void ShouldGetAll()
		{
			var result = WithAnalyticsUnitOfWork.Get(() => Target.GetAll());
			result.Should().Not.Be.Empty();
			result.Where(x => x.TimeZoneCode == "W. Europe Standard Time").Should().Not.Be.Empty();
			result.Where(x => x.TimeZoneCode == "UTC").Should().Not.Be.Empty();
		}
		
		[Test]
		public void ShouldUpdateUtcInUse()
		{
			WithAnalyticsUnitOfWork.Do(() => Target.SetUtcInUse(true));
			var result = WithAnalyticsUnitOfWork.Get(() => Target.Get("UTC"));
			result.IsUtcInUse.Should().Be.True();
		}
		
		[Test]
		public void ShouldMarkDeletedTimeZones()
		{
			var result = WithAnalyticsUnitOfWork.Get(() => Target.Get("W. Europe Standard Time"));
			result.ToBeDeleted.Should().Be.False();
			WithAnalyticsUnitOfWork.Do(() => Target.SetToBeDeleted("W. Europe Standard Time", true));
			result = WithAnalyticsUnitOfWork.Get(() => Target.Get("W. Europe Standard Time"));
			result.ToBeDeleted.Should().Be.True();
		}
	}
	
	[Category("BucketB")]
	[TestFixture]
	[AnalyticsDatabaseTest]
	public class AnalyticsTimeZoneRepositoryForBug47311Test
	{
		public IAnalyticsTimeZoneRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;

		[SetUp]
		public void Setup()
		{
			var timeZones = new UtcAndCetTimeZones();
			var gmtTimeZone = new SpecificTimeZone(TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time"), 3);
			var brasilTimeZone = new ATimeZone("E. South America Standard Time") { TimeZoneId = 2 };
			var analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(brasilTimeZone);
			analyticsDataFactory.Setup(new DataSource { DataSourceId = 2, TimeZoneId = brasilTimeZone.TimeZoneId });
			analyticsDataFactory.Setup(new SysConfiguration("TimeZoneCode", "GMT Standard Time"));
			analyticsDataFactory.Setup(timeZones);
			analyticsDataFactory.Setup(gmtTimeZone);
			analyticsDataFactory.Persist();
		}

		[Test]
		public void ShouldGetAllTimeZonesUsedByDataSourceAndBaseConfiguration()
		{
			var result = WithAnalyticsUnitOfWork.Get(() => Target.GetAllUsedByLogDataSourcesAndBaseConfig());
			result.Should().Not.Be.Empty();
			result.Count.Should().Be(2);
			result.Any(x => x.TimeZoneCode == "E. South America Standard Time").Should().Be.True();
			result.Any(x => x.TimeZoneCode == "GMT Standard Time").Should().Be.True();
		}
	}
}
 