using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{

    public interface IAdvanceSchedulingService
    {
        bool Execute(IList<IScheduleMatrixPro> matrixList,
                                     IDictionary<string, IWorkShiftFinderResult> workShiftFinderResultList);
    }
    public class AdvanceSchedulingService : IAdvanceSchedulingService
    {
        public ISchedulingOptions SchedulingOptions { get; set; }

        public AdvanceSchedulingService(ISchedulingOptions schedulingOptions )
        {
            SchedulingOptions = schedulingOptions;
        }

        public bool Execute(IList<IScheduleMatrixPro> matrixList,
            IDictionary<string, IWorkShiftFinderResult> workShiftFinderResultList)
        {
            bool success = true;

            return success;
        }
    }
}