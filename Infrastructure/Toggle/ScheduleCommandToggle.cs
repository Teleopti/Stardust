using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Infrastructure.Toggle
{
	public class ScheduleCommandToggle : IScheduleCommandToggle
	{
		private readonly IToggleManager _toggleManager;

		public ScheduleCommandToggle(IToggleManager toggleManager)
		{
			_toggleManager = toggleManager;
		}

		public bool IsEnabled(Toggles toggle)
		{
			return _toggleManager.IsEnabled(toggle);
		}
	}
}