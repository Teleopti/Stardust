using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IResolve
	{
		object Resolve(Type type);
	}
}