using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;

namespace Teleopti.Ccc.Domain.ResourcePlanner.Hints
{
	public class BlockSchedulingPreferenceHint : ISchedulePostHint
	{
		public void FillResult(HintResult hintResult, SchedulePostHintInput input)
		{
			var usePreferenes = input.UsePreferences;
			if (!usePreferenes)
				return;
			var people = input.People;
			var period = input.Period;
			var blockPreferenceProvider = input.BlockPreferenceProvider;
			foreach (var person in people)
			{
				var personPeriods = person.PersonPeriods(input.Period);
				if (personPeriods.Any(x => x.PersonContract.Contract.EmploymentType == EmploymentType.HourlyStaff)) continue;
				var blockOption = blockPreferenceProvider.ForAgent(person, period.StartDate);
				if (!blockOption.UseTeamBlockOption) continue;
				var scheduleForPerson = input.Schedules[person];

				var scheduleDays = scheduleForPerson.ScheduledDayCollection(period);

				bool anyDaysWithoutSchedules = scheduleDays.Any(scheduleDay => !scheduleDay.IsScheduled());

				if (!anyDaysWithoutSchedules)
					continue;
				foreach (var scheduleDay in scheduleDays)
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