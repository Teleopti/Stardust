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

		[SetUp]
		public void Setup()
		{
			domainId = Guid.NewGuid();
			domainReferenceId = Guid.NewGuid();
			moduleId = Guid.NewGuid();
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
			         		EndDate = Subscription.DateToString(endDate)
			         	};
		}

		[Test]
		public void PropertiesShouldWork()
		{
			target.BinaryData.Should().Be.EqualTo("test");
			target.DomainIdAsGuid().Should().Be.EqualTo(domainId);
			target.DomainReferenceIdAsGuid().Should().Be.EqualTo(domainReferenceId);
			target.ModuleIdAsGuid().Should().Be.EqualTo(moduleId);
			target.DomainReferenceType.Should().Be.EqualTo("ref");
			target.DomainType.Should().Be.EqualTo("type");
			target.DomainUpdateTypeAsDomainUpdateType().Should().Be.EqualTo(DomainUpdateType.Update);
			target.StartDateAsDateTime().Should().Be.EqualTo(startDate);
			target.EndDateAsDateTime().Should().Be.EqualTo(endDate);
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

		[SetUp]
		public void Setup()
		{
			domainId = Guid.NewGuid();
			domainReferenceId = Guid.NewGuid();
			subscriptionId = Guid.NewGuid();
			startDate = DateTime.Today;
			endDate = startDate.AddDays(1);
			target = new Subscription
			{
				DomainId = Subscription.IdToString(domainId),
				DomainReferenceId = Subscription.IdToString(domainReferenceId),
				DomainReferenceType = "ref",
				DomainType = "type",
				SubscriptionId = Subscription.IdToString(subscriptionId),
				LowerBoundary = Subscription.DateToString(startDate),
				UpperBoundary = Subscription.DateToString(endDate)
			};
		}

		[Test]
		public void PropertiesShouldWork()
		{
			target.DomainIdAsGuid().Should().Be.EqualTo(domainId);
			target.DomainReferenceIdAsGuid().Should().Be.EqualTo(domainReferenceId);
			target.SubscriptionIdAsGuid().Should().Be.EqualTo(subscriptionId);
			target.DomainReferenceType.Should().Be.EqualTo("ref");
			target.DomainType.Should().Be.EqualTo("type");
			target.LowerBoundaryAsDateTime().Should().Be.EqualTo(startDate);
			target.UpperBoundaryAsDateTime().Should().Be.EqualTo(endDate);
		}
	}
}