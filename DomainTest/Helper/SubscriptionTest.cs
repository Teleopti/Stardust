using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.DomainTest.Helper
{
	[TestFixture]
	public class SubscriptionTest
	{
		private Subscription target;
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
			businessUnitId = Guid.NewGuid();
			startDate = DateTime.Today.AddDays(-5);
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

			target.DomainId = null;
			target.Route().Should().Be.EqualTo("datasource/" + target.BusinessUnitId + "/type/ref/" + domainReferenceId);

			target.DomainReferenceId = null;
			target.Route().Should().Be.EqualTo("datasource/" + target.BusinessUnitId + "/type");
		}

		[Test]
		public void ShouldIgnoreBusinessUnitIdButNotDataSourceForStatisticTask()
		{
			target.DomainType = typeof(IStatisticTask).Name;
			target.Route().Should().Be.EqualTo("datasource/" + Subscription.IdToString(Guid.Empty) + "/" + target.DomainType + "/id/" +
			                                   domainId);
		}

		[Test]
		public void ShouldIgnoreBusinessUnitIdAndDataSourceForBatchEnd()
		{
			target.DomainType = typeof(IActualAgentState).Name;
			target.DomainId = Subscription.IdToString(Guid.Empty);
			target.Route().Should().Be.EqualTo("/"+Subscription.IdToString(Guid.Empty)+ "/" + target.DomainType + "/id/" + target.DomainId);
		}

		[Test]
		public void ShouldIncludeBusinessUnitIdAndDataSourceForExternalAgentState()
		{
			target.DomainType = typeof (IActualAgentState).Name;
			target.Route().Should().Be.EqualTo("/" + target.BusinessUnitId + "/" + target.DomainType + "/id/" +
			                                   domainId);
		}

		[Test]
		public void ShouldNotTreatFutureSubscriptionAsShortTerm()
		{
			startDate = DateTime.Today.AddDays(15);
			endDate = startDate.AddDays(30);
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
			target.Route().Should().Not.Contain("ShortTerm");
		}

		[Test]
		public void ShouldNotTreatPastSubscriptionAsShortTerm()
		{
			startDate = DateTime.Today.AddDays(-15);
			endDate = startDate.AddDays(13);
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
			target.Route().Should().Not.Contain("ShortTerm");
		}

		[Test]
		public void ShouldTreatCurrentSubscriptionAsShortTerm()
		{
			startDate = DateTime.Today.AddDays(-1);
			endDate = startDate.AddDays(2);
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
			target.Route().Should().Contain("ShortTerm");
		}

		[Test]
		public void ShouldExcludeDatasourceForTeamAdherenceMessage()
		{
			target = new Subscription
			{
				DataSource = Guid.NewGuid().ToString(),
				DomainType = typeof(SiteAdherenceMessage).Name
			};

			target.Route().Should().Not.Contain(target.DataSource);
		}

		[Test]
		public void ShouldExcludeDatasourceForSiteAdherenceMessage()
		{
			target = new Subscription
			{
				DataSource = Guid.NewGuid().ToString(),
				DomainType = typeof(TeamAdherenceMessage).Name
			};

			target.Route().Should().Not.Contain(target.DataSource);
		}

		[Test]
		public void ShouldExcludeDatasourceForAgentsAdherenceMessage()
		{
			target = new Subscription
			{
				DataSource = Guid.NewGuid().ToString(),
				DomainType = typeof(AgentsAdherenceMessage).Name
			};

			target.Route().Should().Not.Contain(target.DataSource);
		}

		[Test]
		public void ShouldExcludeDatasourceForTrackingMessage()
		{
			target = new Subscription
			{
				DataSource = Guid.NewGuid().ToString(),
				DomainType = typeof(TrackingMessage).Name
			};

			target.Route().Should().Not.Contain(target.DataSource);
		}

	}
}