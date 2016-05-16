using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Logon
{
	public class ImpersonateSystemAttribute : AspectAttribute
	{
		public ImpersonateSystemAttribute() : base(typeof(ImpersonateSystemAspect))
		{
		}
	}
}