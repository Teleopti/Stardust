using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.ApplicationLayer.ReadModels.AgentState.Reader
{
	[TestFixture]
	[UnitOfWorkTest]
	public class EmptyCollectionsTest
	{
		public IAgentStateReadModelReader Target;
		public IAgentStateReadModelPersister Persister;

		[Test]
		public void ShouldLoadAllWhenEmptyCollections()
		{
			var personId = Guid.NewGuid();
			Persister.Upsert(new AgentStateReadModelForTest
			{
				PersonId = personId
			});

			var result = Target.Read(new AgentStateFilter
			{
				TeamIds = Enumerable.Empty<Guid>(),
				SiteIds = Enumerable.Empty<Guid>(),
				SkillIds = Enumerable.Empty<Guid>(),
				ExcludedStateIds = Enumerable.Empty<Guid?>()
			});

			result.Single().PersonId.Should().Be(personId);
		}
	}
}