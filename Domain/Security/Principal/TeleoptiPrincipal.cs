using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Principal;
using System.Threading;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class TeleoptiPrincipal : GenericPrincipal, IUnsafePerson, ITeleoptiPrincipal
	{
        private IPerson _person;
        private IList<ClaimSet> _claimSets = new List<ClaimSet>();
        private IIdentity _identity;

        public TeleoptiPrincipal(IIdentity identity, IPerson person)
            : base(identity, new string[] { })
        {
            _person = person;

            setupPerson();
        }

		private static readonly CurrentTeleoptiPrincipal CurrentTeleoptiPrincipal = new CurrentTeleoptiPrincipal();
		public static ITeleoptiPrincipal Current { get { return CurrentTeleoptiPrincipal.Current(); } }

		public void ChangePrincipal(TeleoptiPrincipal principal)
		{
			_person = principal._person;
			_claimSets = principal._claimSets;
			_identity = principal.Identity;

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