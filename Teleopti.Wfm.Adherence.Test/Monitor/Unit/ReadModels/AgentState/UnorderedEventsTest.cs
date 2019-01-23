using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Monitor;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Unit.ReadModels.AgentState
{
	[TestFixture]
	[DomainTest]
	public class UnorderedEventsTest
	{
		public AgentStateReadModelMaintainer Target;
		public FakeAgentStateReadModelPersister Persister;

		[Test]
		[TestCaseSource(typeof(PermuationFactory), "Permutations")]
		public void ShouldHandleAllCombinationsOfEventOrder(IEnumerable<IEvent> events)
		{
			events.ForEach(e =>
			{
				if (e is PersonAssociationChangedEvent)
					Target.Handle(e.AsArray());
				else
					Target.Handle((dynamic) e);
			});

			var result = Persister.Models.Single();
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
							ExternalLogons = new[] {new ExternalLogon {DataSourceId = 1, UserCode = "roger"}},
							FirstName = "Roger",
							LastName = "Kjatz",
							EmploymentNumber = "1234"
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