using System;
using Autofac;
using Teleopti.Ccc.Domain.ApplicationLayer;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public class AutofacResolve : IResolve
	{
		private readonly IComponentContext _componentContext;

		public AutofacResolve(IComponentContext componentContext) {
			_componentContext = componentContext;
		}

		public object Resolve(Type type)
		{
			return _componentContext.Resolve(type);
		}
	}
}