using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.WorkflowControl
{
	public class AlreadyAbsentValidator : IAbsenceRequestValidator
	{
		public ISchedulingResultStateHolder SchedulingResultStateHolder { get; set; }
		public IPersonAccountBalanceCalculator PersonAccountBalanceCalculator { get; set; }
		public string InvalidReason { get { return "AlreadyAbsent"; } }
		public IResourceOptimizationHelper ResourceOptimizationHelper { get; set; }
		public string DisplayText { get { return "AlreadyAbsent"; } }
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool Validate(IAbsenceRequest absenceRequest)
		{
			var scheduleDays = SchedulingResultStateHolder.Schedules[absenceRequest.Person]
				.ScheduledDayCollection(
					absenceRequest.Period.ToDateOnlyPeriod(absenceRequest.Person.PermissionInformation.DefaultTimeZone()));
			foreach (var scheduleDay in scheduleDays)
			{
				var projection = scheduleDay.ProjectionService().CreateProjection();
				var absenceLayers = projection.FilterLayers(absenceRequest.Period).FilterLayers<IAbsence>();
				if (absenceLayers.Count()>0)
				{
					return false;
				}
			}

			return true;
		}

		public IAbsenceRequestValidator CreateInstance()
		{
			return new AlreadyAbsentValidator();
		}

		public IBudgetGroupAllowanceSpecification BudgetGroupAllowanceSpecification { get; set; }

		public override bool Equals(object obj)
		{
			var validator = obj as AlreadyAbsentValidator;
			return validator != null;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int result = (SchedulingResultStateHolder != null ? SchedulingResultStateHolder.GetHashCode() : 0);
				result = (result * 397) ^ (PersonAccountBalanceCalculator != null ? PersonAccountBalanceCalculator.GetHashCode() : 0);
				result = (result * 397) ^ (ResourceOptimizationHelper != null ? ResourceOptimizationHelper.GetHashCode() : 0);
				result = (result * 397) ^ (BudgetGroupAllowanceSpecification != null ? BudgetGroupAllowanceSpecification.GetHashCode() : 0);
				result = (result * 397) ^ (GetType().GetHashCode());
				return result;
			}
		}
	}
}