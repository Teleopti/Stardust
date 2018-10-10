using System;
using Teleopti.Ccc.Domain.Aop.Core;
using Teleopti.Ccc.Web.Areas.Global;

namespace Teleopti.Ccc.Web.Areas.People.Core.Aspects
{
	public class AuditPersonAttribute : AspectAttribute
	{
		public AuditPersonAttribute() : base(typeof(AuditPersonAspect))
		{
		}
	}
}
