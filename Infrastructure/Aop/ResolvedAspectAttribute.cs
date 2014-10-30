using System;

namespace Teleopti.Ccc.Infrastructure.Aop
{
	public abstract class ResolvedAspectAttribute : Attribute, IResolvedAspectAttribute
	{
	    protected ResolvedAspectAttribute(Type aspectType) { AspectType = aspectType; }
		public int Order { get; set; }
		public Type AspectType { get; set; }
	}
}