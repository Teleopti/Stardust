using System;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Aop
{
	[AttributeUsage(AttributeTargets.Method)]
	public class AuditTrailAttribute : AspectAttribute
	{
		public AuditTrailAttribute() : base(typeof(AuditTrailAspect))
		{
		}

	}
}