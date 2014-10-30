using System;

namespace Teleopti.Ccc.Infrastructure.Aop
{
	public interface IResolvedAspect
	{
		Type AspectType { get; }
	}
}