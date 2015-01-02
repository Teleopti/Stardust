using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;

namespace Teleopti.Ccc.TestCommon.IoC
{
	[TestFixture]
	[IoCTest]
	public class IoCTestTest
	{
		public IPersonRepository PersonRepository;
		public RebuildContainerDelegate RebuildContainer;

		[Test]
		public void ShouldInjectMembers()
		{
			PersonRepository.Should().Be.InstanceOf<PersonRepository>();
		}

		[Test]
		public void ShouldRecreateContainer()
		{
			var instance1 = PersonRepository;
			RebuildContainer();
			var instance2 = PersonRepository;
			instance1.Should().Not.Be.SameInstanceAs(instance2);
		}
	}
}