using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.PersonalAccount;

namespace Teleopti.Ccc.Domain.ApplicationLayer
{
	public interface IPersonAbsenceAccountProvider
	{
		IPersonAccountCollection Find(IPerson person);
	}

	public class PersonAbsenceAccountProvider : IPersonAbsenceAccountProvider
	{
		private readonly IPersonAbsenceAccountRepository _personAbsenceAccountRepository;

		public PersonAbsenceAccountProvider(IPersonAbsenceAccountRepository personAbsenceAccountRepository)
		{
			_personAbsenceAccountRepository = personAbsenceAccountRepository;
		}

		public IPersonAccountCollection Find(IPerson person)
		{
			var allAccounts = _personAbsenceAccountRepository.Find(person);

			IPersonAccountCollection result = new PersonAccountCollection(person);
			foreach (var account in allAccounts)
			{
#pragma warning disable 618
				_personAbsenceAccountRepository.UnitOfWork?.Remove(account);
#pragma warning restore 618
				result.Add(account.EntityClone());
			}
			return result;
		}
	}
}