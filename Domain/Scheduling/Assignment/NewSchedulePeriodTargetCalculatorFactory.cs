using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class NewSchedulePeriodTargetCalculatorFactory
	{
		private readonly IScheduleMatrixPro _matrix;

		public NewSchedulePeriodTargetCalculatorFactory(IScheduleMatrixPro matrix)
		{
			_matrix = matrix;
		}

		public ISchedulePeriodTargetCalculator CreatePeriodTargetCalculator()
		{
			IVirtualSchedulePeriod schedulePeriod = _matrix.SchedulePeriod;
			if (!schedulePeriod.IsValid)
				return null;

		    var employmentType = schedulePeriod.Contract.EmploymentType;

			if (employmentType == EmploymentType.FixedStaffDayWorkTime)
			{
				// new one
				return new NewDynamicDayOffSchedulePeriodTargetCalculator(_matrix);
			}
			// old one 
			return new NewFixedDayOffSchedulePeriodTargetCalculator(_matrix);
		}
	}
}