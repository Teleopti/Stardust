using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using NHibernate.Util;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FakePermissions: IAuthorization, ICurrentAuthorization
	{
		private readonly IList<string> _permittedAnyData = new List<string>(); 
		private readonly IList<Tuple<string, ISiteAuthorization>> _permittedSites = new List<Tuple<string, ISiteAuthorization>>(); 

		public bool IsPermitted(string functionPath, DateOnly dateOnly, IPerson person)
		{
			return _permittedAnyData.Contains(functionPath);
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ITeam team)
		{
			return _permittedAnyData.Contains(functionPath);
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ISite site)
		{
			return _permittedAnyData.Contains(functionPath);
		}

		public bool IsPermitted(string functionPath)
		{
			return _permittedAnyData.Contains(functionPath);
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, IPersonAuthorization authorization)
		{
			return _permittedAnyData.Contains(functionPath);
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ITeamAuthorization authorization)
		{
			return _permittedAnyData.Contains(functionPath);
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ISiteAuthorization authorization)
		{
			if (_permittedAnyData.Contains(functionPath))
				return true;
			if (_permittedSites.Any(x => x.Item1 == functionPath && x.Item2.BusinessUnitId == authorization.BusinessUnitId && x.Item2.SiteId == authorization.SiteId))
				return true;
			return false;
		}

		public IEnumerable<DateOnlyPeriod> PermittedPeriods(string functionPath, DateOnlyPeriod period, IPerson person)
		{
			if (_permittedAnyData.Contains(functionPath))
				return new List<DateOnlyPeriod> { period };
			return new List<DateOnlyPeriod>();
		}

		public IEnumerable<IApplicationFunction> GrantedFunctions()
		{
			var grantedFunctions = new List<IApplicationFunction>();
			_permittedAnyData.ForEach(p => grantedFunctions.Add(new ApplicationFunction(p)));
			return grantedFunctions;
		}

		public bool EvaluateSpecification(ISpecification<IEnumerable<ClaimSet>> specification)
		{
			throw new NotImplementedException();
		}

		public void HasPermission(string functionPath)
		{
			_permittedAnyData.Add(functionPath);
		}

		public void HasPermission(string functionPath, ISiteAuthorization authorization)
		{
			_permittedSites.Add(new Tuple<string, ISiteAuthorization>(functionPath, authorization));
		}

		public IAuthorization Current()
		{
			return this;
		}
	}

}