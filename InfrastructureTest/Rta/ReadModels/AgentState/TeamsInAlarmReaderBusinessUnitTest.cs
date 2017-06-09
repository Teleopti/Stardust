using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.AgentState
{
	[TestFixture]
	[UnitOfWorkTest]
	public class TeamsInAlarmReaderBusinessUnitTest
	{
		public IAgentStateReadModelPersister Persister;
		public ITeamsInAlarmReader Target;
		public MutableNow Now;
		public Database Database;

		[Test]
		public void ShouldFilterOnCurrentBusinessUnit()
		{
			var site = Guid.NewGuid();
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				BusinessUnitId = BusinessUnitFactory.BusinessUnitUsedInTest.Id.Value,
				PersonId = Guid.NewGuid(),
				SiteId = site
			});
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
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