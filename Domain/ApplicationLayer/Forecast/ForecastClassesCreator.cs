using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Forecast
{
	public class ForecastClassesCreator : IForecastClassesCreator
	{
		public IWorkloadDayTemplateCalculator CreateWorkloadDayTemplateCalculator(IStatisticHelper statisticsHelper,
																				  IOutlierRepository outlierRepository)
		{
			return new WorkloadDayTemplateCalculator(statisticsHelper, outlierRepository);
		}

		public ITotalVolume CreateTotalVolume()
		{
			return new TotalVolume();
		}

		public ISkillDayCalculator CreateSkillDayCalculator(ISkill skill, IList<ISkillDay> skillDays,
															DateOnlyPeriod visiblePeriod)
		{
			return new SkillDayCalculator(skill, skillDays, visiblePeriod);
		}

		public ISkillDayCalculator CreateSkillDayCalculator(IMultisiteSkill skill, IList<ISkillDay> skillDays, IList<IMultisiteDay> multisiteDays, IDictionary<IChildSkill, ICollection<ISkillDay>> childSkillDays,
															DateOnlyPeriod visiblePeriod)
		{
			var multisiteSkillDayCalculator = new MultisiteSkillDayCalculator(skill, skillDays, multisiteDays, visiblePeriod);
			foreach (var pair in childSkillDays)
			{
				multisiteSkillDayCalculator.SetChildSkillDays(pair.Key, pair.Value.ToList());
			}
			multisiteSkillDayCalculator.InitializeChildSkills();
			return multisiteSkillDayCalculator;
		}

		public ITaskOwnerPeriod GetNewTaskOwnerPeriod(IList<ITaskOwner> taskOwnerDaysWithoutOutliers)
		{
			return new TaskOwnerPeriod(DateOnly.Today, taskOwnerDaysWithoutOutliers, TaskOwnerPeriodType.Other);
		}
	}

	public interface IForecastClassesCreator
	{
		IWorkloadDayTemplateCalculator CreateWorkloadDayTemplateCalculator(IStatisticHelper statisticsHelper,
																		   IOutlierRepository outlierRepository);

		ITotalVolume CreateTotalVolume();

		ISkillDayCalculator CreateSkillDayCalculator(ISkill skill, IList<ISkillDay> skillDays,
													 DateOnlyPeriod visiblePeriod);

		ISkillDayCalculator CreateSkillDayCalculator(IMultisiteSkill skill, IList<ISkillDay> skillDays, IList<IMultisiteDay> multisiteDays, IDictionary<IChildSkill, ICollection<ISkillDay>> childSkillDays,
													 DateOnlyPeriod visiblePeriod);

		ITaskOwnerPeriod GetNewTaskOwnerPeriod(IList<ITaskOwner> taskOwnerDaysWithoutOutliers);
	}
}