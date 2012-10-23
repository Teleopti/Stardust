using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Restrictions
{
	public class CombinedRestriction : IWorkTimeMinMaxRestriction
	{
		public IWorkTimeMinMaxRestriction One { get; private set; }
		public IWorkTimeMinMaxRestriction Two { get; private set; }

		public CombinedRestriction(IWorkTimeMinMaxRestriction one, IWorkTimeMinMaxRestriction two)
		{
			One = one;
			Two = two;
		}

		public bool MayMatchWithShifts()
		{
			return One.MayMatchWithShifts() && Two.MayMatchWithShifts();
		}

		public bool MayMatchBlacklistedShifts()
		{
			return One.MayMatchBlacklistedShifts() || Two.MayMatchBlacklistedShifts();
		}

		public bool Match(IShiftCategory shiftCategory)
		{
			return One.Match(shiftCategory) && Two.Match(shiftCategory);
		}

		public bool Match(IWorkShiftProjection workShiftProjection)
		{
			return One.Match(workShiftProjection) && Two.Match(workShiftProjection);
		}

	}
}