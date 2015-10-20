using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakeFixedStaffSchedulingService : IFixedStaffSchedulingService
	{
		public IList<IWorkShiftFinderResult> FinderResults { get; private set; }
		public void ClearFinderResults()
		{
			throw new NotImplementedException();
		}

		public bool DoTheScheduling(IList<IScheduleDay> selectedParts, ISchedulingOptions schedulingOptions, 
			bool breakIfPersonCannotSchedule, ISchedulePartModifyAndRollbackService rollbackService)
		{
			throw new NotImplementedException();
		}

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled
		{
			add { }
			remove { }
		}
	}
}