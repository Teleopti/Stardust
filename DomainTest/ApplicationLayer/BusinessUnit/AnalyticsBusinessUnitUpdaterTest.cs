using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.BusinessUnit;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.BusinessUnit
{
	[TestFixture]
	public class AnalyticsBusinessUnitUpdaterTest
	{
		private AnalyticsBusinessUnitUpdater _target;
		private IAnalyticsBusinessUnitRepository _analyticsBusinessUnitRepository;

		[SetUp]
		public void Setup()
		{
			_analyticsBusinessUnitRepository = new FakeAnalyticsBusinessUnitRepository {UseList = true};

			_target = new AnalyticsBusinessUnitUpdater(_analyticsBusinessUnitRepository);
		}

		[Test]
		public void ShouldAddOrUpdateBusinessUnit()
		{
			var expected = new BusinessUnitChangedEvent
			{
				BusinessUnitId = Guid.NewGuid(),
				BusinessUnitName = "TestBu",
				UpdatedOn = DateTime.UtcNow
			};
			_target.Handle(expected);

			var result = _analyticsBusinessUnitRepository.Get(expected.BusinessUnitId);
			result.Should().Not.Be.Null();

			result.BusinessUnitCode.Should().Be.EqualTo(expected.BusinessUnitId);
			result.BusinessUnitName.Should().Be.EqualTo(expected.BusinessUnitName);
			result.DatasourceUpdateDate.Should().Be.EqualTo(expected.UpdatedOn);

		}
	}
}