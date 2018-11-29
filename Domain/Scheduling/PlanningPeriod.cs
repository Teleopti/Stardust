using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Ccc.Domain.Scheduling.Assignment;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class PlanningPeriod : NonversionedAggregateRootWithBusinessUnit
	{
		private DateOnlyPeriod _range;
		private static readonly SchedulePeriodRangeCalculator calculator = new SchedulePeriodRangeCalculator();
		private SchedulePeriodType _periodType;
		private  int _number;
		private PlanningPeriodState _state;
		private readonly ISet<IJobResult> _jobResults;
		private readonly PlanningGroup _planningGroup;

		protected PlanningPeriod()
		{
			_state = PlanningPeriodState.New;
			_jobResults = new HashSet<IJobResult>();
		}

		public PlanningPeriod(IPlanningPeriodSuggestions planningPeriodSuggestions, PlanningGroup planningGroup) : this()
		{
			var suggestedPlanningPeriod = planningPeriodSuggestions.Default();

			_periodType = suggestedPlanningPeriod.PeriodType;
			_number = suggestedPlanningPeriod.Number;
			_range = suggestedPlanningPeriod.Range;
			_planningGroup = planningGroup;
		}

		public PlanningPeriod(DateOnly start, SchedulePeriodType periodType, int number, PlanningGroup planningGroup) : this()
		{
			_range = calculator.PeriodForType(start, new SchedulePeriodForRangeCalculation
			{
				StartDate = start,
				Number = number,
				PeriodType = periodType,
				Culture = CultureInfo.CurrentCulture
			});
			_number = number;
			_periodType = periodType;
			_planningGroup = planningGroup;	
		}

		public virtual DateOnlyPeriod Range => _range;
		public virtual int Number => _number;
		public virtual SchedulePeriodType PeriodType => _periodType;

		public virtual PlanningPeriodState State => _state;

		public virtual PlanningGroup PlanningGroup => _planningGroup;

		public virtual ISet<IJobResult> JobResults => _jobResults;

		public virtual void ChangeRange(SchedulePeriodForRangeCalculation schedulePeriodForRangeCalculation, bool updateTypeAndNumber = false)
		{
			_range = calculator.PeriodForType(schedulePeriodForRangeCalculation.StartDate, schedulePeriodForRangeCalculation);
			if (!updateTypeAndNumber) return;
			_periodType = schedulePeriodForRangeCalculation.PeriodType;
			_number = schedulePeriodForRangeCalculation.Number;
		}

		public virtual void Publish(params IPerson[] people)
		{
			var workflowControlSets = people.Select(p => p.WorkflowControlSet).Where(w => w != null).Distinct();
			foreach (var workflowControlSet in workflowControlSets)
			{
				workflowControlSet.SchedulePublishedToDate = _range.EndDate.Date;
			}
			_state = PlanningPeriodState.Published;
		}

		public virtual PlanningPeriod NextPlanningPeriod(PlanningGroup planningGroup)
		{
			var nextPlanningPeriodStartDate = _range.EndDate.AddDays(1);
			return new PlanningPeriod(nextPlanningPeriodStartDate, _periodType, _number, planningGroup);
		}

		public virtual IJobResult GetLastSchedulingJob()
		{
			return getLastJobResult(JobCategory.WebSchedule);
		}

		public virtual IJobResult GetLastIntradayOptimizationJob()
		{
			return getLastJobResult(JobCategory.WebIntradayOptimization);
		}

		public virtual IJobResult GetLastClearScheduleJob()
		{
			return getLastJobResult(JobCategory.WebClearSchedule);
		}

		public virtual void Reset()
		{
			var clearJobs = _jobResults.Where(x => x.JobCategory == JobCategory.WebClearSchedule).ToArray();
			_jobResults.Clear();
			foreach (var clearJob in clearJobs)
			{
				_jobResults.Add(clearJob);
			}
			_state = PlanningPeriodState.New;
		}

		private IJobResult getLastJobResult(string category)
		{
			return JobResults
				.Where(x => x.JobCategory == category)
				.OrderByDescending(x => x.Timestamp)
				.FirstOrDefault();
		}
	}
}