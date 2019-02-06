using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using NUnit.Framework.Interfaces;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.IocCommon.Toggle;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class IoCTestService : IDisposable
	{
		private ILifetimeScope _container;
		private readonly MethodInfo _method;
		private readonly IEnumerable<object> _injectTo;

		public IoCTestService(ITest testDetails, object attribute)
			: this(new[] {testDetails.Fixture, attribute}, testDetails.Method.MethodInfo)
		{
		}

		public IoCTestService(IEnumerable<object> injectTo, MethodInfo method)
		{
			_injectTo = injectTo;
			_method = method;
		}

		public void InjectFrom(ILifetimeScope container)
		{
			_container = container;
			_injectTo.ForEach(x => InjectTo(_container, x));
		}

		public static void InjectTo(ILifetimeScope container, object instance)
		{
			var type = instance.GetType();
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanWrite);
			properties.ForEach(x => x.SetValue(instance, container.Resolve(x.PropertyType), null));
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
			fields.ForEach(x => x.SetValue(instance, container.Resolve(x.FieldType)));
		}

		public FakeToggleManager Toggles()
		{
			var toggles = new FakeToggleManager();
			if (QueryAllAttributes<AllTogglesOnAttribute>().Any())
				toggles.EnableAll();
			if (QueryAllAttributes<AllTogglesOffAttribute>().Any())
				toggles.DisableAll();
			QueryAllAttributes<ToggleAttribute>().ForEach(a => toggles.Enable(a.Toggle));
			QueryAllAttributes<ToggleOffAttribute>().ForEach(a => toggles.Disable(a.Toggle));
			return toggles;
		}

		public FakeConfigReader Config()
		{
			var config = new FakeConfigReader();
			QueryAllAttributes<SettingAttribute>().ForEach(x => config.FakeSetting(x.Setting, x.Value));
			return config;
		}

		public IEnumerable<T> QueryAllAttributes<T>()
		{
			var fromInjectTargets = _injectTo
				.SelectMany(x => x.GetType().GetCustomAttributes(typeof(T), true).Cast<T>());
			var fromTest = _method?.GetCustomAttributes(typeof(T), true).Cast<T>() ?? Enumerable.Empty<T>();
			var fromAssembly = _method?.DeclaringType.Assembly.GetCustomAttributes(typeof(T)).Cast<T>() ?? Enumerable.Empty<T>();
			return fromInjectTargets
				.Union(fromTest)
				.Union(fromAssembly)
				.ToArray();
		}

		public object Resolve(Type targetType)
		{
			return _container.Resolve(targetType);
		}

		public T Resolve<T>()
		{
			return _container.Resolve<T>();
		}

		public void Dispose()
		{
			_container?.Dispose();
		}
	}
}