using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	public class SiteAdherenceAggregatorTest
	{
		[Test]
		public void ShouldAggregateOutOfAdherenceOnPositiveStaffingEffectForOneSite()
		{
			var siteId = Guid.NewGuid();
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var personId3 = Guid.NewGuid();
			var inAdherence1 = new ActualAgentState { StaffingEffect = 0 };
			var inAdherence2 = new ActualAgentState { StaffingEffect = 0 };
			var outOfAdherence = new ActualAgentState { StaffingEffect = 1 };

			var statisticRepository = MockRepository.GenerateMock<IStatisticRepository>();
			statisticRepository.Stub(x => x.LoadLastAgentState(new[] { personId1, personId2, personId3 }))
				.Return(new List<IActualAgentState> { inAdherence1, inAdherence2, outOfAdherence });

			var personOrganization = MockRepository.GenerateMock<IPersonOrganizationReader>();
			personOrganization.Stub(x => x.LoadAll()).Return(new[]
			{
				new PersonOrganizationData {PersonId = personId1, SiteId = siteId},
				new PersonOrganizationData {PersonId = personId2, SiteId = siteId},
				new PersonOrganizationData {PersonId = personId3, SiteId = siteId},
			});

			var target = new SiteAdherenceAggregator(statisticRepository, personOrganization);

			var result = target.Aggregate(siteId);

			result.Should().Be(1);
		}
	}
}
