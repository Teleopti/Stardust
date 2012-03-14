using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IPersonAbsenceAccountRepository : IRepository<IPersonAbsenceAccount>
    {
        IDictionary<IPerson, IPersonAccountCollection> LoadAllAccounts();
        IPersonAccountCollection Find(IPerson person);
    }
}