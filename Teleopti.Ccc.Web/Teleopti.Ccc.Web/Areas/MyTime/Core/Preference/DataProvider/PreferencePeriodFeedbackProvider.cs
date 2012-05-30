using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Preference.DataProvider
{
	public class PreferencePeriodFeedbackProvider : IPreferencePeriodFeedbackProvider
	{
		private readonly IVirtualSchedulePeriodProvider _virtualSchedulePeriodProvider;

		public PreferencePeriodFeedbackProvider(IVirtualSchedulePeriodProvider virtualSchedulePeriodProvider) {
			_virtualSchedulePeriodProvider = virtualSchedulePeriodProvider;
		}

		public int ShouldHaveDaysOff(DateOnly date)
		{
			return _virtualSchedulePeriodProvider.VirtualSchedulePeriodForDate(date).DaysOff();
		}
	}
}