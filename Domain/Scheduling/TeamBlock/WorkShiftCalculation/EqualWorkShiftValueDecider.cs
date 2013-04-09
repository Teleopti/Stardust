using System;
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
			if (_trueFalseRandomizer.Randomize((int) DateTime.Now.TimeOfDay.Ticks))
				return cache1;

			return cache2;
		}
	}
}