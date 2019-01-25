using System;
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
		private readonly IocConfiguration _config;
		private bool _injecting = false;

		public ServiceLocatorModule(IocConfiguration config)
		{
			_config = config;
		}

		protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
		{
			registration.Activated += (o, e) => injectServiceLocators(e.Context);
		}

		private void injectServiceLocators(IComponentContext container)
		{
			if (_injecting)
				return;
			_injecting = true;

			if (_config.Args().EnableLegacyServiceLocators)
			{
				var state = new ServiceLocatorState();
				var fields = state.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
				fields.ForEach(x => x.SetValue(state, container.Resolve(x.FieldType)));
				ServiceLocator_DONTUSE.Instance.Push(state);
			}
			else
			{
				ServiceLocator_DONTUSE.Instance.Push(null);
			}

			// resolve the disposer so that we are sure its dispose method will be called
			container.Resolve<serviceLocatorDisposer>();
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<serviceLocatorDisposer>();
		}

		private class serviceLocatorDisposer : IDisposable
		{
			public void Dispose()
			{
				ServiceLocator_DONTUSE.Instance.Pop();
			}
		}
	}
}