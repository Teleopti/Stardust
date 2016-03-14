using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IResolve : IDisposable
	{
		object Resolve(Type type);
		IResolve NewScope();
		IEnumerable<Type> ConcreteTypesFor(Type componentType);
	}
}