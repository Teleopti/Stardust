using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class OvertimeShiftLayer : ShiftLayer, IOvertimeShiftLayer
	{
		public OvertimeShiftLayer(IActivity activity, DateTimePeriod period, IMultiplicatorDefinitionSet multiplicatorDefinitionSet)
			:base(activity, period)
		{
			DefinitionSet = multiplicatorDefinitionSet;
		}

		protected OvertimeShiftLayer()
		{
		}

		public virtual IMultiplicatorDefinitionSet DefinitionSet { get; protected set; }
	}
}