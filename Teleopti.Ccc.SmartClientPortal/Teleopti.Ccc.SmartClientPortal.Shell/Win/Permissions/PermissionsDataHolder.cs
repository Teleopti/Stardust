using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Permissions
{
    public class PermissionsDataHolder
    {
        private readonly ICollection<IPersonInRole> _personCollection = new List<IPersonInRole>();


        public ICollection<IPersonInRole> PersonCollection
        {
            get
            {
                return _personCollection;
            }
        }

        public IAvailableData AvailableData { get; set; }

        public bool IsDirtyRole { get; set; }


        public PermissionsDataHolder(bool isDirtyRole)
        {
            IsDirtyRole = isDirtyRole;
        }

        public PermissionsDataHolder(ICollection<IPersonInRole> personCollection, IAvailableData availableData, bool isDirtyRole)
        {
            _personCollection = personCollection;
            AvailableData = availableData;
            IsDirtyRole = isDirtyRole;
        }

        public void AddPersonToCollection(IPersonInRole person)
        {
            if (!_personCollection.Contains(person))
            {
                _personCollection.Add(person);
            }
        }

        public void RemovePersonFromCollection(IPersonInRole person)
        {
            if (_personCollection.Contains(person))
            {
                _personCollection.Remove(person);
            }
        }
    }
}
