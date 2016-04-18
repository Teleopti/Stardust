using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ISchedulingOptionsProvider
	{
		ISchedulingOptions Fetch();
	}
}