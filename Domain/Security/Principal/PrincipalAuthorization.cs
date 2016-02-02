using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class PrincipalAuthorization : IPrincipalAuthorization
    {
		private readonly ICurrentTeleoptiPrincipal _teleoptiPrincipal;

		public PrincipalAuthorization(ICurrentTeleoptiPrincipal teleoptiPrincipal)
        {
            _teleoptiPrincipal = teleoptiPrincipal;
        }

		private static IPrincipalAuthorization _principalAuthorization;

		public static IPrincipalAuthorization Instance()
		{
			return _principalAuthorization ??
			       (_principalAuthorization = new PrincipalAuthorization(new CurrentTeleoptiPrincipal()));
		}

		public static void SetInstance(IPrincipalAuthorization principalAuthorization)
		{
			_principalAuthorization = principalAuthorization;
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, IPerson person)
        {
            return CheckPermitted(functionPath, a => a.Check(_teleoptiPrincipal.Current().Organisation, dateOnly, person));
        }

		private bool CheckPermitted(string functionPath, Func<IAuthorizeAvailableData, bool> availableDataCheck)
		{
			var teleoptiPrincipal = _teleoptiPrincipal.Current();
			if (teleoptiPrincipal == null) return false;
			var claimType = string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace, "/",
				functionPath);
			var dataClaimType = string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace,
				"/AvailableData");
			foreach (var claimSet in teleoptiPrincipal.ClaimSets)
			{
				if (claimSet.FindClaims(claimType, Rights.PossessProperty).Any())
				{
					var availableData =
						claimSet.FindClaims(dataClaimType, Rights.PossessProperty);
					foreach (var claim in availableData)
					{
						var authorizeAvailableData = claim.Resource as IAuthorizeAvailableData;
						if (authorizeAvailableData == null) continue;

						if (availableDataCheck(authorizeAvailableData))
						{
							return true;
						}
					}
				}
			}

			return false;
		}

		public bool IsPermitted(string functionPath, DateOnly dateOnly, IAuthorizeOrganisationDetail authorizeOrganisationDetail)
		{
			var organisation = (IOrganisationMembershipWithId) _teleoptiPrincipal.Current().Organisation;
			return CheckPermitted(functionPath, a => a.Check(organisation, dateOnly, authorizeOrganisationDetail));
		}

        public bool IsPermitted(string functionPath, DateOnly dateOnly, ITeam team)
        {
            return CheckPermitted(functionPath, a => a.Check(_teleoptiPrincipal.Current().Organisation, dateOnly, team));
        }

        public bool IsPermitted(string functionPath, DateOnly dateOnly, ISite site)
        {
            return CheckPermitted(functionPath, a => a.Check(_teleoptiPrincipal.Current().Organisation, dateOnly, site));
        }

        public bool IsPermitted(string functionPath, DateOnly dateOnly, IBusinessUnit businessUnit)
        {
            return CheckPermitted(functionPath, a => a.Check(_teleoptiPrincipal.Current().Organisation, dateOnly, businessUnit));
        }

        public bool IsPermitted(string functionPath)
        {
            return CheckPermitted(functionPath, a => true); //Ignoring available data!
        }
		
		public IEnumerable<DateOnlyPeriod> PermittedPeriods(string functionPath, DateOnlyPeriod period, IPerson person)
        {
            var owningPersonPeriods = _teleoptiPrincipal.Current().Organisation.Periods();
            owningPersonPeriods = owningPersonPeriods.Where(p => p.StartDate <= period.EndDate);

            var checkPersonPeriods = person.PersonPeriods(period);

            var uniqueDates = new HashSet<DateOnly>();
            foreach (var startDate in checkPersonPeriods.Select(p => p.StartDate).Concat(owningPersonPeriods.Select(p => p.StartDate)))
            {
                var minStartDate = startDate;
                if (minStartDate < period.StartDate) minStartDate = period.StartDate;
                uniqueDates.Add(minStartDate);
            }

            uniqueDates.Add(period.StartDate);
            var uniqueDatesArray = uniqueDates.OrderBy(d => d.Date).ToArray();
            var permittedPeriods = new List<DateOnlyPeriod>();

            int i = 0;
            for (; i < uniqueDates.Count-1; i++)
            {
                var currentDate = uniqueDatesArray[i];
                var result = IsPermitted(functionPath, currentDate, person);
                if (result)
                {
                    permittedPeriods.Add(new DateOnlyPeriod(currentDate,uniqueDatesArray[i+1].AddDays(-1)));
                }
            }

            var lastDate = uniqueDatesArray[i];
        	if (IsPermitted(functionPath, lastDate, person))
            {
				var lastDateOfLastPeriod = period.EndDate;
				var lastPersonPeriod = owningPersonPeriods.Where(d => d.StartDate == lastDate);
				if (lastPersonPeriod.Any())
				{
					var endDate = lastPersonPeriod.First().EndDate;
					if (endDate < period.EndDate)
					{
						lastDateOfLastPeriod = endDate;
					}
				}
            	permittedPeriods.Add(new DateOnlyPeriod(lastDate, lastDateOfLastPeriod));
            }

            //Maybe connect adjacent periods?

            return permittedPeriods;
        }

		public IEnumerable<IApplicationFunction> GrantedFunctions()
		{
            var grantedFunctions = new HashSet<IApplicationFunction>();
            foreach (var claimSet in _teleoptiPrincipal.Current().ClaimSets)
            {
                foreach (var claim in claimSet)
                {
					var applicationFunction = claim.Resource as IApplicationFunction;
					if (applicationFunction == null)
						continue;
					grantedFunctions.Add(applicationFunction);
                }
            }
            return grantedFunctions;
        }

        public bool EvaluateSpecification(ISpecification<IEnumerable<ClaimSet>> specification)
        {
			return specification.IsSatisfiedBy(_teleoptiPrincipal.Current().ClaimSets);
        }
		
		public IEnumerable<IApplicationFunction> GrantedFunctionsBySpecification(ISpecification<IApplicationFunction> specification)
        {
			return GrantedFunctions().FilterBySpecification(specification);
        }
    }

    public class ModuleSpecification : Specification<IApplicationFunction>
    {
        public override bool IsSatisfiedBy(IApplicationFunction obj)
        {
            return obj != null && obj.Parent!=null && obj.SortOrder.HasValue && obj.Level == 1;
        }
    }

    public class AllowedToSeeUsersNotInOrganizationSpecification : Specification<IEnumerable<ClaimSet>>
    {
        private readonly string _functionPath;

        public AllowedToSeeUsersNotInOrganizationSpecification(string functionPath)
        {
            _functionPath = functionPath;
        }

	    public override bool IsSatisfiedBy(IEnumerable<ClaimSet> obj)
	    {
		    var principal = TeleoptiPrincipal.CurrentPrincipal;
		    var identity = (ITeleoptiIdentity) principal.Identity;

		    var claimType = string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace, "/",
			    _functionPath);
		    var dataClaimType = string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace,
			    "/AvailableData");

		    foreach (var claimSet in obj)
		    {
			    if (claimSet.FindClaims(claimType, Rights.PossessProperty).Any())
			    {
				    var foundClaims = claimSet.FindClaims(dataClaimType, Rights.PossessProperty);
				    foreach (var foundClaim in foundClaims)
				    {
					    var authorizeMyBusinessUnit = foundClaim.Resource as AuthorizeMyBusinessUnit;
					    var authorizeEveryone = foundClaim.Resource as AuthorizeEveryone;
					    var authorizeExternalAvailableData = foundClaim.Resource as AuthorizeExternalAvailableData;

					    if (authorizeEveryone != null || authorizeMyBusinessUnit != null)
						    return true;

					    if (authorizeExternalAvailableData != null)
					    {
						    if (authorizeExternalAvailableData.Check(principal.Organisation, DateOnly.Today, identity.BusinessUnit))
							    return true;
					    }
				    }
			    }
		    }
		    return false;
	    }
    }
}