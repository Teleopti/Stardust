using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers.ScheduleProjection;
using Teleopti.Ccc.Domain.UnitOfWork;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[MultiDatabaseTest]
	public class AgentStatePersisterDataVersionsTest
	{
		public IAgentStatePersister Target;
		public IScheduleProjectionReadOnlyPersister Persister;
		public IScheduleReader Reader;
		public IKeyValueStorePersister KeyValueStore;
		public WithUnitOfWork App;
		public WithReadModelUnitOfWork ReadModels;
		
		[Test]
		public void ShouldReturnMappingVersionLoadingByPersonId()
		{
			var state = new AgentStateForUpsert {PersonId = Guid.NewGuid()};

			App.Do(() => Target.Upsert(state));
			ReadModels.Do(() => KeyValueStore.Update("RuleMappingsVersion", "the version"));

			var result = App.Get(() => Target.LockNLoad(new[] { state.PersonId }, DeadLockVictim.Yes));
			result.MappingVersion.Should().Be("the version");
		}
		
		[Test]
		public void ShouldReturnScheduleVersionLoadingByPersonId()
		{
			var version = CurrentScheduleReadModelVersion.Generate();
			var state = new AgentStateForUpsert {PersonId = Guid.NewGuid()};

			App.Do(() => Target.Upsert(state));
			ReadModels.Do(() => KeyValueStore.Update("CurrentScheduleReadModelVersion", version));

			var result = App.Get(() => Target.LockNLoad(new[] { state.PersonId }, DeadLockVictim.Yes));
			result.ScheduleVersion.Should().Be(version);
		}

	}
}