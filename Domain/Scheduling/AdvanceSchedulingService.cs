using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{

    public interface IAdvanceSchedulingService
    {
        bool Execute(IDictionary<string, IWorkShiftFinderResult> workShiftFinderResultList);
    }
    public class AdvanceSchedulingService : IAdvanceSchedulingService
    {
        public ISchedulingOptions SchedulingOptions { get; set; }
        public ISchedulingResultStateHolder SchedulingResultStateHolder { get; set; }


		//Are you sure we need to inject this
        public AdvanceSchedulingService(ISchedulingOptions schedulingOptions, ISchedulingResultStateHolder schedulingResultStateHolder  )
        {
            SchedulingOptions = schedulingOptions;
            SchedulingResultStateHolder = schedulingResultStateHolder;
        }

        public bool Execute(IDictionary<string, IWorkShiftFinderResult> workShiftFinderResultList)
        {
            bool success = true;
            //Perhaps something like this
			//call class that finds a random team to schedule
			//call class that return the teamblock dates for a given date (problem if team members don't have same days off)
			//call class that returns the aggregated restrictions for the teamblock (is team member personal skills needed for this?)
			//call class that returns the aggregated intraday dist based on teamblock dates
			//call class that returns a filtered list of valid workshifts, this class will probably consists of a lot of subclasses (should we cover for max seats here?)
			//call class that returns the workshift to use based on valid workshifts, the aggregated intraday dist and other things we need
			//call class that schedules given date with given workshift on the complete team
			//call class that schedules the unscheduled days for the teamblock using the same start time from the given shift, this class will handle steady state as well as individual
			//Repeate steps until we have tried all selected

			//To think about
			//Team members can differ from day to day
            return success;
        }
    }
}