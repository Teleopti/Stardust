using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Analytics;
using Teleopti.Ccc.Domain.Common.Time;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("LongRunning")]
	[AnalyticsUnitOfWorkTest]
	public class AnalyticsPermissionExecutionRepositoryTest
	{
		public ICurrentAnalyticsUnitOfWork UnitOfWork;
		public IAnalyticsPermissionExecutionRepository Target;
		public MutableNow Now;

		[Test]
		public void ShouldReturnDatetimeMinWhenNotExists()
		{
			var result = Target.Get(Guid.NewGuid());

			result.Should().Be.EqualTo(DateTime.MinValue);
			
		}

		[Test]
		public void ShouldReturnValueWhenExists()
		{
			var personId = Guid.NewGuid();
			var expectedDate = getSmallDateTime(DateTime.UtcNow);
			Now.Is(expectedDate);
			Target.Set(personId);
			var result = Target.Get(personId);

			result.Should().Be.EqualTo(expectedDate);
		}

		[Test]
		public void ShouldReturnUpdatedValueWhenUpdated()
		{
			var personId = Guid.NewGuid();
			var firstDate = new DateTime(1986, 03, 07, 11, 15, 0);
			var expectedDate = getSmallDateTime(DateTime.UtcNow);
			Now.Is(firstDate);
			Target.Set(personId);
			var result1 = Target.Get(personId);
			Now.Is(expectedDate);
			Target.Set(personId);
			var result = Target.Get(personId);

			result1.Should().Be.EqualTo(firstDate);
			result.Should().Be.EqualTo(expectedDate);
		}

		private static DateTime getSmallDateTime(DateTime value)
		{
			return new DateTime(value.Year, value.Month, value.Day, value.Hour, value.Minute, 0);
		}
	}
}