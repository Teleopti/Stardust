using System;

namespace Teleopti.Ccc.Domain.Aop.Core
{
	public interface IResolvedAspect
	{
		Type AspectType { get; }
	}
}