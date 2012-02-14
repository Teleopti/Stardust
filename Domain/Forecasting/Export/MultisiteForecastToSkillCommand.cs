using System;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Export
{
    public class MultisiteForecastToSkillCommand : IMultisiteForecastToSkillCommand
    {
        private readonly ISaveForecastToSkillCommand _saveForecastToSkillCommand;
        private readonly ISkillDayLoadHelper _skillDayLoadHelper;
        private readonly IScenarioProvider _scenarioProvider;
        private readonly IJobResultFeedback _jobResultFeedback;
    	private int _progressStep;
        private int _lastProgressStep;

        public MultisiteForecastToSkillCommand(ISaveForecastToSkillCommand saveForecastToSkillCommand, ISkillDayLoadHelper skillDayLoadHelper, IScenarioProvider scenarioProvider, IJobResultFeedback jobResultFeedback)
        {
            _saveForecastToSkillCommand = saveForecastToSkillCommand;
            _skillDayLoadHelper = skillDayLoadHelper;
            _scenarioProvider = scenarioProvider;
            _jobResultFeedback = jobResultFeedback;
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "subskill")]
		public void Execute(ISkillExportSelection skillSelection)
		{
			validateProgressStep(skillSelection);
            foreach (var multisiteSkillForExport in skillSelection.MultisiteSkillsForExport)
            {
                _jobResultFeedback.ReportProgress(_progressStep,
					string.Format(CultureInfo.InvariantCulture, "Starting export of {0}.", multisiteSkillForExport.MultisiteSkill.Name));
                var skillDictionary = _skillDayLoadHelper.LoadSchedulerSkillDays(skillSelection.Period,
                                                                                 new[] { multisiteSkillForExport.MultisiteSkill },
                                                                                 _scenarioProvider.DefaultScenario(multisiteSkillForExport.MultisiteSkill.BusinessUnit));
                var skillStaffPeriodHolder = new SkillStaffPeriodHolder(skillDictionary);

				_jobResultFeedback.Info(string.Format(CultureInfo.InvariantCulture, "Number of subskill mappings: {0}.", multisiteSkillForExport.SubSkillMapping.Count()));
                foreach (var skillExportCombination in multisiteSkillForExport.SubSkillMapping)
                {
					if (!skillExportCombination.TargetSkill.WorkloadCollection.Any())
					{
						_jobResultFeedback.Warning(string.Format(CultureInfo.InvariantCulture,
						                                         "The skill must have at least one workload. (Ignored skill: {0}).",
						                                         skillExportCombination.TargetSkill.Name));
						continue;
					}

                    _jobResultFeedback.ReportProgress(_progressStep,
                        string.Format(CultureInfo.InvariantCulture, "Processing subskill {0}.", skillExportCombination.SourceSkill.Name));
                    ISkillStaffPeriodDictionary skillStaffPeriods;
                    if (skillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(skillExportCombination.SourceSkill, out skillStaffPeriods))
                    {
                        _jobResultFeedback.ReportProgress(0,
							string.Format(CultureInfo.InvariantCulture, "Saving forecast to {0}.", skillExportCombination.TargetSkill.Name));
                        
                        _saveForecastToSkillCommand.Execute(skillSelection.Period, skillExportCombination.TargetSkill, skillStaffPeriods);
                    }
                }
            }
            _jobResultFeedback.ReportProgress(_lastProgressStep, string.Format(CultureInfo.InvariantCulture, "Export of multisite skills succeeded."));
        }

    	private void validateProgressStep(ISkillExportSelection skillSelection)
    	{
    		var numberOfSubSkills = (from m in skillSelection.MultisiteSkillsForExport
    		                         from s in m.SubSkillMapping
    		                         select 1).Sum();

            var steps = numberOfSubSkills + skillSelection.MultisiteSkillsForExport.Count() * 2;
    	    _progressStep = (int) Math.Floor(skillSelection.Incremental*1d/steps);
            _lastProgressStep = skillSelection.Incremental - _progressStep * (steps-1);
    	}
    }

    public interface IMultisiteForecastToSkillCommand
    {
        void Execute(ISkillExportSelection skillSelection);
    }
}