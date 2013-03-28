using System;

namespace Teleopti.Ccc.Infrastructure.ApplicationLayer
{
	public interface IResolve
	{
		object Resolve(Type type);
	}
}