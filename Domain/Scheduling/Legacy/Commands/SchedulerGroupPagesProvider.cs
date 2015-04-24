using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public interface ISchedulerGroupPagesProvider
	{
		IList<GroupPageLight> GetGroups(bool includeSkills);
	}
	public class SchedulerGroupPagesProvider : ISchedulerGroupPagesProvider
	{
		private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private IList<IUserDefinedTabLight> _userDefined;
		private readonly IPersonSelectorReadOnlyRepository _personSelectorReadOnlyRepository;

		public SchedulerGroupPagesProvider(ICurrentUnitOfWorkFactory unitOfWorkFactory, IPersonSelectorReadOnlyRepository personSelectorReadOnlyRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_personSelectorReadOnlyRepository = personSelectorReadOnlyRepository;
		}

		public IList<GroupPageLight> GetGroups(bool includeSkills)
		{
			var lst = new List<GroupPageLight>{new GroupPageLight(Resources.Main,GroupPageType.Hierarchy),
					new GroupPageLight(Resources.Contract, GroupPageType.Contract),
					new GroupPageLight(Resources.ContractSchedule, GroupPageType.ContractSchedule),
					new GroupPageLight(Resources.PartTimePercentage, GroupPageType.PartTimePercentage),
					new GroupPageLight(Resources.Note, GroupPageType.Note),
					new GroupPageLight(Resources.RuleSetBag, GroupPageType.RuleSetBag)};
			if (includeSkills)
				lst.Add(new GroupPageLight(Resources.Skill,GroupPageType.Skill));

			if (_userDefined == null)
			{
				using (_unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork())
				{
					_userDefined = _personSelectorReadOnlyRepository.GetUserDefinedTabs();
				}
			}
			if(_userDefined != null)
			{
				foreach (var userDefinedTabLight in _userDefined)
				{
					lst.Add(new GroupPageLight(userDefinedTabLight.Name, GroupPageType.UserDefined, userDefinedTabLight.Id.ToString()));
				}
			}
			return lst;
		}
	}

	
}