using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Principal;
using System.Threading;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
    public class TeleoptiPrincipal : GenericPrincipal, IUnsafePerson
    {
        private IPerson _person;
        private IList<ClaimSet> _claimSets = new List<ClaimSet>();
        private IPrincipalAuthorization _principalAuthorization;
        private IIdentity _identity;

        public TeleoptiPrincipal(IIdentity identity, IPerson person)
            : base(identity, new string[] { })
        {
            _person = person;

            setupPerson();
        }

        private void setupPerson()
        {
            Organisation = new OrganisationMembership();

            if (_person != null)
            {
                createRegional();
                createOrganisationMembership();
            }
        }

        private void createOrganisationMembership()
        {
            Organisation.AddFromPerson(_person);
        }

        private void createRegional()
        {
            Regional = new Regional(_person.PermissionInformation.DefaultTimeZone(),
                                    _person.PermissionInformation.Culture(), _person.PermissionInformation.UICulture());
        }

        public void ChangePrincipal(TeleoptiPrincipal principal)
        {
            _person = principal._person;
            _principalAuthorization = null;
            _claimSets = principal._claimSets;
            _identity = principal.Identity;

            setupPerson();
        }

        public virtual IPerson GetPerson(IPersonRepository personRepository)
        {
            return personRepository.Get(_person.Id.GetValueOrDefault());
        }

        public void AddClaimSet(ClaimSet claimSet)
        {
            _claimSets.Add(claimSet);
        }

        public override IIdentity Identity
        {
            get
            {
                return _identity ?? base.Identity;
            }
        }

        public IEnumerable<ClaimSet> ClaimSets { get { return _claimSets; } }

        public virtual IPrincipalAuthorization PrincipalAuthorization { get { return _principalAuthorization ?? (_principalAuthorization = new PrincipalAuthorization(this)); } }

        public static TeleoptiPrincipal Current
        {
            get { return Thread.CurrentPrincipal as TeleoptiPrincipal; }
        }

        public IRegional Regional { get; private set; }

        public IOrganisationMembership Organisation { get; private set; }

        IPerson IUnsafePerson.Person
        {
            get { return _person; }
        }
    }

    public interface IUnsafePerson
    {
        IPerson Person { get; }
    }
}