using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsDatabaseTest]
	public class AnalyticsPermissionExecutionRepositoryTest
	{
		ICurrentDataSource currentDataSource;
		INow now;

		[SetUp]
		public void SetUp()
		{
			currentDataSource = CurrentDataSource.Make();
			now = MockRepository.GenerateMock<INow>();
		}

		[Test]
		public void ShouldReturnDatetimeMinWhenNotExists()
		{
			var target = new AnalyticsPermissionExecutionRepository(currentDataSource, now);
			var result = target.Get(Guid.NewGuid());

			result.Should().Be.EqualTo(DateTime.MinValue);
		}

		[Test]
		public void ShouldReturnValueWhenExists()
		{
			var target = new AnalyticsPermissionExecutionRepository(currentDataSource, now);
			var personId = Guid.NewGuid();
			var expectedDate = getSmallDateTime(DateTime.UtcNow);
			now.Stub(n => n.UtcDateTime()).Return(expectedDate);
			target.Set(personId);
			var result = target.Get(personId);

			result.Should().Be.EqualTo(expectedDate);
		}

		[Test]
		public void ShouldReturnUpdatedValueWhenUpdated()
		{
			var target = new AnalyticsPermissionExecutionRepository(currentDataSource, now);
			var personId = Guid.NewGuid();
			var firstDate = new DateTime(1986, 03, 07, 11, 15, 0);
			var expectedDate = getSmallDateTime(DateTime.UtcNow);
			now.Stub(n => n.UtcDateTime()).Return(firstDate).Repeat.Once();
			now.Stub(n => n.UtcDateTime()).Return(expectedDate).Repeat.Once();
			target.Set(personId);
			var result1 = target.Get(personId);
			target.Set(personId);
			var result = target.Get(personId);

			result1.Should().Be.EqualTo(firstDate);
			result.Should().Be.EqualTo(expectedDate);
		}

		private static DateTime getSmallDateTime(DateTime value)
		{
			return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
		}
	}
}