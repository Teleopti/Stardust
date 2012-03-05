using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Repositories
{
    public interface IApplicationRolePersonRepository
    {
        IList<IPersonInRole> GetPersonsInRole(Guid roleId);
        IList<IPersonInRole> GetPersonsNotInRole(Guid roleId, ICollection<Guid> personsIds);
        IList<IRoleLight> Roles();
        IList<IPersonInRole> Persons();
        IList<IRoleLight> RolesOnPerson(Guid selectedPerson);
        IList<IFunctionLight> FunctionsOnPerson(Guid selectedPerson);
        IList<IFunctionLight> Functions();
        IList<IPersonInRole> PersonsWithFunction(Guid selectedFunction);
        IList<IRoleLight> RolesWithFunction(Guid selectedFunction);
        IList<Guid> AvailableData(Guid selectedPerson);
        IList<int> DataRangeOptions(Guid selectedPerson);
        IList<IPersonInRole> PersonsWithRoles(IList<Guid> roles);
        IList<IRoleLight> RolesWithData(Guid id);
    }

    public interface IFunctionLight
    {
        Guid Id { get; set; }
        string Name { get; set; }
        string ResourceName { get; set; }
        string Role { get; set; }
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