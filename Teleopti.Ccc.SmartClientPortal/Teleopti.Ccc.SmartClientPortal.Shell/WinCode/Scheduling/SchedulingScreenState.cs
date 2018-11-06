using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling
{
	/*
	 *  DONT MOVE THIS TO DOMAIN
	 */
	public class SchedulingScreenState
	{
		public SchedulingScreenState(ISchedulerStateHolder schedulerStateHolder)
		{
			SchedulerStateHolder = schedulerStateHolder;
		}

		//add more stuff from "domain stateholder" here
		public void Fill(IEnumerable<IScheduleTag> scheduleTags)
		{
			ScheduleTags = scheduleTags;
		}
		
		public ISchedulerStateHolder SchedulerStateHolder { get; }
		public IEnumerable<IScheduleTag> ScheduleTags { get; private set; }
	}
}