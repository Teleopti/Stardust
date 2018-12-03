using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
    public class GroupPageOptions : IGroupPageOptions
    {
        public GroupPageOptions(IEnumerable<IPerson> persons)
        {
            Persons = persons;
            SelectedPeriod = DateOnly.Today.ToDateOnlyPeriod();
        }

        public IEnumerable<IPerson> Persons { get; }

        public string CurrentGroupPageName { get; set; }

        public string CurrentGroupPageNameKey { get; set; }

        public DateOnlyPeriod SelectedPeriod { get; set; }
    }
}