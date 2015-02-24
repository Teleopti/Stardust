using Teleopti.Ccc.Domain.FeatureFlags;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface IScheduleCommandToggle
	{
		bool IsEnabled(Toggles toggle);
	}
}
