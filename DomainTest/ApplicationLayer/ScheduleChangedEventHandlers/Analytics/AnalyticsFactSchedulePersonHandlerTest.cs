using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.Infrastructure.Repositories.Analytics;
using Teleopti.Interfaces.Infrastructure;
using Teleopti.Interfaces.Infrastructure.Analytics;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[TestFixture]
	public class AnalyticsFactSchedulePersonHandlerTest
	{
		private AnalyticsFactSchedulePersonHandler _target;
		private IAnalyticsScheduleRepository _rep;

		[SetUp]
		public void Setup()
		{
			_rep = MockRepository.GenerateMock<IAnalyticsScheduleRepository>();
			_target = new AnalyticsFactSchedulePersonHandler(_rep);
		}

		[Test]
		public void ShouldGetDataFromRepository()
		{
			var personPeriodId = Guid.NewGuid();
			_rep.Stub(x => x.PersonAndBusinessUnit(personPeriodId))
				.Return(new AnalyticsPersonBusinessUnit {BusinessUnitId = 6, PersonId = 7});

			var result =_target.Handle(personPeriodId);
			result.BusinessUnitId.Should().Be.EqualTo(6);
			result.PersonId.Should().Be.EqualTo(7);
		}

		[Test]
		public void ShouldHaveDefaultWhenNoDataFromRepository()
		{
			var personPeriodId = Guid.NewGuid();
			_rep.Stub(x => x.PersonAndBusinessUnit(personPeriodId))
				.Return(null);

			var result = _target.Handle(personPeriodId);
			result.BusinessUnitId.Should().Be.EqualTo(-1);
			result.PersonId.Should().Be.EqualTo(-1);
		}
	}
}