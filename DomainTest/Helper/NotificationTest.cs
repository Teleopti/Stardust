using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.DomainTest.Helper
{
	[TestFixture]
	public class NotificationTest
	{
		private Notification target;
		private Guid moduleId;
		private Guid domainId;
		private Guid domainReferenceId;
		private DateTime startDate;
		private DateTime endDate;
		private Guid businessUnitId;

		[SetUp]
		public void Setup()
		{
			domainId = Guid.NewGuid();
			domainReferenceId = Guid.NewGuid();
			moduleId = Guid.NewGuid();
			businessUnitId = Guid.NewGuid();
			startDate = DateTime.Today;
			endDate = startDate.AddDays(1);
			target = new Notification
			         	{
			         		BinaryData = "test",
			         		DomainId = Subscription.IdToString(domainId),
			         		DomainReferenceId = Subscription.IdToString(domainReferenceId),
			         		ModuleId = Subscription.IdToString(moduleId),
			         		DomainReferenceType = "ref",
			         		DomainType = "type",
			         		DomainUpdateType = 1,
			         		StartDate = Subscription.DateToString(startDate),
			         		EndDate = Subscription.DateToString(endDate),
							DataSource = "datasource",
							BusinessUnitId = Subscription.IdToString(businessUnitId)
			         	};
		}

		[Test]
		public void PropertiesShouldWork()
		{
			target.BinaryData.Should().Be.EqualTo("test");
			target.DomainIdAsGuid().Should().Be.EqualTo(domainId);
			target.DomainReferenceIdAsGuid().Should().Be.EqualTo(domainReferenceId);
			target.ModuleIdAsGuid().Should().Be.EqualTo(moduleId);
			target.BusinessUnitIdAsGuid().Should().Be.EqualTo(businessUnitId);
			target.DomainReferenceType.Should().Be.EqualTo("ref");
			target.DomainType.Should().Be.EqualTo("type");
			target.DomainUpdateTypeAsDomainUpdateType().Should().Be.EqualTo(DomainUpdateType.Update);
			target.StartDateAsDateTime().Should().Be.EqualTo(startDate);
			target.EndDateAsDateTime().Should().Be.EqualTo(endDate);
			target.DataSource.Should().Be.EqualTo("datasource");
		}

		[Test]
		public void ShouldHaveCorrectRoute()
		{
			var routes = target.Routes();
			routes.Should().Contain("datasource/" + target.BusinessUnitId + "/type");
			routes.Should().Contain("datasource/" + target.BusinessUnitId + "/type/ref/"+target.DomainReferenceId);
			routes.Should().Contain("datasource/" + target.BusinessUnitId + "/type/id/"+target.DomainId);
		}
	}

	[TestFixture]
	public class SubscriptionTest
	{
		private Subscription target;
		private Guid subscriptionId;
		private Guid domainId;
		private Guid domainReferenceId;
		private DateTime startDate;
		private DateTime endDate;
		private Guid businessUnitId;

		[SetUp]
		public void Setup()
		{
			domainId = Guid.NewGuid();
			domainReferenceId = Guid.NewGuid();
			subscriptionId = Guid.NewGuid();
			businessUnitId = Guid.NewGuid();
			startDate = DateTime.Today;
			endDate = startDate.AddDays(1);
			target = new Subscription
			{
				DomainId = Subscription.IdToString(domainId),
				DomainReferenceId = Subscription.IdToString(domainReferenceId),
				DomainReferenceType = "ref",
				DomainType = "type",
				LowerBoundary = Subscription.DateToString(startDate),
				UpperBoundary = Subscription.DateToString(endDate),
				DataSource = "datasource",
				BusinessUnitId = Subscription.IdToString(businessUnitId)
			};
		}

		[Test]
		public void PropertiesShouldWork()
		{
			target.DomainId.Should().Be.EqualTo(domainId.ToString());
			target.DomainReferenceId.Should().Be.EqualTo(domainReferenceId.ToString());
			target.BusinessUnitIdAsGuid().Should().Be.EqualTo(businessUnitId);
			target.DomainReferenceType.Should().Be.EqualTo("ref");
			target.DomainType.Should().Be.EqualTo("type");
			target.LowerBoundaryAsDateTime().Should().Be.EqualTo(startDate);
			target.UpperBoundaryAsDateTime().Should().Be.EqualTo(endDate);
		}

		[Test]
		public void ShouldHaveCorrectRoute()
		{
			target.Route().Should().Be.EqualTo("datasource/" + target.BusinessUnitId + "/type/id/" + domainId);

			target.DomainId = Guid.Empty.ToString();
			target.Route().Should().Be.EqualTo("datasource/" + target.BusinessUnitId + "/type/ref/" + domainReferenceId);

			target.DomainReferenceId = Guid.Empty.ToString();
			target.Route().Should().Be.EqualTo("datasource/" + target.BusinessUnitId + "/type");
		}
	}
}