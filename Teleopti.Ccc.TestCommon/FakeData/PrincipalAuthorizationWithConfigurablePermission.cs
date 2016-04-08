using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class PrincipalAuthorizationWithConfigurablePermission: IPrincipalAuthorization
	{
		private readonly IList<string> _permittedFunctionPaths = new List<string>(); 

		public bool IsPermitted(string functionPath, DateOnly dateOnly, IPerson person)
		{
			return _permittedFunctionPaths.Contains(functionPath);
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ITeam team)
		{
			return _permittedFunctionPaths.Contains(functionPath);
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ISite site)
		{
			return _permittedFunctionPaths.Contains(functionPath);
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, IBusinessUnit businessUnit)
		{
			return _permittedFunctionPaths.Contains(functionPath);
		}

		public bool IsPermitted(string functionPath)
		{
			return _permittedFunctionPaths.Contains(functionPath);
		}

		public IEnumerable<IApplicationFunction> GrantedFunctions() { throw new NotImplementedException(); }

		public IEnumerable<IApplicationFunction> GrantedFunctionsBySpecification(ISpecification<IApplicationFunction> specification) { throw new NotImplementedException(); }

		public bool EvaluateSpecification(ISpecification<IEnumerable<ClaimSet>> specification)
		{
			throw new NotImplementedException();
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, IAuthorizeOrganisationDetail authorizeOrganisationDetail)
		{
			return _permittedFunctionPaths.Contains(functionPath);
		}

		public IEnumerable<DateOnlyPeriod> PermittedPeriods(string functionPath, DateOnlyPeriod period, IPerson person)
		{
			throw new NotImplementedException();
		}

		public void HasPermission(string functionPath)
		{
			_permittedFunctionPaths.Add(functionPath);
		}

		public void Reset()
		{
			_permittedFunctionPaths.Clear();
		}
	}

}