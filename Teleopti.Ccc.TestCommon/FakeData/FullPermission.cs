using System.Collections.Generic;
using System.IdentityModel.Claims;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class FullPermission : IAuthorization, ICurrentAuthorization
	{
		private List<string> _blackList = new List<string>();

		public void AddToBlackList(string functionPath)
		{
			_blackList.Add(functionPath);
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, IPerson person)
		{
			return !_blackList.Contains(functionPath);			
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ITeam team)
		{
			return !_blackList.Contains(functionPath);
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ISite site)
		{
			return !_blackList.Contains(functionPath);
		}

		public bool IsPermitted(string functionPath)
		{
			return !_blackList.Contains(functionPath);
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, IPersonAuthorization authorization)
		{
			return !_blackList.Contains(functionPath);
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ITeamAuthorization authorization)
		{
			return !_blackList.Contains(functionPath);
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, ISiteAuthorization authorization)
		{
			return !_blackList.Contains(functionPath);
		}

		public virtual IEnumerable<DateOnlyPeriod> PermittedPeriods(string functionPath, DateOnlyPeriod period, IPerson person)
		{
			return new[] { period };
		}

		public IEnumerable<IApplicationFunction> GrantedFunctions() {return new List<IApplicationFunction>
		{
			new ApplicationFunction {ForeignId = "0148", ForeignSource = DefinedForeignSourceNames.SourceRaptor}
		}; }

		public bool EvaluateSpecification(ISpecification<IEnumerable<ClaimSet>> specification)
		{
			return true;
		}

		public IAuthorization Current()
		{
			return this;
		}
	}
}