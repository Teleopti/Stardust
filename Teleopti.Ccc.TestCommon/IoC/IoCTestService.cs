using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using NUnit.Framework.Interfaces;
using Teleopti.Ccc.Domain.Collection;

namespace Teleopti.Ccc.TestCommon.IoC
{
	public class IoCTestService
	{
		private IContainer _container;
		private readonly object _attribute;
		private readonly Type _fixtureType;
		private readonly MethodInfo _method;

		public IoCTestService(ITest testDetails, object attribute)
		{
			_attribute = attribute;
			Fixture = testDetails.Fixture;
			_fixtureType = Fixture.GetType();
			_method = testDetails.Method.MethodInfo;
		}

		public object Fixture { get; }

		public void InjectFrom(IContainer container)
		{
			_container = container;
			injectMembers(_attribute.GetType(), _attribute);
			injectMembers(_fixtureType, Fixture);
		}
		
		private void injectMembers(IReflect type, object instance)
		{
			var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(x => x.CanWrite);
			properties.ForEach(x => x.SetValue(instance, _container.Resolve(x.PropertyType), null));
			var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
			fields.ForEach(x => x.SetValue(instance, _container.Resolve(x.FieldType)));
		}

		public IEnumerable<T> QueryAllAttributes<T>()
		{
			var fromFixture = _fixtureType.GetCustomAttributes(typeof(T), true).Cast<T>();
			var fromTest = _method.GetCustomAttributes(typeof(T), true).Cast<T>();
			var fromAttribute = _attribute.GetType().GetCustomAttributes(typeof(T), true).Cast<T>();
			return fromFixture
				.Union(fromTest)
				.Union(fromAttribute)
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
	}
}