using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class PersonalShiftLayer : ShiftLayer, IPersonalShiftLayer
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