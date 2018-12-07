using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class PreferenceHint : ISchedulePostHint
	{
		private readonly IRestrictionOverLimitDecider _restrictionOverLimitDecider;
		private readonly MatrixListFactory _matrixListFactory;

		public PreferenceHint(IRestrictionOverLimitDecider restrictionOverLimitDecider, MatrixListFactory matrixListFactory)
		{
			_restrictionOverLimitDecider = restrictionOverLimitDecider;
			_matrixListFactory = matrixListFactory;
		}

		public void FillResult(HintResult hintResult, SchedulePostHintInput input)
		{
			var matrixes = _matrixListFactory.CreateMatrixListForSelection(input.Schedules, input.People, input.Period);

			foreach (var matrix in matrixes)
			{
				if (_restrictionOverLimitDecider.PreferencesOverLimit(new Percent(input.PreferencesValue), matrix).BrokenDays.Any()&&matrix.IsFullyScheduled())
				{
					if (!hintResult.InvalidResources.Any(x =>x.ResourceId == matrix.Person.Id && x.ValidationErrors.Any(y => y.ResourceType == ValidationResourceType.Preferences)))
					{
						hintResult.Add(new PersonHintError(matrix.Person){ErrorResource = nameof(Resources.AgentScheduledWithoutPreferences)}, GetType(),ValidationResourceType.Preferences);	
					}
				}
			}
		}
	}
}