using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.DayOffPlanning
{
    public interface ISmartDayOffBackToLegalStateService
    {
        bool Execute(ISchedulingResultStateHolder schedulingResultStateHolder, IVirtualSchedulePeriod schedulePeriod, ILockableBitArray bitArray, IDaysOffPreferences daysOffPreferences);
    }
}