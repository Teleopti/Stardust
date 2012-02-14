using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class ScheduleMatrixBackToLegalStateBrutalForceServiceContainer : IScheduleMatrixBackToLegalStateBrutalForceServiceContainer
    {
        private readonly ISchedulePeriodDayOffBackToLegalStateByBrutalForceService _service;
        private readonly IScheduleMatrixPro _scheduleMatrix;

        public ScheduleMatrixBackToLegalStateBrutalForceServiceContainer(
            ISchedulePeriodDayOffBackToLegalStateByBrutalForceService service,
            IScheduleMatrixPro scheduleMatrix)
        {
            _service = service;
            _scheduleMatrix = scheduleMatrix;
        }

        public ISchedulePeriodDayOffBackToLegalStateByBrutalForceService Service
        {
            get { return _service; }
        }

        public IScheduleMatrixPro ScheduleMatrix
        {
            get { return _scheduleMatrix; }
        }
    }
}
