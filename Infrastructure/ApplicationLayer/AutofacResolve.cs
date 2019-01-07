using System;
using System.Collections.Generic;
using System.Linq;
using Autofac;
using Autofac.Core;
using Castle.DynamicProxy;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class AutofacResolve : IResolve
	{
		private readonly ILifetimeScope _lifetimeScope;

		public AutofacResolve(ILifetimeScope lifetimeScope)
		{
			_lifetimeScope = lifetimeScope;
		}

		public T Resolve<T>()
		{
			return _lifetimeScope.Resolve<T>();
		}

		public object Resolve(Type type)
		{
			return _lifetimeScope.Resolve(type);
		}

		public IResolve NewScope()
		{
			return new AutofacResolve(_lifetimeScope.BeginLifetimeScope());
		}

		public IEnumerable<Type> ConcreteTypesFor(Type type)
		{
			return _lifetimeScope
				.ComponentRegistry
				.RegistrationsFor(new TypedService(type))
				.Select(r =>
				{
					var handlerType = r.Activator.LimitType;
					return ProxyUtil.IsProxyType(handlerType)
						? handlerType.BaseType
						: handlerType;
				});
		}

		public void Dispose()
		{
			_lifetimeScope.Dispose();
		}
	}
}