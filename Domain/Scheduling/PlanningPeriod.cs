using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class PlanningPeriod : NonversionedAggregateRootWithBusinessUnit, IPlanningPeriod
	{
		private DateOnlyPeriod _range;
		private static readonly SchedulePeriodRangeCalculator _calculator = new SchedulePeriodRangeCalculator();
		private SchedulePeriodType _periodType;
		private  int _number;
		private PlanningPeriodState _state;

		protected PlanningPeriod()
		{
			_state = PlanningPeriodState.New;
		}
		
		public PlanningPeriod(IPlanningPeriodSuggestions planningPeriodSuggestions) : this()
		{
			var suggestedPlanningPeriod = planningPeriodSuggestions.Default();

			_periodType = suggestedPlanningPeriod.PeriodType;
			_number = suggestedPlanningPeriod.Number;
			_range = suggestedPlanningPeriod.Range;
		}

		public virtual DateOnlyPeriod Range
		{
			get { return _range;  }
		}

		public virtual PlanningPeriodState State
		{
			get { return _state; }
		}

		public virtual void Scheduled()
		{
			_state = PlanningPeriodState.Scheduled;
		}

		public virtual void ChangeRange(SchedulePeriodForRangeCalculation schedulePeriodForRangeCalculation)
		{
			_range = _calculator.PeriodForType(schedulePeriodForRangeCalculation.StartDate, schedulePeriodForRangeCalculation);
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

		public virtual IPlanningPeriod NextPlanningPeriod()
		{
			var nextPlanningPeriodStartDate = _range.EndDate.AddDays(1);
			var range = _calculator.PeriodForType(nextPlanningPeriodStartDate, new SchedulePeriodForRangeCalculation
			{
				Culture = CultureInfo.CurrentCulture,
				Number = _number,
				PeriodType = _periodType,
				StartDate =  nextPlanningPeriodStartDate
			});
			return new PlanningPeriod {_range = range, _number = _number, _periodType = _periodType };
		}
	}
}