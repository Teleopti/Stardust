using Castle.Components.DictionaryAdapter.Xml;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.IocCommon.Configuration;
using Teleopti.Ccc.IocCommon.Toggle;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.IocCommonTest.Configuration
{
	[TestFixture]
	[DomainTest]
	public class EventHandlersModuleTest
	{
		public ResolveEventHandlers Resolver;
		public IResolve Resolve;

		[AllTogglesOff]
		[Test]
		public void ShouldResolveAllEventHandlersWhenTogglesDisabled()
		{
			Resolver.ResolveAllJobs(EventHandlerLocations.OneOfEachEvent());
		}

		[AllTogglesOn]
		[Test]
		public void ShouldResolveAllEventHandlersWhenTogglesEnabled()
		{
			Resolver.ResolveAllJobs(EventHandlerLocations.OneOfEachEvent());
		}

		[Test]
		[Ignore("Reason mandatory for NUnit 3")]
		public void ShouldResolveSameInstanceOfHandlers()
		{
			Resolver.ResolveAllJobs(EventHandlerLocations.OneOfEachEvent())
				.ForEach(x =>
				{
					var instance1 = Resolve.Resolve(x.HandlerType);
					var instance2 = Resolve.Resolve(x.HandlerType);
					instance1.Should().Be.SameInstanceAs(instance2);
				});
		}
	}
}