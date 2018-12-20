using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Logon
{
	public interface IPersonOwner
	{
		IPerson GetPerson(IPersonRepository personRepository);
	}

	public class PrincipalPersonOwner : IPersonOwner
	{
		private readonly ITeleoptiPrincipal _principal;

		public PrincipalPersonOwner(ITeleoptiPrincipal principal)
		{
			_principal = principal;
		}

		public IPerson GetPerson(IPersonRepository personRepository) =>
			personRepository.Get(_principal.PersonId);
	}

	public class SingleOwnedPerson : IPersonOwner
	{
		private readonly IPerson _person;

		public SingleOwnedPerson(IPerson person)
		{
			_person = person;
		}

		public IPerson GetPerson(IPersonRepository personRepository) =>
			_person;
	}
}