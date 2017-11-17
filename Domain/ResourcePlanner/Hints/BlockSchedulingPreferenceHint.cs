using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class BlockSchedulingPreferenceHint : IScheduleHint
	{
		public void FillResult(HintResult hintResult, HintInput input)
		{
			var usePreferenes = input.UsePreferences;
			if (!usePreferenes)
				return;
			var people = input.People;
			var period = input.Period;
			var blockPreferenceProvider = input.BlockPreferenceProvider;
			var scheduleDictionary = input.Schedules ?? input.CurrentSchedule;
			foreach (var person in people)
			{
				var blockOption = blockPreferenceProvider.ForAgent(person, period.StartDate);
				if (!blockOption.UseTeamBlockOption) continue;
				var scheduleForPerson = scheduleDictionary[person];

				var scheduleDays = scheduleForPerson.ScheduledDayCollection(period);
				foreach (var scheduleDay in scheduleDays)
				{
					if (!scheduleDay.IsScheduled())
					{
						var preference = scheduleDay.RestrictionCollection().OfType<IPreferenceRestriction>().SingleOrDefault();
						if (preference != null)
						{
							addValidationError(hintResult, person, "Block scheduling may not work when using preference.");
							break;
						}
					}
				}
			}
		}

		private void addValidationError(HintResult hintResult, IPerson person, string message)
		{
			hintResult.Add(new PersonHintError
			{
				PersonName = person.Name.ToString(),
				PersonId = person.Id.Value,
				ValidationError = message
			}, GetType(), ValidationResourceType.BlockScheduling);
		}
	}
}