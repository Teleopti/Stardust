using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using System.Linq;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	[RemoveMeWithToggle(Toggles.ResourcePlanner_HideExportSchedule_81161)]
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
			var rangeClones = new Dictionary<IPerson, IScheduleRange>();
			var persList = new HashSet<IPerson>();
			IList<IScheduleDay> sourceDataList = new List<IScheduleDay>();

			foreach (var sourcePart in sourceParts)
			{
				persList.Add(sourcePart.Person);
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

				sourceDataList.Add(destinationDay);
				destination.Modify(ScheduleModifier.ScenarioExport, destinationDay, NewBusinessRuleCollection.Minimum(), _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
			}

			foreach (var person in persList)
			{
				rangeClones.Add(person, (IScheduleRange)destination[person].Clone());
			}

			return _newBusinessRules.CheckRules(rangeClones, sourceDataList);
		}

		private static bool isAbsenceInsideScheduleDay(IPersistableScheduleData dataInDestination, IScheduleDay scheduleDay)
		{
			return dataInDestination is IPersonAbsence && !dataInDestination.Period.Intersect(scheduleDay.Period);
		}
	}
}
