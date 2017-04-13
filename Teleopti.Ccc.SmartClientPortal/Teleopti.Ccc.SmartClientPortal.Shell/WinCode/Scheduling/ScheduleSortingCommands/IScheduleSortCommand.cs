using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ScheduleSortingCommands
{
    public interface IScheduleSortCommand
    {
        void Execute(DateOnly dateToExecuteOn);
    }
}