using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Logon
{
	public class AsSystemAttribute : AspectAttribute
	{
		public AsSystemAttribute() : base(typeof(AsSystemAspect))
		{
		}
	}
}