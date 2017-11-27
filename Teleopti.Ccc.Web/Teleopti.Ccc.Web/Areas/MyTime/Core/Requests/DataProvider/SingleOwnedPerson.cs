using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider
{
	public class SingleOwnedPerson : IPersonOwner
	{
		private readonly IPerson _person;

		public SingleOwnedPerson(IPerson person)
		{
			_person = person;
		}

		public IPerson GetPerson(IPersonRepository personRepository) => _person;
	}
}