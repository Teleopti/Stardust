using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Permissions
{
	public  class PersonToRoleAssociation
	{
		private readonly IPersonInRoleQuerier _personInRoleQuerier;
		private readonly IPersonRepository _personRepository;

		public PersonToRoleAssociation(IPersonInRoleQuerier personInRoleQuerier, IPersonRepository personRepository)
		{
			_personInRoleQuerier = personInRoleQuerier;
			_personRepository = personRepository;
		}

		public void RemoveAssociation(IApplicationRole role)
		{
			var personList =  _personInRoleQuerier.GetPersonInRole(role.Id.Value);
			if (personList.Any())
			{
				foreach (var person in _personRepository.FindPeople(personList))
				{
					person.PermissionInformation.RemoveApplicationRole(role);
				}
				
			}
		}
	}
}