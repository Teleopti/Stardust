using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleSortingCommands
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