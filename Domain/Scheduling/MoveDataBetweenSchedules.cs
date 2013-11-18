using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Interfaces.Domain;
using System.Linq;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class MoveDataBetweenSchedules : IMoveDataBetweenSchedules
	{
		private readonly INewBusinessRuleCollection _newBusinessRules;
		private readonly IScheduleDayChangeCallback _scheduleDayChangeCallback;

		public MoveDataBetweenSchedules(INewBusinessRuleCollection newBusinessRules, IScheduleDayChangeCallback scheduleDayChangeCallback)
		{
			_newBusinessRules = newBusinessRules;
			_scheduleDayChangeCallback = scheduleDayChangeCallback;
		}

		public IEnumerable<IBusinessRuleResponse> CopySchedulePartsToAnotherDictionary(IScheduleDictionary destination, IEnumerable<IScheduleDay> sourceParts)
		{
			var ruleBreaks = new List<IBusinessRuleResponse>();
			foreach (var sourcePart in sourceParts)
			{
				var destinationDay = destination[sourcePart.Person].ScheduledDay(sourcePart.DateOnlyAsPeriod.DateOnly);
				var currentDestinationAss = destinationDay.PersonAssignment(true);

				//put aside absence outside current day
				var absencesNextDay = destinationDay.PersonAbsenceCollection(true)
					.Where(dataInDestination => isAbsenceInsideScheduleDay(dataInDestination, sourcePart)).ToList();

				destinationDay.Clear<IExportToAnotherScenario>();
				foreach (var dataToExport in sourcePart.PersistableScheduleDataCollection().OfType<IExportToAnotherScenario>())
				{
					if (isAbsenceInsideScheduleDay(dataToExport, sourcePart))
						continue;
					//remove me when #25559 is fixed////
					var dataToExportAsAss = dataToExport as IPersonAssignment;
					if (dataToExportAsAss != null)
					{
						currentDestinationAss.FillWithDataFrom(dataToExportAsAss);
						destinationDay.Add(currentDestinationAss);
						continue;
					}
					///////
					var clonedWithNewParameters = dataToExport.CloneAndChangeParameters(destinationDay);
					destinationDay.Add(clonedWithNewParameters);
				}

				//put back absence outside current day
				foreach (var clonedWithNewParameters in absencesNextDay.Select(dataInDestination => dataInDestination.CloneAndChangeParameters(destinationDay)))
				{
					destinationDay.Add(clonedWithNewParameters);
				}

				ruleBreaks.AddRange(modifyDestination(destination, destinationDay));
			}
			return ruleBreaks;
		}

		private static bool isAbsenceInsideScheduleDay(INonversionedPersistableScheduleData dataInDestination, IScheduleDay scheduleDay)
		{
			return dataInDestination is IPersonAbsence && !dataInDestination.Period.Intersect(scheduleDay.Period);
		}

		private IEnumerable<IBusinessRuleResponse> modifyDestination(IScheduleDictionary destination, IScheduleDay sourceData)
		{
			var ruleBreaks = destination.Modify(ScheduleModifier.ScenarioExport, sourceData, _newBusinessRules, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
			if (ruleBreaks.Any())
			{
				destination.Modify(ScheduleModifier.ScenarioExport, sourceData, NewBusinessRuleCollection.Minimum(), _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
			}
			return ruleBreaks;
		}
	}
}
