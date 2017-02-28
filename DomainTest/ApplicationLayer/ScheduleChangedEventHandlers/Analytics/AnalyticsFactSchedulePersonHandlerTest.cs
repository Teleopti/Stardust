using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.Analytics;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.ScheduleChangedEventHandlers.Analytics
{
	[TestFixture]
	public class AnalyticsFactSchedulePersonHandlerTest
	{
		private AnalyticsFactSchedulePersonMapper _target;
		private FakeAnalyticsPersonPeriodRepository _rep;

		[SetUp]
		public void Setup()
		{
			_rep = new FakeAnalyticsPersonPeriodRepository();
			_target = new AnalyticsFactSchedulePersonMapper(_rep);
		}

		[Test]
		public void ShouldGetDataFromRepository()
		{
			var personPeriodId = Guid.NewGuid();
			_rep.AddPersonPeriod(new AnalyticsPersonPeriod {BusinessUnitId = 6, PersonId = 7, PersonPeriodCode = personPeriodId});

			var result =_target.Map(personPeriodId);
			result.BusinessUnitId.Should().Be.EqualTo(6);
			result.PersonId.Should().Be.EqualTo(7);
		}

		[Test]
		public void ShouldHaveDefaultWhenNoDataFromRepository()
		{
			var personPeriodId = Guid.NewGuid();

			var result = _target.Map(personPeriodId);
			result.BusinessUnitId.Should().Be.EqualTo(-1);
			result.PersonId.Should().Be.EqualTo(-1);
		}
	}
}