using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Web.Areas.Global.Aspect
{
	public class AuditTrailAttribute : AspectAttribute
	{
		public AuditTrailAttribute() : base(typeof(AuditTrailAspect))
		{
		}

	}
}