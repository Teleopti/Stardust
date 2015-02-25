using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators
{
	public class DeleteAllValidator : ISikuliValidator
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IAggregateSkill _totalSkill;

		public DeleteAllValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill)
		{
			_schedulerState = schedulerState;
			_totalSkill = totalSkill;
		}

		public string Description
		{
			get { return "All scheduled hours must be 0."; }
		}

		public SikuliValidationResult Validate()
		{
			var result = new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
			var scheduledHours = ValidatorHelper.GetDailyScheduledHoursForFullPeriod(_schedulerState, _totalSkill);
			if (scheduledHours.Any(d => d.HasValue && d.Value > 0))
			{
				result.Details.AppendLine("Scheduled hours = 0 : Fail");
				result.Result = SikuliValidationResult.ResultValue.Fail;
				return result;
			}
			result.Details.AppendLine("Scheduled hours = 0 : OK");
			return result;
		}
	}
}
