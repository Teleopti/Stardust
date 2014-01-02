using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting
{
	public interface IStatistic
	{
		void Match(IEnumerable<IWorkloadDayBase> workloadDays, IList<IStatisticTask> statisticTasks);

		void Match(IWorkload workload, IEnumerable<IWorkloadDayBase> workloadDays, IList<IStatisticTask> statisticTasks);

		IWorkload CalculateTemplateDays(IList<IWorkloadDayBase> workloadDays);

		IWorkload CalculateCustomTemplateDay(IList<IWorkloadDayBase> workloadDays, int dayIndex);

		IWorkloadDayBase GetTemplateWorkloadDay(IWorkloadDayTemplate workloadDayTemplate, IList<IWorkloadDayBase> workloadDays);

		void Match(IList<ISkillStaffPeriod> targetSkillStaffPeriods, IList<ITemplateTaskPeriod> templateTaskPeriodsWithStatistics, IEnumerable<IActiveAgentCount> activeAgentCountCollection);
	}
}