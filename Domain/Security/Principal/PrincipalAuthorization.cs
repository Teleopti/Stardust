﻿using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;
using Teleopti.Ccc.Domain.Specification;

namespace Teleopti.Ccc.Domain.Security.Principal
{
    public class PrincipalAuthorization : IPrincipalAuthorization
    {
		private readonly ITeleoptiPrincipal _teleoptiPrincipal;

		public PrincipalAuthorization(ITeleoptiPrincipal teleoptiPrincipal)
        {
            _teleoptiPrincipal = teleoptiPrincipal;
        }

        public bool IsPermitted(string functionPath, DateOnly dateOnly, IPerson person)
        {
            return CheckPermitted(functionPath, a => a.Check(_teleoptiPrincipal.Organisation, dateOnly, person));
        }

        private bool CheckPermitted(string functionPath, Func<IAuthorizeAvailableData,bool> availableDataCheck)
        {
            foreach (var claimSet in _teleoptiPrincipal.ClaimSets)
            {
                if (claimSet.FindClaims(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace, "/", functionPath), Rights.PossessProperty).Any())
                {
                    var availableData =
                        claimSet.FindClaims(
                            string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace,
                                          "/AvailableData"), Rights.PossessProperty);
                    foreach (var claim in availableData)
                    {
                        IAuthorizeAvailableData authorizeAvailableData = claim.Resource as IAuthorizeAvailableData;
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
			return CheckPermitted(functionPath, a => a.Check((IOrganisationMembershipWithId)_teleoptiPrincipal.Organisation, dateOnly, authorizeOrganisationDetail));
		}

        public bool IsPermitted(string functionPath, DateOnly dateOnly, ITeam team)
        {
            return CheckPermitted(functionPath, a => a.Check(_teleoptiPrincipal.Organisation, dateOnly, team));
        }

        public bool IsPermitted(string functionPath, DateOnly dateOnly, ISite site)
        {
            return CheckPermitted(functionPath, a => a.Check(_teleoptiPrincipal.Organisation, dateOnly, site));
        }

        public bool IsPermitted(string functionPath, DateOnly dateOnly, IBusinessUnit businessUnit)
        {
            return CheckPermitted(functionPath, a => a.Check(_teleoptiPrincipal.Organisation, dateOnly, businessUnit));
        }

        public bool IsPermitted(string functionPath)
        {
            return CheckPermitted(functionPath, a => true); //Ignoring available data!
        }

        public IEnumerable<DateOnlyPeriod> PermittedPeriods(IApplicationFunction applicationFunction, DateOnlyPeriod period, IPerson person)
        {
            var owningPersonPeriods = _teleoptiPrincipal.Organisation.Periods();
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
            var functionPath = applicationFunction.FunctionPath;

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
            HashSet<IApplicationFunction> applicationFunctions = new HashSet<IApplicationFunction>();
            foreach (var claimSet in _teleoptiPrincipal.ClaimSets)
            {
                foreach (var claim in claimSet)
                {
                    IApplicationFunction applicationFunction = claim.Resource as IApplicationFunction;
                    if (applicationFunction==null) continue;

                    applicationFunctions.Add(applicationFunction);
                }
            }
            return applicationFunctions;
        }

        public bool EvaluateSpecification(ISpecification<IEnumerable<ClaimSet>> specification)
        {
            return specification.IsSatisfiedBy(_teleoptiPrincipal.ClaimSets);
        }

        //new ExternalApplicationFunctionSpecification(DefinedForeignSourceNames.SourceMatrix)
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
            var principal = TeleoptiPrincipal.Current;
            var identity = (ITeleoptiIdentity)principal.Identity;

            foreach (var claimSet in obj)
            {
                if (claimSet.FindClaims(string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace, "/", _functionPath), Rights.PossessProperty).Any())
                {
                    var foundClaims = claimSet.FindClaims(
                        string.Concat(TeleoptiAuthenticationHeaderNames.TeleoptiAuthenticationHeaderNamespace,
                                      "/AvailableData"), Rights.PossessProperty);
                    foreach (var foundClaim in foundClaims)
                    {
                        var authorizeMyBusinessUnit = foundClaim.Resource as AuthorizeMyBusinessUnit;
                        var authorizeEveryone = foundClaim.Resource as AuthorizeEveryone;
                        var authorizeExternalAvailableData = foundClaim.Resource as AuthorizeExternalAvailableData;

                        if (authorizeEveryone != null || authorizeMyBusinessUnit != null)
                            return true;

                        if (authorizeExternalAvailableData!=null)
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