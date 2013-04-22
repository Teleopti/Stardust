using System;
using Autofac;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Autofac")]
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