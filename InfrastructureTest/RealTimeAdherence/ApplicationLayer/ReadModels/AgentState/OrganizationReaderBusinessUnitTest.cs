using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.ApplicationLayer.ReadModels.AgentState
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
			var businessUnit = BusinessUnit.Current().Id.Value;
			Persister.UpsertAssociation(new AssociationInfoForTest
			{
				PersonId = person1,
				BusinessUnitId = businessUnit,
				SiteId = site,
			});
			Persister.UpsertAssociation(new AssociationInfoForTest
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