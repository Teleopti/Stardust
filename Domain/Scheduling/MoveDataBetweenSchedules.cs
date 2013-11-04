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
            foreach (var part in sourceParts)
            {
                var sourceData = destination[part.Person].ScheduledDay(part.DateOnlyAsPeriod.DateOnly);
				IList<IPersistableScheduleData> absencesNextDay = new List<IPersistableScheduleData>();
	            var currentDestinationAss = sourceData.PersonAssignment(true);



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
					if(dataToExport is PersonAbsence && !dataToExport.Period.Intersect(part.Period))
						continue;
									//remove me when #25559 is fixed////
	                var dataToExportAsAss = dataToExport as IPersonAssignment;
									if (dataToExportAsAss != null)
									{
										currentDestinationAss.FillWithDataFrom(dataToExportAsAss);
										sourceData.Add(currentDestinationAss);
										continue;
									}
									///////
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

                ruleBreaks.AddRange(modifyDestination(destination, sourceData));
            }
            return ruleBreaks;
        }

        private IEnumerable<IBusinessRuleResponse> modifyDestination(IScheduleDictionary destination, IScheduleDay sourceData)
        {
            var ruleBreaks = destination.Modify(ScheduleModifier.ScenarioExport, sourceData, _newBusinessRules, _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
            if(ruleBreaks.Count()>0)
            {
                destination.Modify(ScheduleModifier.ScenarioExport, sourceData, NewBusinessRuleCollection.Minimum(), _scheduleDayChangeCallback, new ScheduleTagSetter(NullScheduleTag.Instance));
            }
            return ruleBreaks;
        }
    }
}
