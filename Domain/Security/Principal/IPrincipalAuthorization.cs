﻿using System.Collections.Generic;
using System.IdentityModel.Claims;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
    public interface IPrincipalAuthorization
    {
        bool IsPermitted(string functionPath, DateOnly dateOnly, IPerson person);
        bool IsPermitted(string functionPath, DateOnly dateOnly, ITeam team);
        bool IsPermitted(string functionPath, DateOnly dateOnly, ISite site);
        bool IsPermitted(string functionPath);
		IEnumerable<IApplicationFunction> GrantedFunctions();
		IEnumerable<IApplicationFunction> GrantedFunctionsBySpecification(ISpecification<IApplicationFunction> specification);
        bool EvaluateSpecification(ISpecification<IEnumerable<ClaimSet>> specification);
    	bool IsPermitted(string functionPath, DateOnly dateOnly, IAuthorizeOrganisationDetail authorizeOrganisationDetail);
    	IEnumerable<DateOnlyPeriod> PermittedPeriods(string functionPath, DateOnlyPeriod period, IPerson person);
    }
}