using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Module = Autofac.Module;

namespace Teleopti.Ccc.IocCommon.Configuration
{
	internal class ServiceLocatorModule : Module
	{
		private bool _injecting = false;

		protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
		{
			registration.Activated += (o, e) => injectServiceLocatorForEntity(e.Context);
		}

		private void injectServiceLocatorForEntity(IComponentContext container)
		{
			if (_injecting)
				return;
			_injecting = true;

			// set service locators property values
			var type = typeof (ServiceLocatorForEntity);
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static).Where(x => x.CanWrite);
			properties.ForEach(x => x.SetValue(null, container.Resolve(x.PropertyType), null));

			// resolve the disposer so that we are sure its dispose method will be called
			container.Resolve<serviceLocatorForEntityDisposer>();
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<serviceLocatorForEntityDisposer>();
		}

		private class serviceLocatorForEntityDisposer : IDisposable
		{
			public void Dispose()
			{
				// clear property values
				var type = typeof(ServiceLocatorForEntity);
				var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static).Where(x => x.CanWrite);
				properties.ForEach(x => x.SetValue(null, null, null));
			}
		}
	}
}