using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Wfm.Adherence.Monitor.Infrastructure;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Infrastructure.AgentState
{
	[TestFixture]
	[UnitOfWorkTest]
	public class OrganizationReaderBusinessUnitTest
	{
		public ICurrentBusinessUnit BusinessUnit;
		public IAgentStateReadModelPersister Persister;
		public IOrganizationReader Target;

		[Test]
		public void ShouldFilterOnCurrentBusinessUnit()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var site = Guid.NewGuid();
			var businessUnit = BusinessUnit.CurrentId();
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person1,
				BusinessUnitId = businessUnit,
				SiteId = site,
			});
			Persister.UpsertAssociation(new AssociationInfo
			{
				PersonId = person2,
				BusinessUnitId = Guid.NewGuid(),
				SiteId = Guid.NewGuid(),
			});

			var result = Target.Read().Single();

			result.SiteId.Should().Be(site);
			result.BusinessUnitId.Should().Be(businessUnit);
		}
	}
}