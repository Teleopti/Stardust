using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class PlanningPeriod : NonversionedAggregateRootWithBusinessUnit, IPlanningPeriod
	{
		private DateOnlyPeriod _range;
		private static readonly SchedulePeriodRangeCalculator calculator = new SchedulePeriodRangeCalculator();
		private SchedulePeriodType _periodType;
		private  int _number;
		private PlanningPeriodState _state;
		private readonly ISet<IJobResult> _jobResults;
		private readonly IPlanningGroup _planningGroup;

		protected PlanningPeriod()
		{
			_state = PlanningPeriodState.New;
			_jobResults = new HashSet<IJobResult>();
		}

		protected PlanningPeriod(IPlanningGroup planningGroup):this()
		{
			_planningGroup = planningGroup;
		}

		public PlanningPeriod(IPlanningPeriodSuggestions planningPeriodSuggestions, IPlanningGroup planningGroup) : this()
		{
			var suggestedPlanningPeriod = planningPeriodSuggestions.Default();

			_periodType = suggestedPlanningPeriod.PeriodType;
			_number = suggestedPlanningPeriod.Number;
			_range = suggestedPlanningPeriod.Range;
			_planningGroup = planningGroup;
		}


		public PlanningPeriod(IPlanningPeriodSuggestions planningPeriodSuggestions) : this(planningPeriodSuggestions, null)
		{
		}

		public PlanningPeriod(DateOnlyPeriod range, IPlanningGroup planningGroup=null) : this()
		{
			_range = range;
			updateChangeFromDayType(_range);
			_planningGroup = planningGroup;
		}

		public virtual DateOnlyPeriod Range => _range;

		public virtual PlanningPeriodState State => _state;

		public virtual IPlanningGroup PlanningGroup => _planningGroup;

		public virtual ISet<IJobResult> JobResults => _jobResults;

		public virtual void Scheduled()
		{
			_state = PlanningPeriodState.Scheduled;
		}

		public virtual void ChangeRange(SchedulePeriodForRangeCalculation schedulePeriodForRangeCalculation, bool updateTypeAndNumber = false)
		{
			_range = calculator.PeriodForType(schedulePeriodForRangeCalculation.StartDate, schedulePeriodForRangeCalculation);
			if (!updateTypeAndNumber) return;
			_periodType = schedulePeriodForRangeCalculation.PeriodType;
			_number = schedulePeriodForRangeCalculation.Number;
			if (schedulePeriodForRangeCalculation.PeriodType == SchedulePeriodType.Day)
			{
				updateChangeFromDayType(_range);
			}
		}

		private void updateChangeFromDayType(DateOnlyPeriod period)
		{
			var numberOfDays = period.DayCount();
			if (numberOfDays % 7 == 0)
			{
				_periodType = SchedulePeriodType.Week;
				_number = numberOfDays / 7;
			}
			else if (_range.StartDate.Day == 1 && _range.EndDate.Month != _range.EndDate.AddDays(1).Month)
			{
				_periodType = SchedulePeriodType.Month;
				_number = 12 * (_range.EndDate.Year - _range.StartDate.Year) + (_range.EndDate.Month - _range.StartDate.Month) + 1;
			}
			else
			{
				_periodType = SchedulePeriodType.Day;
				_number = numberOfDays;
			}
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

		public virtual IPlanningPeriod NextPlanningPeriod(IPlanningGroup planningGroup)
		{
			var nextPlanningPeriodStartDate = _range.EndDate.AddDays(1);
			var range = calculator.PeriodForType(nextPlanningPeriodStartDate, new SchedulePeriodForRangeCalculation
			{
				Culture = CultureInfo.CurrentCulture,
				Number = _number,
				PeriodType = _periodType,
				StartDate = nextPlanningPeriodStartDate
			});
			return new PlanningPeriod(planningGroup) { _range = range, _number = _number, _periodType = _periodType };
		}

		public virtual IJobResult GetLastSchedulingJob()
		{
			return getLastJobResult(JobCategory.WebSchedule);
		}

		public virtual IJobResult GetLastIntradayOptimizationJob()
		{
			return getLastJobResult(JobCategory.WebIntradayOptimiztion);
		}

		public virtual void Reset()
		{
			JobResults.Clear();
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