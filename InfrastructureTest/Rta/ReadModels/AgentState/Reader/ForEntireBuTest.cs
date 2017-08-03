using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

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
	}
}