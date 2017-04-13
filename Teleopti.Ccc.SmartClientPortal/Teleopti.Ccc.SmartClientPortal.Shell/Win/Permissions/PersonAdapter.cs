using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Permissions
{
    public class PersonAdapter
    {
        public IPerson Person { get; set; }

        public bool IsDirty { get; set; }

        public bool IsLazyLoaded { get; set; }

        public PersonAdapter(IPerson person, bool isDirty)
        {
            IsLazyLoaded = true;
            Person = person;
            IsDirty = isDirty;
        }
    }
}
