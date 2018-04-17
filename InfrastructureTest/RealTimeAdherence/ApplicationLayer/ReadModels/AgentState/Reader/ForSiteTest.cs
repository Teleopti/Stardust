﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.ApplicationLayer.ReadModels.AgentState.Reader
{
	[TestFixture]
	[UnitOfWorkTest]
	public class ForSiteTest
	{
		public IAgentStateReadModelReader Target;
		public IAgentStateReadModelPersister Persister;
		
		[Test]
		public void ShouldLoadAgentStatesBySiteIds()
		{
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			Persister.UpsertToActive(new AgentStateReadModelForTest {SiteId = siteId1, PersonId = Guid.NewGuid()});
			Persister.UpsertToActive(new AgentStateReadModelForTest {SiteId = siteId2, PersonId = Guid.NewGuid()});
			Persister.UpsertToActive(new AgentStateReadModelForTest {SiteId = Guid.Empty, PersonId = Guid.NewGuid()});

			var result = Target.Read(new AgentStateFilter {SiteIds = new[] {siteId1, siteId2}});

			result.Count().Should().Be(2);
		}
	}
}