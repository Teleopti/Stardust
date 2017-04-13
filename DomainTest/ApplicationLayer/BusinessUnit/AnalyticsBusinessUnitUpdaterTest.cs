using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.BusinessUnit;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.BusinessUnit
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsBusinessUnitUpdaterTest : ISetup
	{
		public AnalyticsBusinessUnitUpdater Target;
		public FakeAnalyticsBusinessUnitRepository AnalyticsBusinessUnitRepository;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<AnalyticsBusinessUnitUpdater>();
		}

		[Test]
		public void ShouldAddOrUpdateBusinessUnit()
		{
			AnalyticsBusinessUnitRepository.UseList = true;
			var expected = new BusinessUnitChangedEvent
			{
				BusinessUnitId = Guid.NewGuid(),
				BusinessUnitName = "TestBu",
				UpdatedOn = DateTime.UtcNow
			};
			Target.Handle(expected);

			var result = AnalyticsBusinessUnitRepository.Get(expected.BusinessUnitId);
			result.Should().Not.Be.Null();

			result.BusinessUnitCode.Should().Be.EqualTo(expected.BusinessUnitId);
			result.BusinessUnitName.Should().Be.EqualTo(expected.BusinessUnitName);
			result.DatasourceUpdateDate.Should().Be.EqualTo(expected.UpdatedOn);

		}
	}
}