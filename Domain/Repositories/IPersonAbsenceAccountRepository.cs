using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IPersonAbsenceAccountRepository : IRepository<IPersonAbsenceAccount>
    {
        IDictionary<IPerson, IPersonAccountCollection> LoadAllAccounts();
        IPersonAccountCollection Find(IPerson person);
        IDictionary<IPerson, IPersonAccountCollection> FindByUsers(IEnumerable<IPerson> persons);
		[Obsolete("Don't use! Shouldn't be here - use ICurrentUnitOfWork instead (or get the unitofwork in some other way).")]
		IUnitOfWork UnitOfWork { get; }
	}
}