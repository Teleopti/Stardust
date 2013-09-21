using System;

namespace Teleopti.Ccc.Web.Core.Aop.Core
{
	public abstract class ResolvedAspectAttribute : Attribute, IResolvedAspectAttribute
	{
	    protected ResolvedAspectAttribute(Type aspectType) { AspectType = aspectType; }
		public int Order { get; set; }
		public Type AspectType { get; set; }
	}
}