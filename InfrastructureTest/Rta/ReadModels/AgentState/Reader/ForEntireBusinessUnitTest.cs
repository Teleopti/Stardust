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
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.AgentState.Reader
{
	[DatabaseTest]
	[TestFixture]
	public class ForEntireBusinessUnitTest
	{
		public Database Database;
		public IAgentStateReadModelPersister StatePersister;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IAgentStateReadModelReader Target;
		public ICurrentBusinessUnit BusinessUnit;

		[Test]
		public void ShouldGetAgentStates()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var londonId = Guid.NewGuid();
			var parisId = Guid.NewGuid();

			WithUnitOfWork.Do(() =>
			{
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = person1,
					BusinessUnitId = BusinessUnit.Current().Id,
					SiteId = londonId
				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = person2,
					BusinessUnitId = BusinessUnit.Current().Id,
					SiteId = parisId
				});
			});

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter()))
				.Select(x => x.SiteId.Value)
				.Should().Have.SameValuesAs(londonId, parisId);
		}

		[Test]
		public void ShouldGetAgentStatesInAlram()
		{
			Now.Is("2017-08-04 08:30");
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var londonId = Guid.NewGuid();
			var parisId = Guid.NewGuid();

			WithUnitOfWork.Do(() =>
			{
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = person1,
					BusinessUnitId = BusinessUnit.Current().Id,
					SiteId = londonId,
					AlarmStartTime = "2017-08-03 08:00".Utc(),
					IsRuleAlarm = true

				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = person2,
					BusinessUnitId = BusinessUnit.Current().Id,
					SiteId = parisId,
					AlarmStartTime = "2017-08-03 08:00".Utc(),
					IsRuleAlarm = true
				});

			});

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter { InAlarm = true }))
				.Select(x => x.SiteId.Value)
				.Should().Have.SameValuesAs(new[] { londonId, parisId });
		}

		[Test]
		public void GetOnlyGetAgentsForCurrentBusinessUnit()
		{
			var personId = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = personId,
					BusinessUnitId = BusinessUnit.Current().Id,

				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = Guid.NewGuid(),
					BusinessUnitId = Guid.NewGuid(),
				});
			});

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter())).Single()
				.PersonId
				.Should().Be(personId);
		}

		[Test]
		public void GetOnlyGetAgentsInAlarmForCurrentBusinessUnit()
		{
			Now.Is("2017-08-03 08:30");
			var personId = Guid.NewGuid();
			WithUnitOfWork.Do(() =>
			{
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = personId,
					BusinessUnitId = BusinessUnit.Current().Id,
					SiteId = Guid.NewGuid(),
					AlarmStartTime = "2017-08-02 08:00".Utc(),
					IsRuleAlarm = true

				});
				StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
				{
					PersonId = Guid.NewGuid(),
					BusinessUnitId = Guid.NewGuid(),
					SiteId = Guid.NewGuid(),
					AlarmStartTime = "2017-08-02 08:00".Utc(),
					IsRuleAlarm = true
				});
			});

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter{InAlarm = true})).Single()
				.PersonId
				.Should().Be(personId);
		}

		[Test]
		public void GetNumberOfAgents()
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

			WithUnitOfWork.Get(() => Target.Read(new AgentStateFilter()))
				.Count().Should().Be(1);
		}

		[Test]
		public void ShouldGetMaximum50AgentStates()
		{
			WithUnitOfWork.Do(() =>
			{
				Enumerable.Range(0, 51)
					.ForEach(i =>
					{
						StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
						{
							PersonId = Guid.NewGuid(),
							BusinessUnitId = BusinessUnit.Current().Id,
							SiteId = Guid.NewGuid()
						});
					});
			});

			WithUnitOfWork.Get(() =>
					Target.Read(new AgentStateFilter()))
				.Count().Should().Be(50);
		}

		[Test]
		public void ShouldGetMaximum50AgentStatesInAlarm()
		{
			Now.Is("2017-08-03 08:30");
			WithUnitOfWork.Do(() =>
			{
				Enumerable.Range(0, 51)
					.ForEach(i => StatePersister.PersistWithAssociation(new AgentStateReadModelForTest
					{
						PersonId = Guid.NewGuid(),
						BusinessUnitId = BusinessUnit.Current().Id,
						SiteId = Guid.NewGuid(),
						AlarmStartTime = "2017-08-02 08:00".Utc(),
						IsRuleAlarm = true
					}));
			});

			WithUnitOfWork.Get(() =>
					Target.Read(new AgentStateFilter { InAlarm = true }))
					.Count().Should().Be(50);
		}
	}
}