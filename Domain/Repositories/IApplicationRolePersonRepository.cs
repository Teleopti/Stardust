using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IApplicationRolePersonRepository
    {
        IList<IPersonInRole> GetPersonsInRole(Guid roleId);
        IList<IPersonInRole> GetPersonsNotInRole(Guid roleId, ICollection<Guid> personsIds);
        IList<IRoleLight> Roles();
    }

    public interface IRoleLight
    {
        Guid Id { get; set; }
        string Name { get; set; }
    }

    public interface IPersonInRole : IEquatable<IPersonInRole>
    {
        Guid Id { get; set; }
        string FirstName { get; set; }
        string LastName { get; set; }
        string Team { get; set; }
    }
}