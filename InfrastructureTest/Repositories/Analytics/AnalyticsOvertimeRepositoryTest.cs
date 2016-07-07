using NUnit.Framework;
using System;
using System.Linq;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsDatabaseTest]
	public class AnalyticsOvertimeRepositoryTest
	{
		public IAnalyticsOvertimeRepository Target;

		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;
		private const int businessUnitId = 12;

		[Test]
		public void ShouldAddOvertime()
		{
			var expected = new AnalyticsOvertime
			{
				BusinessUnitId = businessUnitId,
				DatasourceUpdateDate = DateTime.UtcNow,
				IsDeleted = false,
				OvertimeCode = Guid.NewGuid(),
				OvertimeName = "Test"
			};

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddOrUpdate(expected);
			});

			var overtimes = WithAnalyticsUnitOfWork.Get(() => Target.Overtimes());

			overtimes.Should().Not.Be.Empty();
			var actual = overtimes.Single();
			actual.IsDeleted.Should().Be.EqualTo(expected.IsDeleted);
			actual.OvertimeCode.Should().Be.EqualTo(expected.OvertimeCode);
			actual.OvertimeName.Should().Be.EqualTo(expected.OvertimeName);
		}

		[Test]
		public void ShouldUpdateOvertime()
		{
			var original = new AnalyticsOvertime
			{
				BusinessUnitId = businessUnitId,
				DatasourceUpdateDate = DateTime.UtcNow- TimeSpan.FromSeconds(1),
				IsDeleted = false,
				OvertimeCode = Guid.NewGuid(),
				OvertimeName = "Test"
			};

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddOrUpdate(original);
			});

			var expected = new AnalyticsOvertime
			{
				BusinessUnitId = businessUnitId,
				DatasourceUpdateDate = DateTime.UtcNow,
				IsDeleted = true,
				OvertimeCode = original.OvertimeCode,
				OvertimeName = "Changed"
			};

			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.AddOrUpdate(expected);
			});

			var overtimes = WithAnalyticsUnitOfWork.Get(() => Target.Overtimes());

			overtimes.Should().Not.Be.Empty();
			var actual = overtimes.Single();
			actual.IsDeleted.Should().Be.EqualTo(expected.IsDeleted);
			actual.OvertimeCode.Should().Be.EqualTo(expected.OvertimeCode);
			actual.OvertimeName.Should().Be.EqualTo(expected.OvertimeName);
		}

	}
}