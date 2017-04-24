using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IEqualWorkShiftValueDecider
	{
		ShiftProjectionCache Decide(ShiftProjectionCache cache1, ShiftProjectionCache cache2);
	}

	public class EqualWorkShiftValueDecider : IEqualWorkShiftValueDecider
	{
		private readonly ITrueFalseRandomizer _trueFalseRandomizer;

		public EqualWorkShiftValueDecider(ITrueFalseRandomizer trueFalseRandomizer)
		{
			_trueFalseRandomizer = trueFalseRandomizer;
		}

		public ShiftProjectionCache Decide(ShiftProjectionCache cache1, ShiftProjectionCache cache2)
		{
			return _trueFalseRandomizer.Randomize() ? cache1 : cache2;
		}
	}
}