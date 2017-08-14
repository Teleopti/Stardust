using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.AgentState.Reader
{

	[DatabaseTest]
	[TestFixture]
	public class ForEntireBuTest
	{
		public Database Database;
		public IAgentStateReadModelPersister StatePersister;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IAgentStateReadModelReader Target;
		public ICurrentBusinessUnit BusinessUnit;
		[Test]
		public void GetNoOfAgentsForNoSelection()
		{

			WithUnitOfWork.Do(() =>
			{
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = Guid.NewGuid(),
					BusinessUnitId = BusinessUnit.Current().Id,

				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = Guid.NewGuid(),
					BusinessUnitId = Guid.NewGuid(),

				});
			});

			WithUnitOfWork.Get(() =>
					Target.Read(new AgentStateFilter
					{
						InAlarm = false
					}))
				.Count().Should().Be(1);
		}

		[Test]
		public void ShouldGetMamimum50AgentStatesForEntireBu()
		{
		
			WithUnitOfWork.Do(() =>
			{
				Enumerable.Range(0, 51)
					.ForEach(i =>
					{
						StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
						{
							PersonId = Guid.NewGuid(),
							SiteId = Guid.NewGuid()

						});

					});
			});

			WithUnitOfWork.Get(() =>
					Target.Read(new AgentStateFilter
					{
						InAlarm = false
					}))
				.Count().Should().Be(50);
		}

		[Test]
		public void ShouldGetMamimum50AgentStatesInAlarmForEntireBu()
		{
			Now.Is("2017-08-03 08:30");
			WithUnitOfWork.Do(() =>
			{
				Enumerable.Range(0, 51)
					.ForEach(i =>
					{
						StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
						{
							PersonId = Guid.NewGuid(),
							SiteId = Guid.NewGuid(),
							AlarmStartTime = "2017-08-02 08:00".Utc(),
							IsRuleAlarm = true

						});

					});
			});

			WithUnitOfWork.Get(() =>
					Target.Read(new AgentStateFilter
					{
						InAlarm = true
					}))
				.Count().Should().Be(50);
		}


		[Test]
		public void ShouldGetAgentStatesForEntireBu()
		{

			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var siteLondonId = Guid.NewGuid();
			var siteParisId = Guid.NewGuid();

			WithUnitOfWork.Do(() =>
			{
				
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = person1,
					SiteId = siteLondonId

				});

				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = person2,
					SiteId = siteParisId

				});

			});

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter
				{
					InAlarm = false
				}))
				.Select(x => x.SiteId.Value)
				.Should().Have.SameValuesAs(new[] {siteLondonId, siteParisId});

		}

		[Test]
		public void ShouldGetAgentStatesInAlramForEntireBu()
		{
			Now.Is("2017-08-04 08:30");
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var siteLondonId = Guid.NewGuid();
			var siteParisId = Guid.NewGuid();

			WithUnitOfWork.Do(() =>
			{

				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = person1,
					SiteId = siteLondonId,
					AlarmStartTime = "2017-08-03 08:00".Utc(),
					IsRuleAlarm = true

				});

				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = person2,
					SiteId = siteParisId,
					AlarmStartTime = "2017-08-03 08:00".Utc(),
					IsRuleAlarm = true

				});

			});

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter
			{
				InAlarm = true
			}))
				.Select(x => x.SiteId.Value)
				.Should().Have.SameValuesAs(new[] { siteLondonId, siteParisId });

		}
	}
}