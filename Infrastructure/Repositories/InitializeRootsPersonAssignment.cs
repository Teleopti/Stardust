using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Infrastructure.Repositories
{
    public class InitializeRootsPersonAssignment
    {
        private readonly IEnumerable<IPersonAssignment> _personAssignments;

        public InitializeRootsPersonAssignment(IEnumerable<IPersonAssignment> personAssignments)
        {
            _personAssignments = personAssignments;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        public void Initialize()
        {
            //todo: rk - ta bort denna och ladda i de use case där de behövs istället!
            var assWithMainShifts = (from a in _personAssignments
                                     where a.ToMainShift() != null
                                     select a);
            var mainShiftActivities = (from a in assWithMainShifts
                                       from al in a.MainShiftActivityLayers
                                       select al.Payload).Distinct();
            var persShiftActivities = (from a in _personAssignments
                                       from p in a.PersonalShiftCollection
                                       from al in p.LayerCollection
                                       select al.Payload).Distinct();
            var overTimeActivities = (from a in _personAssignments
                                      from o in a.OvertimeShiftCollection
                                      from al in o.LayerCollection
                                      select al.Payload).Distinct();
            var mainShifts = (from s in assWithMainShifts
                              select s.ToMainShift()).Distinct();
            mainShiftActivities.ForEach(a =>
            {
                if (!LazyLoadingManager.IsInitialized(a))
                    LazyLoadingManager.Initialize(a);
            });
            persShiftActivities.ForEach(a =>
            {
                if (!LazyLoadingManager.IsInitialized(a))
                    LazyLoadingManager.Initialize(a);
            });
            overTimeActivities.ForEach(a =>
            {
                if (!LazyLoadingManager.IsInitialized(a))
                    LazyLoadingManager.Initialize(a);
            });


            foreach (var personAss in _personAssignments)
            {
                foreach (var overtime in personAss.OvertimeShiftCollection)
                {
                    foreach (IOvertimeShiftActivityLayer layer in overtime.LayerCollection)
                    {
                        if (!LazyLoadingManager.IsInitialized(layer.DefinitionSet))
                            LazyLoadingManager.Initialize(layer.DefinitionSet);   
                    }
                }
            }

            foreach (IMainShift shift in mainShifts)
            {
                if (!LazyLoadingManager.IsInitialized(shift.ShiftCategory))
                    LazyLoadingManager.Initialize(shift.ShiftCategory);
                if (shift.ShiftCategory != null
                    && shift.ShiftCategory.DayOfWeekJusticeValues != null)
                {
                    foreach (KeyValuePair<DayOfWeek, int> pair in shift.ShiftCategory.DayOfWeekJusticeValues)
                    {
                        if (!LazyLoadingManager.IsInitialized(pair))
                            LazyLoadingManager.Initialize(pair);
                    }
                }
            }
        }
    }
}
