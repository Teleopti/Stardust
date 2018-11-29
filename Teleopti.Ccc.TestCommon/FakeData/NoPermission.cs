using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class NoPermission : IAuthorization, ICurrentAuthorization
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

		public bool IsPermitted(string functionPath)
		{
			return false;
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, IPersonAuthorization authorization)
		{
			return false;
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ITeamAuthorization authorization)
		{
			return false;
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ISiteAuthorization authorization)
		{
			return false;
		}

		public IEnumerable<DateOnlyPeriod> PermittedPeriods(string functionPath, DateOnlyPeriod period, IPerson person)
		{
			return new List<DateOnlyPeriod>(0);
		}

		public IEnumerable<IApplicationFunction> GrantedFunctions() { throw new NotImplementedException(); }

		public bool EvaluateSpecification(ISpecification<IEnumerable<ClaimSet>> specification)
		{
			return false;
		}

		public IAuthorization Current()
		{
			return this;
		}
	}

}