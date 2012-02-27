using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Security
{
	public class ModifyPassword : IModifyPassword
	{
		private readonly ILoadPasswordPolicyService _loadPasswordPolicyService;
		private readonly IUserDetailRepository _userDetailRepository;

		public ModifyPassword(ILoadPasswordPolicyService loadPasswordPolicyService, IUserDetailRepository userDetailRepository)
		{
			_loadPasswordPolicyService = loadPasswordPolicyService;
			_userDetailRepository = userDetailRepository;
		}

		public bool Change(IPerson person, string oldPassword, string newPassword)
		{
			return person.ChangePassword(oldPassword, 
			                             newPassword, 
			                             _loadPasswordPolicyService,
			                             _userDetailRepository.FindByUser(person));
		}
	}
}