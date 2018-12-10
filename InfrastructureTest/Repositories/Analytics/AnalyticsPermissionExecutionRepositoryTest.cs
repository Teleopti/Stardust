using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.UnitOfWork;


namespace Teleopti.Ccc.InfrastructureTest.Repositories.Analytics
{
	[TestFixture]
	[Category("BucketB")]
	[AnalyticsDatabaseTest]
	public class AnalyticsPermissionExecutionRepositoryTest
	{
		public ICurrentAnalyticsUnitOfWork UnitOfWork;
		public IAnalyticsPermissionExecutionRepository Target;
		public MutableNow Now;
		public WithAnalyticsUnitOfWork WithAnalyticsUnitOfWork;

		[Test]
		public void ShouldReturnDatetimeMinWhenNotExists()
		{
			
			var result = WithAnalyticsUnitOfWork.Get(() => Target.Get(Guid.NewGuid(), 0));

			result.Should().Be.EqualTo(DateTime.MinValue);
			
		}

		[Test]
		public void ShouldReturnValueWhenExists()
		{
			var personId = Guid.NewGuid();
			const int businessUnitId = 0;
			var expectedDate = DateHelper.GetSmallDateTime(DateTime.UtcNow);
			Now.Is(expectedDate);
			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.Set(personId, businessUnitId);
			});

			var result = WithAnalyticsUnitOfWork.Get(() => Target.Get(personId, businessUnitId));

			result.Should().Be.EqualTo(expectedDate);
		}

		[Test]
		public void ShouldReturnUpdatedValueWhenUpdated()
		{
			var personId = Guid.NewGuid();
			const int businessUnitId = 0;
			var firstDate = new DateTime(1986, 03, 07, 11, 15, 0);
			var expectedDate = DateHelper.GetSmallDateTime(DateTime.UtcNow);
			Now.Is(firstDate);
			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.Set(personId, businessUnitId);
			});
			var result1 = WithAnalyticsUnitOfWork.Get(() => Target.Get(personId, businessUnitId));
			Now.Is(expectedDate);
			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.Set(personId, businessUnitId);
			});
			var result = WithAnalyticsUnitOfWork.Get(() => Target.Get(personId, businessUnitId));

			result1.Should().Be.EqualTo(firstDate);
			result.Should().Be.EqualTo(expectedDate);
		}

		[Test]
		public void ShouldReturnDatetimeMinWhenExistsForOtherBusinessUnit()
		{
			var personId = Guid.NewGuid();
			WithAnalyticsUnitOfWork.Do(() =>
			{
				Target.Set(personId, 0);
			});

			var result = WithAnalyticsUnitOfWork.Get(() => Target.Get(personId, 1));

			result.Should().Be.EqualTo(DateTime.MinValue);

		}
	}
}