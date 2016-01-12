using System;

namespace Teleopti.Ccc.Domain.Aop.Core
{
	public interface IAttributeForAspect
	{
		Type AspectType { get; }
		int Order { get; }
	}
}