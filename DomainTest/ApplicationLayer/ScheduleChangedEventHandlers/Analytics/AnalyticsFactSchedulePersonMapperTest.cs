using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[TestFixture]
	[DomainTest]
	public class AnalyticsFactSchedulePersonMapperTest : ISetup
	{
		public IAnalyticsFactSchedulePersonMapper Target;
		public FakeAnalyticsPersonPeriodRepository AnalyticsPersonPeriods;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<FakeAnalyticsPersonPeriodRepository>();
		}

		[Test]
		public void ShouldGetDataFromRepository()
		{
			var personPeriodId = Guid.NewGuid();
			AnalyticsPersonPeriods.AddPersonPeriod(new AnalyticsPersonPeriod {BusinessUnitId = 6, PersonId = 7, PersonPeriodCode = personPeriodId});

			var result = Target.Map(personPeriodId);
			result.BusinessUnitId.Should().Be.EqualTo(6);
			result.PersonId.Should().Be.EqualTo(7);
		}

		[Test]
		public void ShouldHaveDefaultWhenNoDataFromRepository()
		{
			var personPeriodId = Guid.NewGuid();

			var result = Target.Map(personPeriodId);
			result.BusinessUnitId.Should().Be.EqualTo(-1);
			result.PersonId.Should().Be.EqualTo(-1);
		}
		
	}
}