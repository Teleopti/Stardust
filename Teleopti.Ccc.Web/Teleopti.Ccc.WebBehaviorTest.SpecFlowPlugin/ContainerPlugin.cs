using Autofac;
using TechTalk.SpecFlow.Infrastructure;
using TechTalk.SpecFlow.Plugins;
using Teleopti.Ccc.WebBehaviorTest.SpecFlowPlugin;

[assembly: RuntimePlugin(typeof(ContainerPlugin))]

namespace Teleopti.Ccc.WebBehaviorTest.SpecFlowPlugin
{
	public class ContainerPlugin : IRuntimePlugin
	{
		public static void UseContainer(IContainer container)
		{
			_container = container;
		}

		private static IComponentContext _container;
		private static bool _initialized = false;

		public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters)
		{
			runtimePluginEvents.CustomizeGlobalDependencies += (sender, args) =>
			{
				if (_initialized) return;
				_initialized = true;
				args.ObjectContainer.RegisterTypeAs<AutofacTestObjectResolver, ITestObjectResolver>();
			};

			runtimePluginEvents.CustomizeScenarioDependencies += (sender, args) =>
			{
				args.ObjectContainer.RegisterFactoryAs(() => _container);
			};
		}
	}
}