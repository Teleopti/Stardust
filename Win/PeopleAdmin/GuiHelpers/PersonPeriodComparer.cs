using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.PeopleAdmin.GuiHelpers
{
    public class PersonPeriodComparer:IEqualityComparer<IPersonPeriod>
    {
        public bool Equals(IPersonPeriod x, IPersonPeriod y)
        {
            return x.StartDate.Date == y.StartDate.Date;
        }

        public int GetHashCode(IPersonPeriod obj)
        {
            return obj.StartDate.Date.GetHashCode();
        }
    }
}
