using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Foundation;

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
            var assWithShiftCategories = (from a in _personAssignments
                                     where a.ShiftCategory != null
                                     select a);
            var mainShiftActivities = (from a in assWithShiftCategories
                                       from al in a.MainActivities()
                                       select al.Payload).Distinct();
            var persShiftActivities = (from a in _personAssignments
                                       from pl in a.PersonalActivities()
                                       select pl.Payload).Distinct();
            var overTimeActivities = (from a in _personAssignments
                                      from ol in a.OvertimeActivities()
                                      select ol.Payload).Distinct();


            mainShiftActivities.ForEach(LazyLoadingManager.Initialize);
            persShiftActivities.ForEach(LazyLoadingManager.Initialize);
            overTimeActivities.ForEach(LazyLoadingManager.Initialize);


            foreach (var personAss in _personAssignments)
            {
                foreach (var overtime in personAss.OvertimeActivities())
                {
                    LazyLoadingManager.Initialize(overtime.DefinitionSet);   
                }

	            LazyLoadingManager.Initialize(personAss.DayOff());
            }
        }
    }
}
