using System;
using System.Collections.Generic;
using AutoMapper;
using NUnit.Framework;

namespace Teleopti.Ccc.WebTest.Core.Mapping
{
	public class TestMappingDependencyResolver
	{
		private readonly Dictionary<Type, Func<object>> _registry = new Dictionary<Type, Func<object>>();

		public TestMappingDependencyResolver() { Register(() => Mapper.Engine); }

		public TestMappingDependencyResolver Register<T>(T instance) { return Register(() => instance); }

		public TestMappingDependencyResolver Register<T>(Func<T> constructor)
		{
			_registry.Add(typeof(T), () => constructor.Invoke());
			return this;
		}

		public T Resolve<T>()
		{
			return (T) Resolve(typeof (T));
		}

		public object Resolve(Type type)
		{
			if (_registry.ContainsKey(type))
				return _registry[type].Invoke();
			Assert.Fail("Can not resolve type " + type.AssemblyQualifiedName);
			return null;
		}
	}
}