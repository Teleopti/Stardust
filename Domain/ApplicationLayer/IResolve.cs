using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IResolve : IDisposable
	{
		object Resolve(Type type);
		IResolve NewScope();
	}
}