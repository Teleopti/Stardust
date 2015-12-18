using Teleopti.Ccc.Domain.Aop.Core;

namespace Teleopti.Ccc.Domain.Common.TimeLogger
{
	public sealed class LogTimeAttribute : AspectAttribute
	{
		public LogTimeAttribute() : base(typeof(LogTimeAspect))
		{
			
		} 
	}
}