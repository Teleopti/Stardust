using System.Configuration;
using Autofac;

namespace Teleopti.Ccc.IocCommon
{
	public class IocArgs
	{
		public string FeatureToggle { get; set; }
		public string ToggleMode { get; set; }

		public bool MessageBrokerListeningEnabled { get; set; }
		public IContainer SharedContainer { get; set; }

		public IocArgs()
		{
			FeatureToggle = ConfigurationManager.AppSettings["FeatureToggle"];
			ToggleMode = ConfigurationManager.AppSettings["ToggleMode"];
		}

		public T ResolveSharedComponent<T>(IComponentContext c)
		{
			return SharedContainer == null ? c.Resolve<T>() : SharedContainer.Resolve<T>();
		}
	}
}