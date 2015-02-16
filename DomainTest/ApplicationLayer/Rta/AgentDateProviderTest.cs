using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[TestFixture]
	public class AgentDateProviderTest
	{
		[Test]
		public void ShouldGetDateInAgentTimeZone()
		{
			var now = new ThisIsNow("2014-10-10 05:00");
			var person = new Person().WithId();
			person.PermissionInformation.SetDefaultTimeZone(new HawaiiTimeZone().TimeZone());
			var personRepository = MockRepository.GenerateStub<IPersonRepository>();
			personRepository.Stub(x => x.Get(person.Id.GetValueOrDefault())).Return(person);
			var target = new AgentDateProvider(now, personRepository);
			var result = target.Get(person.Id.GetValueOrDefault());
			result.Should().Be("2014-10-09".Date());
		}
	}
}
