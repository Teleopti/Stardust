using System;
using Autofac;

namespace Stardust.Manager
{
	public class ManagerStarter
	{
		public void Start(ManagerConfiguration managerConfiguration, IComponentContext componentContext)
		{
			if (managerConfiguration == null)
			{
				throw new ArgumentNullException("nodeConfiguration");
			}
			if (componentContext == null)
			{
				throw new ArgumentNullException("componentContext");
			}

			var builder = new ContainerBuilder();
			builder.RegisterModule(new ManagerModule(managerConfiguration));
			builder.Update(componentContext.ComponentRegistry);

			//to start the timers etc
			componentContext.Resolve<ManagerController>();
		}
	}
}