using System.Linq;
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

		public virtual int OrderIndex
		{
			get
			{
				var ass = Parent as IPersonAssignment;
				if (ass == null)
					return -1;
				return ass.MainLayers.ToList().IndexOf(this);
			}
		}
	}
}