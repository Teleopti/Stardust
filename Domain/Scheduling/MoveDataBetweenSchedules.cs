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
			var rangeClones = new Dictionary<IPerson, IScheduleRange>();
			var persList = new HashSet<IPerson>();
			IList<IScheduleDay> sourceDataList = new List<IScheduleDay>();

			foreach (var part in sourceParts)
			{
				persList.Add(part.Person);
				var sourceData = destination[part.Person].ScheduledDay(part.DateOnlyAsPeriod.DateOnly);
				IList<IPersistableScheduleData> absencesNextDay = new List<IPersistableScheduleData>();

				foreach (var dataInDestination in sourceData.PersistableScheduleDataCollection())
				{
					if (dataInDestination is IPersonAbsence && !dataInDestination.Period.Intersect(part.Period))
					{
						absencesNextDay.Add(dataInDestination);
					}
				}

				sourceData.Clear<IExportToAnotherScenario>();
				foreach (var dataToExport in part.PersistableScheduleDataCollection().OfType<IExportToAnotherScenario>())
				{
					// bug #22073, the part contains PersonsAbsence for the next day too, for some reason, so we skip them
					if (dataToExport is PersonAbsence && !dataToExport.Period.Intersect(part.Period))
						continue;
					var clonedWithNewParameters = dataToExport.CloneAndChangeParameters(sourceData);
					sourceData.Add(clonedWithNewParameters);
				}

				foreach (var dataInDestination in absencesNextDay.OfType<IExportToAnotherScenario>())
				{
					if (dataInDestination is IPersonAbsence)
					{
						var clonedWithNewParameters = dataInDestination.CloneAndChangeParameters(sourceData);
						sourceData.Add(clonedWithNewParameters);
					}
				}

				sourceDataList.Add(sourceData);
				destination.Modify(ScheduleModifier.ScenarioExport, sourceData, NewBusinessRuleCollection.Minimum(), _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
			}

			foreach (var person in persList)
			{
				rangeClones.Add(person, (IScheduleRange)destination[person].Clone());
			}

			return _newBusinessRules.CheckRules(rangeClones, sourceDataList);
		}    
    }
}
