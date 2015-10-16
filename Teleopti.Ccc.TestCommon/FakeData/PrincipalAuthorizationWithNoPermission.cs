using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class PrincipalAuthorizationWithNoPermission : IPrincipalAuthorization
	{
		public bool IsPermitted(string functionPath, DateOnly dateOnly, IPerson person)
		{
			return false;
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ITeam team)
		{
			return false;
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ISite site)
		{
			return false;
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, IBusinessUnit businessUnit)
		{
			return false;
		}

		public bool IsPermitted(string functionPath)
		{
			return false;
		}

		public IEnumerable<IApplicationFunction> GrantedFunctions() { throw new NotImplementedException(); }

		public IEnumerable<IApplicationFunction> GrantedFunctionsBySpecification(ISpecification<IApplicationFunction> specification) { throw new NotImplementedException(); }

		public bool EvaluateSpecification(ISpecification<IEnumerable<ClaimSet>> specification)
		{
			return false;
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, IAuthorizeOrganisationDetail authorizeOrganisationDetail)
		{
			return false;
		}

		public IEnumerable<DateOnlyPeriod> PermittedPeriods(string functionPath, DateOnlyPeriod period, IPerson person)
		{
			return new List<DateOnlyPeriod>(0);
		}
	}

}