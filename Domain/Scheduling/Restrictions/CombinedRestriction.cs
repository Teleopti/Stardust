using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class CombinedRestriction : IWorkTimeMinMaxRestriction
	{
		private readonly IWorkTimeMinMaxRestriction _one;
		private readonly IWorkTimeMinMaxRestriction _two;

		public CombinedRestriction(IWorkTimeMinMaxRestriction one, IWorkTimeMinMaxRestriction two)
		{
			_one = one;
			_two = two;
		}

		public bool MayMatch()
		{
			return _one.MayMatch() && _two.MayMatch();
		}

		public bool MayMatchBlacklistedShifts()
		{
			return _one.MayMatchBlacklistedShifts() || _two.MayMatchBlacklistedShifts();
		}

		public bool Match(IShiftCategory shiftCategory)
		{
			return _one.Match(shiftCategory) && _two.Match(shiftCategory);
		}

		public bool Match(IWorkShiftProjection workShiftProjection)
		{
			return _one.Match(workShiftProjection) && _two.Match(workShiftProjection);
		}
	}
}