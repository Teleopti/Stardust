using System;

namespace Teleopti.Ccc.Web.Core.Aop.Core
{
	public  class ResolvedAspectAttribute : Attribute, IResolvedAspectAttribute
	{
		public ResolvedAspectAttribute(Type aspectType) { AspectType = aspectType; }
		public int Order { get; set; }
		public Type AspectType { get; set; }
	}
}