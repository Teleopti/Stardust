using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleSortingCommands
{
    public class SortByStartAscendingCommand : ScheduleSortCommandBase, IScheduleSortCommand
    {
        public SortByStartAscendingCommand(ISchedulerStateHolder schedulerState):base(schedulerState)
        {}

        public void Execute(DateOnly dateToExecuteOn)
        {
            CreateLists(dateToExecuteOn);
            Projections = sort(Projections);
            Absence = sort(Absence);
            MergeLists();
        }

        private static List<IVisualLayerCollection> sort(IEnumerable<IVisualLayerCollection> projections)
        {
            var sorted = from p in projections
                            orderby p.Period().Value.StartDateTime
                            select p;
            return sorted.ToList();
        }
    }
}