using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[Category("BucketB")]
	[TestFixture]
	[AnalyticsDatabaseTest]
	[Toggle(Toggles.ETL_EventbasedTimeZone_40870)]
	public class AnalyticsTimeZoneRepositoryWithCreationTest: ISetup
	{
		public IAnalyticsTimeZoneRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		public FakeEventPublisher FakeEventPublisher;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeEventPublisher>().For<IEventPublisher>();
		}

		[SetUp]
		public void Setup()
		{
			var analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(new SysConfiguration("TimeZoneCode", "W. Europe Standard Time"));
			analyticsDataFactory.Persist();
		}

		[Test]
		public void ShouldCreateMissingTimeZoneWhenLoading()
		{
			var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("W. Europe Standard Time");
			var timeZone = WithAnalyticsUnitOfWork.Get(() => Target.Get(targetTimeZone.Id));
			timeZone.TimeZoneCode.Should().Be.EqualTo(targetTimeZone.Id);

			FakeEventPublisher.PublishedEvents.Should().Not.Be.Empty();
			FakeEventPublisher.PublishedEvents.OfType<AnalyticsTimeZoneChangedEvent>().SingleOrDefault().Should().Not.Be.Null();
		}
	}
}