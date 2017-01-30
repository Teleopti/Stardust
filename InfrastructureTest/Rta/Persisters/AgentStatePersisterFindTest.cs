using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[TestFixture]
	[UnitOfWorkTest]
	public class AgentStatePersisterFindTest
	{
		public IAgentStatePersister Persister;
		public ICurrentAnalyticsUnitOfWork UnitOfWork;
		public MutableNow Now;

		[Test]
		public void ShouldFind()
		{
			var state = new AgentStateForUpsert {PersonId = Guid.NewGuid(), UserCode = "user"};
			Persister.Upsert(state);

			var result = Persister.ReadForTest(new ExternalLogon {UserCode = "user"}).SingleOrDefault();

			result.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldGetEmptyStatesIfNotFound()
		{
			var result = Persister.ReadForTest(new ExternalLogon());

			result.Should().Be.Empty();
		}

	}
}