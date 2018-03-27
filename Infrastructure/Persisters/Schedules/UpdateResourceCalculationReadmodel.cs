using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Infrastructure.Persisters.Schedules
{
	public class UpdateResourceCalculationReadmodel : IUpdateResourceCalculationReadmodel
	{
		private readonly IScheduleDayDifferenceSaver _scheduleDayDifferenceSaver;

		public UpdateResourceCalculationReadmodel(IScheduleDayDifferenceSaver scheduleDayDifferenceSaver)
		{
			_scheduleDayDifferenceSaver = scheduleDayDifferenceSaver;
		}

		public void Execute(IScheduleRange scheduleRange)
		{
			_scheduleDayDifferenceSaver.SaveDifferences(scheduleRange);
		}
	}
}