using System;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using Teleopti.Ccc.Domain.Logon;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;

namespace Teleopti.Ccc.Sdk.WcfHost.Service.LogOn
{
    public class ClaimSetAuthorizationPolicy : IAuthorizationPolicy
    {
        private readonly string _id;
        private readonly ClaimCache _claimCache = new ClaimCache();
        private readonly PersonInRoleCache _personInRoleCache = new PersonInRoleCache();

        public ClaimSetAuthorizationPolicy()
        {
            _id = Guid.NewGuid().ToString();
        }

        public ClaimSet Issuer => ClaimSet.System;

	    public string Id => _id;

	    public bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            object obj;
            if (!evaluationContext.Properties.TryGetValue("Principal", out obj))
            {
                return true;
            }

            var teleoptiPrincipal = obj as TeleoptiPrincipalForLegacy;
            if (string.IsNullOrEmpty(teleoptiPrincipal?.Identity.Name))
            {
                return true;
            }

            lock (teleoptiPrincipal)
            {
                var personId = teleoptiPrincipal.PersonId;
                if (personId == Guid.Empty) return true;

                var unitOfWorkFactory = ((ITeleoptiIdentity) teleoptiPrincipal.Identity).DataSource.Application;
                var personRepository = new PersonRepository(new FromFactory(() => unitOfWorkFactory));

                using (var unitOfWork = unitOfWorkFactory.CreateAndOpenUnitOfWork())
                {
	                var personInRoleDetails = getOrAddPersonInRoleDetails(personId,teleoptiPrincipal,personRepository);
	                if (personInRoleDetails == null) return true;

                    foreach (var applicationRoleId in personInRoleDetails.Roles)
                    {
                        var claimSet = _claimCache.Get(applicationRoleId);
                        if (claimSet == null)
                        {
                            var roleRepository = new ApplicationRoleRepository(unitOfWork);
                            var applicationRole = roleRepository.Get(applicationRoleId);

	                        var licensedFunctionsProvider = new LicensedFunctionsProvider(new DefinedRaptorApplicationFunctionFactory());
							if (!hasValidLicense(licensedFunctionsProvider,unitOfWorkFactory.Name)) return true;

	                        var roleToClaimSetTransformer =
		                        new ClaimSetForApplicationRole(
			                        new ApplicationFunctionsForRole(
				                        licensedFunctionsProvider,
					                        new ApplicationFunctionRepository(new ThisUnitOfWork(unitOfWork))
				                        )
			                        );

                            claimSet = roleToClaimSetTransformer.Transform(applicationRole, unitOfWorkFactory.Name);
                            _claimCache.Add(claimSet, applicationRoleId);
                        }

                        teleoptiPrincipal.AddClaimSet(claimSet);
                        evaluationContext.AddClaimSet(this, claimSet);
                    }
                }
            }

            return true;
        }

		private PersonInRoleCacheItem getOrAddPersonInRoleDetails(Guid personId, ITeleoptiPrincipal teleoptiPrincipal, IPersonRepository personRepository)
		{
			var personInRoleDetails = _personInRoleCache.Get(personId);
			if (personInRoleDetails == null)
			{
				var person = personRepository.Get(teleoptiPrincipal.PersonId);
				if (person == null) return null;

				personInRoleDetails = new PersonInRoleCacheItem(person);
				_personInRoleCache.Add(personInRoleDetails, personId);
			}
			return personInRoleDetails;
		}

	    private static bool hasValidLicense(ILicensedFunctionsProvider licensedFunctionsProvider, string dataSourceName)
	    {
		    try
		    {
			    licensedFunctionsProvider.LicensedFunctions(dataSourceName);
			    return true;
		    }
		    catch (NullReferenceException)
		    {
			    return false;
		    }
	    }
    }
}