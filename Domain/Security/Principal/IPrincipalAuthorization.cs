using System.Collections.Generic;
using System.IdentityModel.Claims;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
    public interface IPrincipalAuthorization
    {
        bool IsPermitted(string functionPath, DateOnly dateOnly, IPerson person);
        bool IsPermitted(string functionPath, DateOnly dateOnly, ITeam team);
        bool IsPermitted(string functionPath, DateOnly dateOnly, ISite site);
        bool IsPermitted(string functionPath, DateOnly dateOnly, IBusinessUnit businessUnit);
        bool IsPermitted(string functionPath);
        IEnumerable<DateOnlyPeriod> PermittedPeriods(IApplicationFunction applicationFunction, DateOnlyPeriod period, IPerson person);
        IEnumerable<IApplicationFunction> GrantedFunctions(IApplicationFunctionRepository repository);
        IEnumerable<IApplicationFunction> GrantedFunctionsBySpecification(IApplicationFunctionRepository repository, ISpecification<IApplicationFunction> specification);
        bool EvaluateSpecification(ISpecification<IEnumerable<ClaimSet>> specification);
    	bool IsPermitted(string functionPath, DateOnly dateOnly, IAuthorizeOrganisationDetail authorizeOrganisationDetail);
    }
}