using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Logon
{
	public class FullPermissionsAttribute : AspectAttribute
	{
		public FullPermissionsAttribute() : base(typeof(FullPermissionsAspect))
		{
		}
	}
}