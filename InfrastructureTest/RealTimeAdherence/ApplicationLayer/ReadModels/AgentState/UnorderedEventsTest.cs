using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ReadModels;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.ApplicationLayer.ReadModels.AgentState
{
	[TestFixture]
	[DatabaseTest]
	public class AgentStateReadModelMaintainerUnorderedEventsTest
	{
		public AgentStateReadModelMaintainer Target;
		public WithUnitOfWork UnitOfWork;
		public IAgentStateReadModelPersister Persister;
		
		[Test]
		[TestCaseSource(typeof(PermuationFactory), "Permutations")]
		public void ShouldHandleAllCombinationsOfEventOrder(IEnumerable<IEvent> events)
		{
			var personId = events.OfType<PersonAssociationChangedEvent>().Single().PersonId;

			events.ForEach(e => Target.Handle((dynamic)e));
			
			var result = UnitOfWork.Get(() => Persister.Load(personId));
			result.FirstName.Should().Be("Roger");
			result.LastName.Should().Be("Kjatz");
			result.EmploymentNumber.Should().Be("1234");
			result.TeamName.Should().Be("preferences");
			result.SiteName.Should().Be("london");
		}
		
		public class PermuationFactory
		{
			public static IEnumerable Permutations
			{
				get
				{
					var personId = Guid.NewGuid();
					var teamId = Guid.NewGuid();
					var siteId = Guid.NewGuid();
					var events = new List<IEvent>
				{
					new PersonAssociationChangedEvent
					{
						PersonId = personId,
						TeamId = teamId,
						TeamName = "preferences",
						SiteId = siteId,
						SiteName = "london",
						Timestamp = "2017-04-12 08:00:00".Utc(),
						ExternalLogons = new []{new Ccc.Domain.ApplicationLayer.Events.ExternalLogon { DataSourceId = 1, UserCode = "roger"} },
						FirstName = "Roger",
						LastName = "Kjatz",
						EmploymentNumber = "1234",
					},
					new SiteNameChangedEvent
					{
						SiteId = siteId,
						Name = "london"
					},
					new TeamNameChangedEvent
					{
						TeamId = teamId,
						Name = "preferences"
					}
				};

					var permutations = events.Permutations();

					return from p in permutations
						   let name = (from pe in p select pe.GetType().Name).Aggregate((current, next) => current + ", " + next)
						   select new TestCaseData(p).SetName(name);
				}
			}
		}
	}
}