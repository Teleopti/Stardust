using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ScheduleSortingCommands
{
    public class NoSortCommand : ScheduleSortCommandBase, IScheduleSortCommand
    {
        public NoSortCommand(ISchedulerStateHolder schedulerState) : base(schedulerState)
        {
        }

        public void Execute(DateOnly dateToExecuteOn)
        {
            //do nothing
        }
    }
}