using System;
using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.IdentityModel.Policy;
using System.Linq;
using System.Web;
using System.Web.Caching;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.WcfService.LogOn
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

        public ClaimSet Issuer
        {
            get { return ClaimSet.System; }
        }

        public string Id
        {
            get { return _id; }
        }

        public bool Evaluate(EvaluationContext evaluationContext, ref object state)
        {
            object obj;
            if (!evaluationContext.Properties.TryGetValue("Principal", out obj))
            {
                return true;
            }

            var teleoptiPrincipal = obj as TeleoptiPrincipal;
            if (teleoptiPrincipal == null ||
                string.IsNullOrEmpty(teleoptiPrincipal.Identity.Name))
            {
                return true;
            }

            lock (teleoptiPrincipal)
            {
                var person = ((IUnsafePerson) teleoptiPrincipal).Person;
                if (person == null)
                {
                    return true;
                }

                var unitOfWorkFactory = ((ITeleoptiIdentity) teleoptiPrincipal.Identity).DataSource.Application;
                var personRepository = new PersonRepository(unitOfWorkFactory);
                var personId = person.Id.GetValueOrDefault();

                using (var unitOfWork = unitOfWorkFactory.CreateAndOpenUnitOfWork())
                {
                    var personInRoleDetails = _personInRoleCache.Get(personId);
                    if (personInRoleDetails == null)
                    {
                        person = teleoptiPrincipal.GetPerson(personRepository);
                        if (person == null) return true;

                        personInRoleDetails = new PersonInRoleCacheItem(person);
                        _personInRoleCache.Add(personInRoleDetails, personId);
                    }
                    foreach (var applicationRoleId in personInRoleDetails.Roles)
                    {
                        var claimSet = _claimCache.Get(applicationRoleId);
                        if (claimSet == null)
                        {
                            var roleRepository = new ApplicationRoleRepository(unitOfWork);
                            var applicationRole = roleRepository.Get(applicationRoleId);

                        	var roleToClaimSetTransformer =
                        		new RoleToClaimSetTransformer(
                        			new FunctionsForRoleProvider(
                        				new LicensedFunctionsProvider(new DefinedRaptorApplicationFunctionFactory()),
                        				new ExternalFunctionsProvider(new RepositoryFactory())
                        				),
                        			new ClaimWithEntity()
                        			);

                            claimSet = roleToClaimSetTransformer.Transform(applicationRole, unitOfWork);
                            _claimCache.Add(claimSet, applicationRoleId);
                        }

                        teleoptiPrincipal.AddClaimSet(claimSet);
                        evaluationContext.AddClaimSet(this, claimSet);
                    }
                }
            }

            return true;
        }
    }

    public class PersonInRoleCacheItem
    {
        public PersonInRoleCacheItem(IPerson person)
        {
            Roles = person.PermissionInformation.ApplicationRoleCollection.Select(r => r.Id.GetValueOrDefault()).ToList();
        }

        public IEnumerable<Guid> Roles { get; private set; }
    }

    public class PersonInRoleCache
    {
        private readonly Cache _cache = HttpRuntime.Cache;

        public void Add(PersonInRoleCacheItem cacheItem, Guid personId)
        {
            string key = personId.ToString();
            _cache.Add(key, cacheItem, null, DateTime.Now.AddMinutes(30), Cache.NoSlidingExpiration, CacheItemPriority.Normal, null);
        }

        public PersonInRoleCacheItem Get(Guid personId)
        {
            string key = personId.ToString();
            return _cache[key] as PersonInRoleCacheItem;
        }
    }
}