using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.LegacyWrappers;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class IntradayForecaster : IIntradayForecaster
	{
		private const int smoothing = 5;
		private readonly ILoadStatistics _loadStatistics;

		public IntradayForecaster(ILoadStatistics loadStatistics)
		{
			_loadStatistics = loadStatistics;
		}

		public void Apply(IWorkload workload, DateOnlyPeriod templatePeriod, IEnumerable<ITaskOwner> futureWorkloadDays)
		{
			var workloadDays = _loadStatistics.LoadWorkloadDay(workload, templatePeriod).ToArray();
			var statistic = new Statistic(workload);
			statistic.CalculateTemplateDays(workloadDays);

			for (var i = 0; i < 7; i++)
			{
				var template = (IWorkloadDayTemplate)workload.GetTemplateAt(TemplateTarget.Workload, i);
				template.SnapshotTemplateTaskPeriodList(TaskPeriodType.Tasks);
				template.DoRunningSmoothing(smoothing, TaskPeriodType.Tasks);
			}

			var helper = new TaskOwnerHelper(futureWorkloadDays);
			helper.BeginUpdate();
			workload.SetDefaultTemplates(futureWorkloadDays);
			helper.EndUpdate();
		}
	}
}