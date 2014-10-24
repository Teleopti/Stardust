using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
    [TestFixture]
    [Category("LongRunning")]
    public class RtaRepositoryTest : DatabaseTest
    {
        private IRtaRepository target;

		protected override void SetupForRepositoryTest()
		{
			target = new RtaRepository();
		}

		[Test]
		public void VerifyLoadActualAgentState()
		{
			var person = PersonFactory.CreatePerson("Ashlee", "Andeen");
			person.SetId(Guid.NewGuid());
			var result = target.LoadActualAgentState(new List<IPerson> {person});
			Assert.IsNotNull(result);
		}

		[Test]
		public void VerifyLoadOneActualAgentState()
		{
			VerifyAddOrUpdateActualAgentState();
			Assert.IsNotNull(target.LoadOneActualAgentState(Guid.Empty));
		}

		[Test]
		public void VerifyAddOrUpdateActualAgentState()
		{
			var agentState = new ActualAgentState
				{
					ReceivedTime = new DateTime(1900, 1, 1),
					OriginalDataSourceId = ""
				};
			target.AddOrUpdateActualAgentState(agentState);
		}

	    [Test]
	    public void ShouldLoadLastAgentState()
	    {
			var person = PersonFactory.CreatePerson("Ashlee", "Andeen");
			person.SetId(Guid.NewGuid());
			var result = target.LoadLastAgentState(new List<Guid> { person.Id.GetValueOrDefault() });
			Assert.IsNotNull(result);
	    }

    }
}
