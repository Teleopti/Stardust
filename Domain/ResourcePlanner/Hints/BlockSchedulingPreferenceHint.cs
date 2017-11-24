using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class BlockSchedulingPreferenceHint : IScheduleHint
	{
		public void FillResult(HintResult hintResult, HintInput input)
		{
			var usePreferenes = input.UsePreferences;
			if (!usePreferenes || input.Schedules==null)
				return;
			var people = input.People;
			var period = input.Period;
			var blockPreferenceProvider = input.BlockPreferenceProvider;
			foreach (var person in people)
			{
				var blockOption = blockPreferenceProvider.ForAgent(person, period.StartDate);
				if (!blockOption.UseTeamBlockOption) continue;
				var scheduleForPerson = input.Schedules[person];

				var scheduleDays = scheduleForPerson.ScheduledDayCollection(period);
				foreach (var scheduleDay in scheduleDays)
				{
					if (!scheduleDay.IsScheduled())
					{
						var preference = scheduleDay.RestrictionCollection().OfType<IPreferenceRestriction>().SingleOrDefault();
						if (preference != null)
						{
							addValidationError(hintResult, person, nameof(Resources.BlockSchedulingNotWorkingWhenUsingPreferences));
							break;
						}
					}
				}
			}
		}

		private void addValidationError(HintResult hintResult, IPerson person, string resourceString, params object[] resourceData)
		{
			hintResult.Add(new PersonHintError
			{
				PersonName = person.Name.ToString(),
				PersonId = person.Id.Value,
				ErrorResource = resourceString,
				ErrorResourceData = resourceData.ToList()
			}, GetType(), ValidationResourceType.BlockScheduling);
		}
	}
}