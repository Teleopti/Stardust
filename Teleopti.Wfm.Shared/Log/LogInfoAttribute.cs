using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Aop
{
	public class LogInfoAttribute : AspectAttribute
	{
		public LogInfoAttribute() : base(typeof(LogInfoAspect))
		{
		}
	}
}