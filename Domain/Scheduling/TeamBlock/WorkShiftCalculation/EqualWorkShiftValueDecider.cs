using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.WorkShiftCalculation
{
	public interface IEqualWorkShiftValueDecider
	{
		IShiftProjectionCache Decide(IShiftProjectionCache cache1, IShiftProjectionCache cache2);
	}

	public class EqualWorkShiftValueDecider : IEqualWorkShiftValueDecider
	{
		private readonly ITrueFalseRandomizer _trueFalseRandomizer;

		public EqualWorkShiftValueDecider(ITrueFalseRandomizer trueFalseRandomizer)
		{
			_trueFalseRandomizer = trueFalseRandomizer;
		}

		public IShiftProjectionCache Decide(IShiftProjectionCache cache1, IShiftProjectionCache cache2)
		{
			return _trueFalseRandomizer.Randomize() ? cache1 : cache2;
		}
	}
}