﻿using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages.General;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public class MultisiteForecastToSkillCommand : IMultisiteForecastToSkillCommand
    {
        private readonly IImportForecastToSkillCommand _importForecastToSkillCommand;
        private readonly ISkillDayLoadHelper _skillDayLoadHelper;
        private readonly IScenarioRepository _scenarioRepository;
        private readonly IJobResultFeedback _feedback;

		  public MultisiteForecastToSkillCommand(IImportForecastToSkillCommand importForecastToSkillCommand, ISkillDayLoadHelper skillDayLoadHelper, IScenarioRepository scenarioRepository, IJobResultFeedback feedback)
        {
            _importForecastToSkillCommand = importForecastToSkillCommand;
            _skillDayLoadHelper = skillDayLoadHelper;
            _scenarioRepository = scenarioRepository;
            _feedback = feedback;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.String.Format(System.String,System.Object)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "subskill")]
		public void Execute(ISkillExportSelection skillSelection)
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
                        _importForecastToSkillCommand.Execute(skillExportCombination.SourceSkill, skillExportCombination.TargetSkill, skillStaffPeriods, skillSelection.Period);
                    }
                }
            }
        }
    }

    public interface ISendBusMessage
    {
        void Process(IEnumerable<IForecastsRow> importForecast, ISkill targetSkill, DateOnlyPeriod period);
    }

    public interface IMultisiteForecastToSkillCommand
    {
        void Execute(ISkillExportSelection skillSelection);
    }
}