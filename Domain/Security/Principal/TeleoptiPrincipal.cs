using System.Collections.Generic;
using System.IdentityModel.Claims;
using System.Security.Principal;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Security.Principal
{
	public class TeleoptiPrincipal : GenericPrincipal, IUnsafePerson, ITeleoptiPrincipal
	{
		private static readonly CurrentTeleoptiPrincipal CurrentTeleoptiPrincipal = new CurrentTeleoptiPrincipal();
		public static ITeleoptiPrincipal CurrentPrincipal { get { return CurrentTeleoptiPrincipal.Current(); } }





		private IPerson _person;
		private IList<ClaimSet> _claimSets = new List<ClaimSet>();
		private IIdentity _identity;

		public TeleoptiPrincipal(IIdentity identity, IPerson person) : base(identity, new string[] { })
        {
            _person = person;
            InitializeFromPerson();
        }

		public void ChangePrincipal(TeleoptiPrincipal principal)
		{
			_person = principal._person;
			_claimSets = principal._claimSets;
			_identity = principal.Identity;

			InitializeFromPerson();
		}

		private void InitializeFromPerson()
		{
			// dont ask my why, just refact without changing behavior...
			if (_person == null)
			{
				Organisation = new OrganisationMembership();
				return;
			}
			Regional = Principal.Regional.FromPersonWithThreadCultureFallback(_person);
			Organisation = OrganisationMembership.FromPerson(_person);
		}

		public override IIdentity Identity { get { return _identity ?? base.Identity; } }

		public IRegional Regional { get; private set; }
		public IOrganisationMembership Organisation { get; private set; }
		public IEnumerable<ClaimSet> ClaimSets { get { return _claimSets; } }

		public void AddClaimSet(ClaimSet claimSet) { _claimSets.Add(claimSet); }

		public virtual IPerson GetPerson(IPersonRepository personRepository)
		{
			return personRepository.Get(_person.Id.GetValueOrDefault());
		}

		IPerson IUnsafePerson.Person { get { return _person; } }

	}

    public interface IUnsafePerson
    {
        IPerson Person { get; }
    }
}