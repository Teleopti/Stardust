using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.AgentState.Reader
{
	[UnitOfWorkTest]
	[TestFixture]
	public class Order
	{
		public Database Database;
		public IAgentStateReadModelPersister Persister;
		public MutableNow Now;
		public WithUnitOfWork WithUnitOfWork;
		public IAgentStateReadModelReader Target;

		[Test]
		public void ShouldOrderByAlarmStartTimeForStatesInAlarm()
		{
			Now.Is("2016-06-15 12:05");
			var teamId = Guid.NewGuid();
			var personId1 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f8");
			var personId2 = Guid.Parse("aeca77e1-bdc5-4f6d-bab1-bcfcdafa53f9");
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId1,
				AlarmStartTime = "2016-06-15 12:02".Utc(),
				IsRuleAlarm = true
			});
			Persister.PersistWithAssociation(new AgentStateReadModelForTest
			{
				TeamId = teamId,
				PersonId = personId2,
				AlarmStartTime = "2016-06-15 12:01".Utc(),
				IsRuleAlarm = true
			});

			var result = Target.Read(new AgentStateFilter()
			{
				TeamIds = teamId.AsArray(),
				InAlarm = true
			});

			result.First().PersonId.Should().Be(personId2);
			result.Second().PersonId.Should().Be(personId1);
		}


	}
}