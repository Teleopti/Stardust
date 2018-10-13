using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{

    public class SmartDayOffBackToLegalStateSolverContainer : ISmartDayOffBackToLegalStateSolverContainer
    {
        private readonly ISmartDayOffBackToLegalStateService _dayOffBackToLegalStateService;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IScheduleMatrixOriginalStateContainer _matrixOriginalStateContainer;
        private readonly ILockableBitArray _bitArray;
        private bool _result;

        public SmartDayOffBackToLegalStateSolverContainer(IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer, ILockableBitArray bitArray, ISmartDayOffBackToLegalStateService dayOffBackToLegalStateService, ISchedulingResultStateHolder schedulingResultStateHolder)
        {
            _dayOffBackToLegalStateService = dayOffBackToLegalStateService;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_matrixOriginalStateContainer = matrixOriginalStateContainer;
            _bitArray = bitArray;
        }

        public bool Result
        {
            get { return _result; }
        }

        public IScheduleMatrixOriginalStateContainer MatrixOriginalStateContainer
        {
            get { return _matrixOriginalStateContainer; }
        }

        public ILockableBitArray BitArray
        {
            get { return _bitArray; }
        }

        public void Execute(IDaysOffPreferences daysOffPreferences)
        {
            _result = _dayOffBackToLegalStateService.Execute(_dayOffBackToLegalStateService.BuildSolverList(_schedulingResultStateHolder,  MatrixOriginalStateContainer.ScheduleMatrix.SchedulePeriod, BitArray, daysOffPreferences, 20), 20);
        }
    }
}