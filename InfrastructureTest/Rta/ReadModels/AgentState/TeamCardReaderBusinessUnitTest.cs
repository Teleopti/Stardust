using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.AgentState
{
	[TestFixture]
	[UnitOfWorkTest]
	public class TeamCardReaderBusinessUnitTest
	{
		public ICurrentBusinessUnit BusinessUnit;
		public IAgentStateReadModelPersister Persister;
		public ITeamCardReader Target;
		public MutableNow Now;
		public Database Database;

		[Test]
		public void ShouldFilterOnCurrentBusinessUnit()
		{
			var site = Guid.NewGuid();
			Persister.Upsert(new AgentStateReadModelForTest
			{
				BusinessUnitId = BusinessUnit.Current().Id.Value,
				PersonId = Guid.NewGuid(),
				SiteId = site
			});
			Persister.Upsert(new AgentStateReadModelForTest
			{
				BusinessUnitId = Guid.NewGuid(),
				PersonId = Guid.NewGuid(),
				SiteId = Guid.NewGuid(),
			});

			Target.Read().Single()
				.SiteId.Should().Be(site);
		}
		
	}
}