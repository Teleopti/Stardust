#region Imports

using Teleopti.Ccc.Domain.AgentInfo;
using System.Collections;
using System.Collections.Generic;

#endregion

namespace Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers
{

    public class PersonRotationEqualityComparer : IEqualityComparer<PersonRotation>
    {
        public bool Equals(PersonRotation x, PersonRotation y)
        {
            return x.Person.Name == y.Person.Name;
        }

        public int GetHashCode(PersonRotation obj)
        {
            return obj.Person.GetHashCode();
        }
    }

 

}
