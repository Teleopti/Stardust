namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
    public interface ISmartDayOffBackToLegalStateSolverContainer
    {
        bool Result { get; }

        IScheduleMatrixOriginalStateContainer MatrixOriginalStateContainer { get; }

        ILockableBitArray BitArray { get; }

        void Execute(IDaysOffPreferences daysOffPreferences);
    }
}