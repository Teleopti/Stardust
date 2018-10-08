using System;
using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Web.Areas.People.Core.Aspects
{
	public class AuditPersonAttribute : AspectAttribute
	{
		public AuditPersonAttribute() : base(typeof(AuditPersonAspect))
		{
		}
	}

	public class AuditTrailAttribute : AspectAttribute
	{
		//version1
		//public AuditTrailAttribute(Type type) : base(typeof(AuditTrailAspect))
		//{
		//	AuditContextType = type;
		//}

		//public Type AuditContextType { get; }

		public AuditTrailAttribute() : base(typeof(AuditTrailAspect))
		{
		}

	}
}
