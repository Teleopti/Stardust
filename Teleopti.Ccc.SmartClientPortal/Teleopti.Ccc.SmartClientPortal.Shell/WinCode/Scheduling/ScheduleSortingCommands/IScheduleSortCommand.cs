

using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleSortingCommands
{
    public interface IScheduleSortCommand
    {
        void Execute(DateOnly dateToExecuteOn);
    }
}