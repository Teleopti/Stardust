using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Web.Areas.People.Core.Aspects
{
	public class AuditPersonAttribute : AspectAttribute
	{
		public AuditPersonAttribute() : base(typeof(AuditPersonAspect))
		{
		}
	}
}
