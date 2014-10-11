using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider
{
	public class AgentBadgeProvider: IAgentBadgeProvider
	{
		private readonly IAgentBadgeRepository _repository;
		private readonly IPermissionProvider _permissionProvider;
		private readonly IPersonRepository _personRepository;

		public AgentBadgeProvider(IAgentBadgeRepository repository, IPermissionProvider permissionProvider, IPersonRepository personRepository)
		{
			_repository = repository;
			_permissionProvider = permissionProvider;
			_personRepository = personRepository;
		}

		public IEnumerable<IPerson> GetPermittedAgents(DateOnly date, string functionPath)
		{
			var agents = _repository.GetAgentToplistOfBadges() ?? new IAgentBadgeInfo[] { };
			var personGuidList = agents.Select(item => item.PersonId).ToList();

			var personList = _personRepository.FindPeople(personGuidList);

			return (from t in personList
					where _permissionProvider.HasPersonPermission(functionPath, date, t)
					select t).ToArray();		
		}
	}
}


