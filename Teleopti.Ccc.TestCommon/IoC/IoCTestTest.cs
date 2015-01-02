using Autofac;
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
		public IIoCTestContext Context;

		[Test]
		public void ShouldInjectMembers()
		{
			PersonRepository.Should().Be.InstanceOf<PersonRepository>();
		}

		[Test]
		public void ShouldRecreateContainer()
		{
			var instance1 = PersonRepository;
			Context.RebuildContainer();
			var instance2 = PersonRepository;
			instance1.Should().Not.Be.SameInstanceAs(instance2);
		}

		[Test]
		public void ShouldRecreateContainerWithTestRegistrations()
		{
			var instance1 = PersonRepository;
			Context.RebuildContainer(b => b.RegisterInstance(instance1).As<IPersonRepository>());
			var instance2 = PersonRepository;
			instance1.Should().Be.SameInstanceAs(instance2);
		}
	}
}