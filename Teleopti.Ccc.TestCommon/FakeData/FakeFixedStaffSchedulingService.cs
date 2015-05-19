using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeFixedStaffSchedulingService : IFixedStaffSchedulingService
	{
		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
		public IList<IWorkShiftFinderResult> FinderResults { get; private set; }
		public void ClearFinderResults()
		{
			throw new NotImplementedException();
		}

		public bool DoTheScheduling(IList<IScheduleDay> selectedParts, ISchedulingOptions schedulingOptions, bool useOccupancyAdjustment,
			bool breakIfPersonCannotSchedule, ISchedulePartModifyAndRollbackService rollbackService)
		{
			throw new NotImplementedException();
		}
	}
}