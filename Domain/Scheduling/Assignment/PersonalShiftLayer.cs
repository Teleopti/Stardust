using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class PersonalShiftLayer : ShiftLayer
	{
		public PersonalShiftLayer(IActivity activity, DateTimePeriod period)
			:base(activity, period)
		{
		}

		protected PersonalShiftLayer()
		{
		}
	}
}