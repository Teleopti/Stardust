using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
    public interface IPeopleLoader 
    {
        void Initialize(ISchedulerStateHolder schedulerStateHolder);
    }
}