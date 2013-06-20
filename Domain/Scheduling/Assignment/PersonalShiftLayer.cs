using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class PersonalShiftLayer : ShiftLayer, IPersonalShiftLayer
	{
		public PersonalShiftLayer(IActivity activity, DateTimePeriod period)
			:base(activity, period)
		{
		}

		public virtual int OrderIndex
		{
			get
			{
				var ass = Parent as IPersonAssignment;
				if (ass == null)
					return -1;
				return ass.PersonalLayers.ToList().IndexOf(this);
			}
		}
	}
}