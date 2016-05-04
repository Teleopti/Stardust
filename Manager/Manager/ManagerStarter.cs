using System;
using Autofac;
using Autofac.Integration.WebApi;
using Stardust.Manager.Helpers;
using Stardust.Manager.Interfaces;
using Stardust.Manager.Validations;

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
		}
	}
}