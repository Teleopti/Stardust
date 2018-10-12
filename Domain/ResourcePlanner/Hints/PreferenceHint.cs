using System.Linq;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class PreferenceHint : IScheduleHint
	{
		private readonly IRestrictionOverLimitDecider _restrictionOverLimitDecider;
		private readonly MatrixListFactory _matrixListFactory;

		public PreferenceHint(IRestrictionOverLimitDecider restrictionOverLimitDecider, MatrixListFactory matrixListFactory)
		{
			_restrictionOverLimitDecider = restrictionOverLimitDecider;
			_matrixListFactory = matrixListFactory;
		}

		public void FillResult(HintResult hintResult, HintInput input)
		{
			if (input.Schedules == null)
				return;
			
			var matrixes = _matrixListFactory.CreateMatrixListForSelection(input.Schedules, input.People, input.Period);

			foreach (var matrix in matrixes)
			{
				if (_restrictionOverLimitDecider.PreferencesOverLimit(new Percent(1), matrix).BrokenDays.Any())
				{
					hintResult.Add(new PersonHintError(matrix.Person){ErrorResource = nameof(Resources.AgentScheduledWithoutPreferences)}, GetType(),ValidationResourceType.Preferences);					
				}
			}
		}
	}
}