using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Sikuli.Validators
{
	public class IntervalBalanceBeforeValidator : ISikuliValidator
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly IAggregateSkill _totalSkill;

		public IntervalBalanceBeforeValidator(ISchedulerStateHolder schedulerState, IAggregateSkill totalSkill)
		{
			_schedulerState = schedulerState;
			_totalSkill = totalSkill;
		}

		public SikuliValidationResult Validate()
		{

			var result = new SikuliValidationResult(true);
			var lowestIntervalBalances = ValidatorHelper.GetDailyLowestIntraIntervalBalanceForPeriod(_schedulerState, _totalSkill.AggregateSkills[1]);
			if (lowestIntervalBalances == null)
			{
				result.Result = false;
				result.Details.AppendLine("Validator failure");
			}
			var checkResult = lowestIntervalBalances != null && lowestIntervalBalances.Any(c => c > 0 && c < 0.8);
			result.Details.AppendLine("Details:");
			if (checkResult)
				result.Details.AppendLine("Lowest intra interva balance: OK");
			else
			{
				result.Details.AppendLine("Lowest intra interva balance: Fail");
				result.Result = false;
			}
			return result;
		}
	}
}
