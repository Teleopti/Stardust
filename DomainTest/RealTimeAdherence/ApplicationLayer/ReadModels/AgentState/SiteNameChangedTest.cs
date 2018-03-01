using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.ApplicationLayer.ReadModels.AgentState
{
	[TestFixture]
	[DomainTest]
	public class SiteNameChangedTest
	{
		public AgentStateReadModelMaintainer Target;
		public FakeAgentStateReadModelPersister Persister;

		[Test]
		public void ShouldUpdateSiteName()
		{
			var siteId = Guid.NewGuid();
			Persister.Has(new AgentStateReadModel { PersonId = Guid.NewGuid(), SiteId = siteId });

			Target.Handle(new SiteNameChangedEvent
			{
				SiteId = siteId,
				Name = "london"
			});

			Persister.Models.Single().SiteName.Should().Be("london");
		}
	}
}