using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using System.Linq;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators
{
	public class ScheduleOvertimeValidator : ISikuliValidator
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IAggregateSkill _totalSkill;

		public ScheduleOvertimeValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill)
		{
			_schedulerState = schedulerState;
			_totalSkill = totalSkill;
		}

		public SikuliValidationResult Validate()
		{
			var requestedDay = new DateOnly(2011, 04, 05);
			var result = new SikuliValidationResult(SikuliValidationResult.ResultValue.Pass);
			var lowestIntervalBalances = ValidatorHelper.GetStandardDeviationForDay(_schedulerState, _totalSkill.AggregateSkills[1], requestedDay);
			if (lowestIntervalBalances == null)
			{
				result.Result = SikuliValidationResult.ResultValue.Fail;
				result.Details.AppendLine("Validator failure");
				return result;
			}
			return result;
		}
	}
}
