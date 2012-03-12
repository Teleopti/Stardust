using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IUserDetailRepository : IRepository<IUserDetail>
    {
        IUserDetail FindByUser(IPerson user);
        IDictionary<IPerson, IUserDetail> FindAllUsers();
        IDictionary<IPerson, IUserDetail> FindByUsers(IEnumerable<IPerson> persons);
    }
}