using System;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Aop
{
	[AttributeUsage(AttributeTargets.Method)]
	public class PreActionAuditAttribute : AspectAttribute
	{
		public PreActionAuditAttribute() : base(typeof(PreActionAuditAspect))
		{ 
		}
	}

	[AttributeUsage(AttributeTargets.Method)]
	public class PostActionAuditAttribute : AspectAttribute
	{
		public PostActionAuditAttribute() : base(typeof(PostActionAuditAspect))
		{
		}
	}
}