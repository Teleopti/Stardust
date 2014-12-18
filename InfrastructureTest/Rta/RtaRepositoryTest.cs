using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Rta;
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
	    public void ShouldLoadLastAgentState()
	    {
			var person = PersonFactory.CreatePerson("Ashlee", "Andeen");
			person.SetId(Guid.NewGuid());
			var result = target.LoadLastAgentState(new List<Guid> { person.Id.GetValueOrDefault() });
			Assert.IsNotNull(result);
	    }

    }
}
