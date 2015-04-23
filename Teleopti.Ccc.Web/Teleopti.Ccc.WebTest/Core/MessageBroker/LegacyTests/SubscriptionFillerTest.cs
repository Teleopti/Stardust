using System;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.Web.Core.MessageBroker;
using Teleopti.Interfaces.MessageBroker;

namespace Teleopti.Ccc.WebTest.Core.MessageBroker.LegacyTests
{
	[TestFixture]
	public class SubscriptionFillerTest
	{
		[Test]
		public void ShouldFillSubscriptionWithDatasource()
		{
			var target = new SubscriptionFiller(
				new FakeCurrentDatasource("data source"),
				new SpecificBusinessUnit(new BusinessUnit("unit")));
			var subscription = new Subscription();

			target.Invoke(subscription);

			subscription.DataSource.Should().Be.EqualTo("data source");
		}

		[Test]
		public void ShouldFillSubscriptionWithBusinessUnitId()
		{
			var businessUnit = new BusinessUnit("unit");
			businessUnit.SetId(Guid.NewGuid());
			var target = new SubscriptionFiller(MockRepository.GenerateMock<ICurrentDataSource>(), new SpecificBusinessUnit(businessUnit));
			var subscription = new Subscription();
		
			target.Invoke(subscription);

			subscription.BusinessUnitId.Should().Be.EqualTo(businessUnit.Id.Value.ToString());
		}
	}
}