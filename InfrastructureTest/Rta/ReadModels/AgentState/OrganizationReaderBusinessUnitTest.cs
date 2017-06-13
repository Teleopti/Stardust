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
	public class OrganizationReaderBusinessUnitTest
	{
		public ICurrentBusinessUnit BusinessUnit;
		public IAgentStateReadModelPersister Persister;
		public IOrganizationReader Target;
		public MutableNow Now;
		public Database Database;

		[Test]
		public void ShouldFilterOnCurrentBusinessUnit()
		{
			var site = Guid.NewGuid();
			var businessUnit = BusinessUnit.Current().Id.Value;
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				BusinessUnitId = businessUnit,
				PersonId = Guid.NewGuid(),
				SiteId = site
			});
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				BusinessUnitId = Guid.NewGuid(),
				PersonId = Guid.NewGuid(),
				SiteId = Guid.NewGuid(),
			});

			var result = Target.Read().Single();

			result.SiteId.Should().Be(site);
			result.BusinessUnitId.Should().Be(businessUnit);
		}
		
	}
}