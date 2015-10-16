using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class PrincipalAuthorizationWithFullPermission : IPrincipalAuthorization
	{
		public bool IsPermitted(string functionPath, DateOnly dateOnly, IPerson person)
		{
			return true;
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ITeam team)
		{
			return true;
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ISite site)
		{
			return true;
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, IBusinessUnit businessUnit)
		{
			return true;
		}

		public bool IsPermitted(string functionPath)
		{
			return true;
		}

		public IEnumerable<IApplicationFunction> GrantedFunctions() { throw new NotImplementedException(); }

		public IEnumerable<IApplicationFunction> GrantedFunctionsBySpecification(ISpecification<IApplicationFunction> specification) { throw new NotImplementedException(); }

		public bool EvaluateSpecification(ISpecification<IEnumerable<ClaimSet>> specification)
		{
			return true;
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, IAuthorizeOrganisationDetail authorizeOrganisationDetail)
		{
			return true;
		}

		public IEnumerable<DateOnlyPeriod> PermittedPeriods(string functionPath, DateOnlyPeriod period, IPerson person)
		{
			return new[] { period };
		}
	}
}