using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Principal;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class TeleoptiPrincipal : GenericPrincipal, IUnsafePerson, ITeleoptiPrincipal
	{
		private static readonly CurrentTeleoptiPrincipal currentTeleoptiPrincipal = new CurrentTeleoptiPrincipal(new ThreadPrincipalContext());

		public static ITeleoptiPrincipal CurrentPrincipal => currentTeleoptiPrincipal.Current();

		private IClaimsOwner _claimsOwner;
		private IPerson _person;
		private IIdentity _identity;

		public TeleoptiPrincipal(IIdentity identity, IPerson person) : base(identity, new string[] { })
		{
            _person = person;
			_claimsOwner = new ClaimsOwner(person);
            InitializeFromPerson();
        }

		public void ChangePrincipal(TeleoptiPrincipal principal)
		{
			_person = principal._person;
			_claimsOwner = principal._claimsOwner;
			_identity = principal.Identity;

			InitializeFromPerson();
		}

		private void InitializeFromPerson()
		{
			if (_person != null)
			{
				Regional = Principal.Regional.FromPersonWithThreadCultureFallback(_person);
			}
		}

		public override IIdentity Identity => _identity ?? base.Identity;

		public IRegional Regional { get; private set; }
		public IOrganisationMembership Organisation => _claimsOwner.Organisation;

		public IEnumerable<ClaimSet> ClaimSets => _claimsOwner.ClaimSets;

		public void AddClaimSet(ClaimSet claimSet) => _claimsOwner.AddClaimSet(claimSet);

		public virtual IPerson GetPerson(IPersonRepository personRepository)
		{
			return personRepository.Get(_person.Id.GetValueOrDefault());
		}

		IPerson IUnsafePerson.Person => _person;
	}

	public class ClaimsOwner : IClaimsOwner
	{
		private readonly IList<ClaimSet> _claimSets = new List<ClaimSet>();

		public ClaimsOwner(IPerson person)
		{
			Organisation = person == null ? new OrganisationMembership() : OrganisationMembership.FromPerson(person);
		}

		public IOrganisationMembership Organisation { get; }
		public IEnumerable<ClaimSet> ClaimSets => _claimSets;
		public void AddClaimSet(ClaimSet claimSet)
		{
			_claimSets.Add(claimSet);
		}
	}

    public interface IUnsafePerson
    {
        IPerson Person { get; }
    }
}