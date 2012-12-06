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

        public AdvanceSchedulingService(ISchedulingOptions schedulingOptions, ISchedulingResultStateHolder schedulingResultStateHolder  )
        {
            SchedulingOptions = schedulingOptions;
            SchedulingResultStateHolder = schedulingResultStateHolder;
        }

        public bool Execute(IDictionary<string, IWorkShiftFinderResult> workShiftFinderResultList)
        {
            bool success = true;
            
            return success;
        }
    }
}