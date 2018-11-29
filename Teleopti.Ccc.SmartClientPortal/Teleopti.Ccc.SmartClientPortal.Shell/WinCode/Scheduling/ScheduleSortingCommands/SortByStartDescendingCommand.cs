using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Scheduling.ScheduleSortingCommands
{
    public class SortByStartDescendingCommand : ScheduleSortCommandBase, IScheduleSortCommand
    {
        public SortByStartDescendingCommand(ISchedulerStateHolder schedulerState) : base(schedulerState)
        {
        }

        public void Execute(DateOnly dateToExecuteOn)
        {
            CreateLists(dateToExecuteOn);
            Projections = sort(Projections);
            Absence = sort(Absence);
            MergeLists();
        }

        private static List<Tuple<IVisualLayerCollection, IPerson>> sort(IEnumerable<Tuple<IVisualLayerCollection, IPerson>> projections)
        {
            var sorted = from p in projections
                         orderby p.Item1.Period().Value.StartDateTime descending
                         select p;
            return sorted.ToList();
        }
    }
}