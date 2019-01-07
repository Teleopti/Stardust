using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.Collection;
using Module = Autofac.Module;

namespace Teleopti.Ccc.TestCommon.IoC
{
	// these properties are injected by reflection in ServiceLocatorForFakesModule
	// can be used in fakes to get proper lifetime because fakes may live longer than container
	public static class ServiceLocatorForFakes
	{
		public static IResolve Resolver { get; set; }
		public static T Resolve<T>() => Resolver.Resolve<T>();
	}

	public class ServiceLocatorForFakesModule : Module
	{
		private bool _injecting = false;

		protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
		{
			registration.Activated += (o, e) => injectServiceLocators(e.Context);
		}

		private void injectServiceLocators(IComponentContext container)
		{
			if (_injecting)
				return;
			_injecting = true;

			// set service locators
			inject(container, typeof(ServiceLocatorForFakes));

			// resolve the disposer so that we are sure its dispose method will be called
			container.Resolve<serviceLocatorDisposer>();
		}

		private static void inject(IComponentContext container, IReflect type)
		{
			// set roperty values
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static).Where(x => x.CanWrite);
			properties.ForEach(x => x.SetValue(null, container.Resolve(x.PropertyType), null));
		}

		private static void clear(Type type)
		{
			// clear property values
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static).Where(x => x.CanWrite);
			properties.ForEach(x => x.SetValue(null, null, null));
		}

		protected override void Load(ContainerBuilder builder)
		{
			builder.RegisterType<serviceLocatorDisposer>();
		}

		private class serviceLocatorDisposer : IDisposable
		{
			public void Dispose()
			{
				clear(typeof(ServiceLocatorForFakes));
			}
		}
	}
}