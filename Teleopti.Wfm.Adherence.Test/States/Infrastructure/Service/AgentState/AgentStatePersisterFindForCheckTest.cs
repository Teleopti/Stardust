using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.States.Infrastructure.Service.AgentState
{
	[TestFixture]
	[UnitOfWorkTest]
	public class AgentStatePersisterFindForCheckTest
	{
		public IAgentStatePersister Persister;
		public ICurrentAnalyticsUnitOfWork UnitOfWork;

		[Test]
		public void ShouldFindNew()
		{
			var person = Guid.NewGuid();
			Persister.Prepare(new AgentStatePrepare
			{
				PersonId = person,
			}, DeadLockVictim.Yes);

			var result = Persister.FindForCheck().SingleOrDefault();

			result.PersonId.Should().Be(person);
			result.LastCheck.Should().Be(null);
		}

		[Test]
		public void ShouldFindWithTime()
		{
			var person = Guid.NewGuid();
			Persister.Prepare(new AgentStatePrepare
			{
				PersonId = person,
			}, DeadLockVictim.Yes);
			Persister.Update(new Adherence.States.AgentState
			{
				PersonId = person,
				ReceivedTime = "2016-10-26 12:00".Utc()
			});

			var result = Persister.FindForCheck().SingleOrDefault();

			result.LastCheck.Should().Be("2016-10-26 12:00".Utc());
		}

		[Test]
		public void ShouldFindWithoutTime()
		{
			var person = Guid.NewGuid();
			Persister.Prepare(new AgentStatePrepare
			{
				PersonId = person,
			}, DeadLockVictim.Yes);
			Persister.Update(new Adherence.States.AgentState
			{
				PersonId = person,
				ReceivedTime = null
			});

			var result = Persister.FindForCheck().SingleOrDefault();

			result.LastCheck.Should().Be(null);
		}
		
		[Test]
		public void ShouldFindWithTimeWindowCheckSum()
		{
			var person = Guid.NewGuid();
			Persister.Prepare(new AgentStatePrepare
			{
				PersonId = person,
			}, DeadLockVictim.Yes);
			Persister.Update(new Adherence.States.AgentState
			{
				PersonId = person,
				ReceivedTime = "2016-10-26 12:00".Utc(),
				TimeWindowCheckSum = 123
			});

			var result = Persister.FindForCheck().SingleOrDefault();

			result.LastTimeWindowCheckSum.Should().Be(123);
		}


	}
}