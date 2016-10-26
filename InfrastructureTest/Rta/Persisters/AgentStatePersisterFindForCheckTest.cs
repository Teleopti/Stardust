using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
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
				ExternalLogons = new[] { new ExternalLogon { UserCode = "user" } }
			}, DeadLockVictim.Yes);

			var result = Persister.FindForCheck("2016-10-26 12:00".Utc()).SingleOrDefault();

			result.UserCode.Should().Be("user");
		}

		[Test]
		public void ShouldFindOnTime()
		{
			var person = Guid.NewGuid();
			Persister.Prepare(new AgentStatePrepare
			{
				PersonId = person,
				ExternalLogons = new[] { new ExternalLogon { UserCode = "user"} }
			}, DeadLockVictim.Yes);
			Persister.Update(new AgentState
			{
				PersonId = person,
				NextCheck = "2016-10-26 12:00".Utc()
			}, true);

			var result = Persister.FindForCheck("2016-10-26 12:00".Utc()).SingleOrDefault();

			result.UserCode.Should().Be("user");
		}

		[Test]
		public void ShouldFindPassedTime()
		{
			var person = Guid.NewGuid();
			Persister.Prepare(new AgentStatePrepare
			{
				PersonId = person,
				ExternalLogons = new[] { new ExternalLogon { UserCode = "user" } }
			}, DeadLockVictim.Yes);
			Persister.Update(new AgentState
			{
				PersonId = person,
				NextCheck = "2016-10-26 12:00".Utc()
			}, true);

			var result = Persister.FindForCheck("2016-10-26 12:02".Utc()).SingleOrDefault();

			result.UserCode.Should().Be("user");
		}

		[Test]
		public void ShouldFindNullTime()
		{
			var person = Guid.NewGuid();
			Persister.Prepare(new AgentStatePrepare
			{
				PersonId = person,
				ExternalLogons = new[] { new ExternalLogon { UserCode = "user" } }
			}, DeadLockVictim.Yes);
			Persister.Update(new AgentState
			{
				PersonId = person,
				NextCheck = null
			}, true);

			var result = Persister.FindForCheck("2016-10-26 12:00".Utc()).SingleOrDefault();

			result.UserCode.Should().Be("user");
		}

		[Test]
		public void ShouldNotFindFutureCheck()
		{
			var person = Guid.NewGuid();
			Persister.Prepare(new AgentStatePrepare
			{
				PersonId = person,
				ExternalLogons = new[] { new ExternalLogon { UserCode = "user" } }
			}, DeadLockVictim.Yes);
			Persister.Update(new AgentState
			{
				PersonId = person,
				NextCheck = "2016-10-26 12:01".Utc()
			}, true);

			var result = Persister.FindForCheck("2016-10-26 12:00".Utc());

			result.Should().Be.Empty();
		}

		[Test]
		public void ShouldFindAfterScheduleInvalidation()
		{
			var person = Guid.NewGuid();
			Persister.Prepare(new AgentStatePrepare
			{
				PersonId = person,
				ExternalLogons = new[] { new ExternalLogon { UserCode = "user" } }
			}, DeadLockVictim.Yes);
			Persister.Update(new AgentState
			{
				PersonId = person,
				NextCheck = "2016-10-26 13:00".Utc()
			}, true);
			Persister.InvalidateSchedules(person, DeadLockVictim.Yes);

			var result = Persister.FindForCheck("2016-10-26 12:00".Utc()).SingleOrDefault();

			result.UserCode.Should().Be("user");
		}


	}
}