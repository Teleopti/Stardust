using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using NHibernate.Util;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class ConfigurablePermissions: IAuthorization, ICurrentAuthorization
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

		public bool IsPermitted(string functionPath)
		{
			return _permittedFunctionPaths.Contains(functionPath);
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, IPersonAuthorization authorization)
		{
			return _permittedFunctionPaths.Contains(functionPath);
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ITeamAuthorization authorization)
		{
			return _permittedFunctionPaths.Contains(functionPath);
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ISiteAuthorization authorization)
		{
			return _permittedFunctionPaths.Contains(functionPath);
		}

		public IEnumerable<DateOnlyPeriod> PermittedPeriods(string functionPath, DateOnlyPeriod period, IPerson person)
		{
			if (_permittedFunctionPaths.Contains(functionPath))
				return new List<DateOnlyPeriod> { period };
			return new List<DateOnlyPeriod>();
		}

		public IEnumerable<IApplicationFunction> GrantedFunctions()
		{
			var grantedFunctions = new List<IApplicationFunction>();
			_permittedFunctionPaths.ForEach(p => grantedFunctions.Add(new ApplicationFunction(p)));
			return grantedFunctions;
		}

		public bool EvaluateSpecification(ISpecification<IEnumerable<ClaimSet>> specification)
		{
			throw new NotImplementedException();
		}

		public void HasPermission(string functionPath)
		{
			_permittedFunctionPaths.Add(functionPath);
		}

		public IAuthorization Current()
		{
			return this;
		}
	}

}