using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.GroupPageCreator
{
    public class GroupPageOptions : IGroupPageOptions
    {
        public GroupPageOptions(IEnumerable<IPerson> persons)
        {
            Persons = persons;
            SelectedPeriod = new DateOnlyPeriod(DateOnly.Today,DateOnly.Today);
        }

        public IEnumerable<IPerson> Persons { get; private set; }

        public string CurrentGroupPageName { get; set; }

        public string CurrentGroupPageNameKey { get; set; }

        public DateOnlyPeriod SelectedPeriod { get; set; }
    }
}