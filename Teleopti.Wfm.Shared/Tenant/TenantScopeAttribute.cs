using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Logon.Aspects
{
	public class TenantScopeAttribute : AspectAttribute
	{
		public TenantScopeAttribute() : base(typeof(TenantScopeAspect))
		{
		}
	}
}