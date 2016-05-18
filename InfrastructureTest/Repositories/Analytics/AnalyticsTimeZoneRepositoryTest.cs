using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[Category("LongRunning")]
	[TestFixture]
	[AnalyticsUnitOfWorkTest]
	public class AnalyticsTimeZoneRepositoryTest
	{
		public ICurrentAnalyticsUnitOfWork UnitOfWork;
		public IAnalyticsTimeZoneRepository Target;

		[Test]
		public void ShouldGet()
		{
			var timeZones = new UtcAndCetTimeZones();
			var analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(timeZones);
			analyticsDataFactory.Persist();

			var result = Target.Get("W. Europe Standard Time");
			result.TimeZoneId.Should().Be.EqualTo(1);
		}
	}
}