using System.Collections.Generic;
using Teleopti.Ccc.Domain.DayOffPlanning;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling
{

    public class SmartDayOffBackToLegalStateSolverContainer : ISmartDayOffBackToLegalStateSolverContainer
    {
        private readonly ISmartDayOffBackToLegalStateService _dayOffBackToLegalStateService;
        private readonly IScheduleMatrixOriginalStateContainer _matrixOriginalStateContainer;
        private readonly ILockableBitArray _bitArray;
        private bool _result;

        public SmartDayOffBackToLegalStateSolverContainer(IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer, ILockableBitArray bitArray, ISmartDayOffBackToLegalStateService dayOffBackToLegalStateService)
        {
            _dayOffBackToLegalStateService = dayOffBackToLegalStateService;
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

        public void Execute()
        {
            _result = _dayOffBackToLegalStateService.Execute(_dayOffBackToLegalStateService.BuildSolverList(BitArray), 20);
        }

        public IList<string> FailedSolverDescriptionKeys
        {
            get { return _dayOffBackToLegalStateService.FailedSolverDescriptionKeys; }
        }
    }
}