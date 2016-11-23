﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[UnitOfWorkTest]
	public class AgentStatePersisterGetTest
	{
		public IAgentStatePersister Persister;
		public ICurrentAnalyticsUnitOfWork UnitOfWork;
		public MutableNow Now;
		
		[Test]
		public void ShouldGetByPersonIds()
		{
			var state1 = new AgentStateForUpsert { PersonId = Guid.NewGuid() };
			var state2 = new AgentStateForUpsert { PersonId = Guid.NewGuid() };
			var state3 = new AgentStateForUpsert { PersonId = Guid.NewGuid() };
			Persister.Upsert(state1);
			Persister.Upsert(state2);
			Persister.Upsert(state3);

			var result = Persister.Get(new[] {state1.PersonId, state3.PersonId});
			
			result.Select(x => x.PersonId).Should().Have.SameValuesAs(state1.PersonId, state3.PersonId);
		}

		[Test]
		public void ShouldGetCurrentActualAgentStates()
		{
			var writer = Persister;
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			writer.Upsert(new AgentStateForUpsert { PersonId = personId1 });
			writer.Upsert(new AgentStateForUpsert { PersonId = personId2 });

			var result = Persister.GetStates();

			result.Where(x => x.PersonId == personId1).Should().Have.Count.EqualTo(1);
			result.Where(x => x.PersonId == personId2).Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldGetCurrentActualAgentStatesWithAllData()
		{
			var writer = Persister;
			var state = new AgentStateForUpsert
			{
				PersonId = Guid.NewGuid(),
				BusinessUnitId = Guid.NewGuid(),
				TeamId = Guid.NewGuid(),
				SiteId = Guid.NewGuid(),
				RuleId = Guid.NewGuid(),
				RuleStartTime = "2014-11-11 10:33".Utc(),
				BatchId = "2014-11-11 10:34".Utc(),
				SourceId = "1",
				PlatformTypeId = Guid.NewGuid(),
				ReceivedTime = "2014-11-11 10:36".Utc(),
				StateCode = "statecode",
				StateGroupId = Guid.NewGuid(),
				StateStartTime = "2014-11-11 10:37".Utc(),
			};
			writer.Upsert(state);

			var result = Persister.GetStates().Single();

			result.PersonId.Should().Be(state.PersonId);
			result.BusinessUnitId.Should().Be(state.BusinessUnitId);
			result.TeamId.Should().Be(state.TeamId);
			result.SiteId.Should().Be(state.SiteId);
			result.RuleId.Should().Be(state.RuleId);
			result.RuleStartTime.Should().Be(state.RuleStartTime);
			result.BatchId.Should().Be(state.BatchId);
			result.SourceId.Should().Be(state.SourceId);
			result.PlatformTypeId.Should().Be(state.PlatformTypeId);
			result.ReceivedTime.Should().Be(state.ReceivedTime);
			result.StateCode.Should().Be(state.StateCode);
			result.StateGroupId.Should().Be(state.StateGroupId);
			result.StateStartTime.Should().Be(state.StateStartTime);
		}
		
	}
}