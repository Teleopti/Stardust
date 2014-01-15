using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class MainShiftLayer : ShiftLayer, IMainShiftLayer
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