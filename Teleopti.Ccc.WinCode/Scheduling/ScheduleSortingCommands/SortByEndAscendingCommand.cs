using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.ScheduleSortingCommands
{
    public class SortByEndAscendingCommand : ScheduleSortCommandBase, IScheduleSortCommand
    {
        public SortByEndAscendingCommand(ISchedulerStateHolder schedulerState) : base(schedulerState)
        {
        }

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
                         orderby p.Period().Value.EndDateTime
                         select p;
            return sorted.ToList();
        }
    }
}