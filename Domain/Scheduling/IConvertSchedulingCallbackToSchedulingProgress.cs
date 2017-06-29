using Teleopti.Ccc.Domain.Helper;

namespace Teleopti.Ccc.Domain.Scheduling
{
	//should not be needed, but good for now
	public interface IConvertSchedulingCallbackToSchedulingProgress
	{
		ISchedulingProgress Convert();
	}
}