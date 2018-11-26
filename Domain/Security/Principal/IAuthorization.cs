using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public static class AuthorizationExtensions
	{
		public static bool IsPermitted(this IAuthorization authorization, string functionPath, DateTime date, IPerson person) =>
			authorization.IsPermitted(functionPath, new DateOnly(date), person);
	}
	
    public interface IAuthorization
    {
        bool IsPermitted(string functionPath);
		bool IsPermitted(string functionPath, DateOnly dateOnly, IPerson person);
		bool IsPermitted(string functionPath, DateOnly dateOnly, ITeam team);
		bool IsPermitted(string functionPath, DateOnly dateOnly, ISite site);
		bool IsPermitted(string functionPath, DateOnly dateOnly, IPersonAuthorization authorization);
		bool IsPermitted(string functionPath, DateOnly dateOnly, ITeamAuthorization authorization);
		bool IsPermitted(string functionPath, DateOnly dateOnly, ISiteAuthorization authorization);

		IEnumerable<DateOnlyPeriod> PermittedPeriods(string functionPath, DateOnlyPeriod period, IPerson person);

		IEnumerable<IApplicationFunction> GrantedFunctions();
        bool EvaluateSpecification(ISpecification<IEnumerable<ClaimSet>> specification);
    }

	public interface IPersonAuthorization
	{
		Guid PersonId { get; }
		Guid? TeamId { get; }
		Guid? SiteId { get; }
		Guid BusinessUnitId { get; }
	}

	public class PersonAuthorization : IPersonAuthorization
	{
		public Guid PersonId { get; set; }
		public Guid? TeamId { get; set; }
		public Guid? SiteId { get; set; }
		public Guid BusinessUnitId { get; set; }
	}

	public interface ITeamAuthorization
	{
		Guid TeamId { get; set; }
		Guid SiteId { get; set; }
		Guid BusinessUnitId { get; set; }
	}

	public class TeamAuthorization : ITeamAuthorization
	{
		public Guid TeamId { get; set; }
		public Guid SiteId { get; set; }
		public Guid BusinessUnitId { get; set; }
	}

	public interface ISiteAuthorization
	{
		Guid SiteId { get; set; }
		Guid BusinessUnitId { get; set; }
	}

	public class SiteAuthorization : ISiteAuthorization
	{
		public Guid SiteId { get; set; }
		public Guid BusinessUnitId { get; set; }
	}

}