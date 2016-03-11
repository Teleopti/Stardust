using System;
using Autofac;
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

		public object Resolve(Type type)
		{
			return _lifetimeScope.Resolve(type);
		}

		public IResolve NewScope()
		{
			return new AutofacResolve(_lifetimeScope.BeginLifetimeScope());
		}

		public void Dispose()
		{
			_lifetimeScope.Dispose();
		}
	}
}