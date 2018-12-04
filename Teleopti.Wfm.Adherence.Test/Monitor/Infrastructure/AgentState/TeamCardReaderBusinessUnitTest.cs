using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Infrastructure.AgentState
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