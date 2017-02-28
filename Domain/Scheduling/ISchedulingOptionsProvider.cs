using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public interface ISchedulingOptionsProvider
	{
		ISchedulingOptions Fetch();
	}
}