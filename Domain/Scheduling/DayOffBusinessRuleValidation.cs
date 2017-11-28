using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class DayOffBusinessRuleValidation
	{
		public bool Validate(TargetScheduleSummary targetSummary, CurrentScheduleSummary currentSummary)
		{
			if (!targetSummary.TargetDaysOff.HasValue)
				return false;

			var scheduledDaysOff = currentSummary.NumberOfDaysOff;
			return targetSummary.TargetDaysOff.Value - targetSummary.NegativeTargetDaysOffTolerance <= scheduledDaysOff &&
				targetSummary.TargetDaysOff.Value + targetSummary.PositiveTargetDaysOffTolerance >= scheduledDaysOff;
		}
	}
}