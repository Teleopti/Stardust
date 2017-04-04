using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class MainShiftLayer : ShiftLayer
	{
		public MainShiftLayer(IActivity activity, DateTimePeriod period)
			:base(activity, period)
		{
		}
		protected MainShiftLayer()
		{
		}
	}
}