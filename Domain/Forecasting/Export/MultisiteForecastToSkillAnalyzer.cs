using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.ForecastsFile;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Messages.General;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public class MultisiteForecastToSkillAnalyzer : IMultisiteForecastToSkillCommand
    {
        private readonly ISkillDayLoadHelper _skillDayLoadHelper;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly IJobResultFeedback _feedback;
	    private readonly ISplitImportForecastMessage _splitImportForecastMessage;

	    public MultisiteForecastToSkillAnalyzer( ISkillDayLoadHelper skillDayLoadHelper, IScenarioRepository scenarioRepository, IJobResultFeedback feedback, ISplitImportForecastMessage splitImportForecastMessage)
        {
           _skillDayLoadHelper = skillDayLoadHelper;
            _scenarioRepository = scenarioRepository;
            _feedback = feedback;
		    _splitImportForecastMessage = splitImportForecastMessage;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "subskill")]
		public void Execute(SkillExportSelection skillSelection)
		{
            foreach (var multisiteSkillForExport in skillSelection.MultisiteSkillsForExport)
            {
                _feedback.ReportProgress(0,
					string.Format(CultureInfo.InvariantCulture, "Starting export of {0}.", multisiteSkillForExport.MultisiteSkill.Name));
                var skillDictionary = _skillDayLoadHelper.LoadSchedulerSkillDays(skillSelection.Period,
                                                                                 new[] { multisiteSkillForExport.MultisiteSkill },
                                                                                 _scenarioRepository.LoadDefaultScenario(multisiteSkillForExport.MultisiteSkill.BusinessUnit));
                var skillStaffPeriodHolder = new SkillStaffPeriodHolder(skillDictionary);

				_feedback.Info(string.Format(CultureInfo.InvariantCulture, "Number of subskill mappings: {0}.", multisiteSkillForExport.SubSkillMapping.Count()));
                foreach (var skillExportCombination in multisiteSkillForExport.SubSkillMapping)
                {
					if (!skillExportCombination.TargetSkill.WorkloadCollection.Any())
					{
						_feedback.Warning(string.Format(CultureInfo.InvariantCulture,
						                                         "The skill must have at least one workload. (Ignored skill: {0}).",
						                                         skillExportCombination.TargetSkill.Name));
						continue;
					}

                    _feedback.ReportProgress(0,
                        string.Format(CultureInfo.InvariantCulture, "Processing subskill {0}.", skillExportCombination.SourceSkill.Name));
                    ISkillStaffPeriodDictionary skillStaffPeriods;
                    if (skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(skillExportCombination.SourceSkill, out skillStaffPeriods))
                    {
                        _feedback.ReportProgress(0,
							string.Format(CultureInfo.InvariantCulture, "Saving forecast to {0}.", skillExportCombination.TargetSkill.Name));
                        analyzeForecast(skillExportCombination.SourceSkill, skillExportCombination.TargetSkill, skillStaffPeriods, skillSelection.Period);
                    }
                }
            }
        }

	    private void analyzeForecast(IChildSkill sourceSkill, ISkill targetSkill, ISkillStaffPeriodDictionary skillStaffPeriods, DateOnlyPeriod period)
	    {
			var result = new List<IForecastsRow>();
			foreach (var skillStaffPeriod in skillStaffPeriods.Values)
			{
				result.Add(new ForecastsRow
				{
					LocalDateTimeFrom = skillStaffPeriod.Period.StartDateTimeLocal(sourceSkill.TimeZone),
					LocalDateTimeTo = skillStaffPeriod.Period.EndDateTimeLocal(sourceSkill.TimeZone),
					UtcDateTimeFrom = skillStaffPeriod.Period.StartDateTime,
					UtcDateTimeTo = skillStaffPeriod.Period.EndDateTime,
					SkillName = sourceSkill.Name,
					Tasks = skillStaffPeriod.Payload.TaskData.Tasks,
					TaskTime = skillStaffPeriod.Payload.TaskData.AverageTaskTime.TotalSeconds,
					AfterTaskTime = skillStaffPeriod.Payload.TaskData.AverageAfterTaskTime.TotalSeconds,
					Agents = skillStaffPeriod.Payload.ForecastedIncomingDemand,
					Shrinkage = skillStaffPeriod.Payload.Shrinkage.Value
				});
			}

			_splitImportForecastMessage.Process(result, targetSkill, period);
		}
    }

    public interface ISplitImportForecastMessage
    {
        void Process(IEnumerable<IForecastsRow> importForecast, ISkill targetSkill, DateOnlyPeriod period);
    }

    public interface IMultisiteForecastToSkillCommand
    {
        void Execute(SkillExportSelection skillSelection);
    }
}