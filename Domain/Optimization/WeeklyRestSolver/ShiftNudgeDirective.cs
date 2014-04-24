using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
	public class ShiftNudgeDirective
	{
		private readonly IEffectiveRestriction _effectiveRestriction;
		private readonly NudgeDirection _direction;

		public enum NudgeDirection
		{
			Left,
			Right
		}

		public ShiftNudgeDirective()
		{
			_effectiveRestriction = new EffectiveRestriction();
		}

		public ShiftNudgeDirective(IEffectiveRestriction effectiveRestriction, NudgeDirection direction)
		{
			_effectiveRestriction = effectiveRestriction;
			_direction = direction;
		}

		public IEffectiveRestriction EffectiveRestriction
		{
			get { return _effectiveRestriction; }
		}

		public NudgeDirection Direction
		{
			get { return _direction; }
		}
	}
}