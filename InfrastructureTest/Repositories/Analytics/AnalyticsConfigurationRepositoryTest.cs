using System.Globalization;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon.TestData.Analytics;
using Teleopti.Ccc.TestCommon.TestData.Core;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("BucketB")]
	[AnalyticsDatabaseTest]
	public class AnalyticsConfigurationRepositoryTest
	{
		public IAnalyticsConfigurationRepository Target;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		private AnalyticsDataFactory analyticsDataFactory;

		[Test]
		public void NoCultureInDatabaseReturnsCurrentCulture()
		{
			var expected = CultureInfo.CurrentCulture;

			var result = WithAnalyticsUnitOfWork.Get(() => Target.GetCulture());

			result.Name.Should().Be.EqualTo(expected.Name);
		}

		[Test]
		public void EnglishCultureCanBeMapped()
		{
			var expected = persistCulture(new CultureInfo("en-US"));

			var result = WithAnalyticsUnitOfWork.Get(() => Target.GetCulture());

			result.Name.Should().Be.EqualTo(expected.Name);
		}

		[Test]
		public void SwedishCultureCanBeMapped()
		{
			var expected = persistCulture(new CultureInfo("sv-SE"));

			var result = WithAnalyticsUnitOfWork.Get(() => Target.GetCulture());

			result.Name.Should().Be.EqualTo(expected.Name);
		}

		private CultureInfo persistCulture(CultureInfo culture)
		{
			analyticsDataFactory = new AnalyticsDataFactory();
			analyticsDataFactory.Setup(new SysConfiguration("Culture", culture.LCID.ToString(CultureInfo.InvariantCulture)));
			analyticsDataFactory.Persist();
			return culture;
		}
	}
}