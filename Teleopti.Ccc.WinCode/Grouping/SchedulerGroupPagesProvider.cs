using System.Collections.Generic;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.UserTexts;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Grouping
{
	public interface ISchedulerGroupPagesProvider
	{
		IList<IGroupPageLight> GetGroups(bool includeSkills);
	}
	public class SchedulerGroupPagesProvider : ISchedulerGroupPagesProvider
	{
		private readonly IUnitOfWorkFactory _unitOfWorkFactory;
		private IList<IUserDefinedTabLight> _userDefined;
		private IPersonSelectorReadOnlyRepository _personSelectorReadOnlyRepository;

		public SchedulerGroupPagesProvider(IUnitOfWorkFactory unitOfWorkFactory, IPersonSelectorReadOnlyRepository personSelectorReadOnlyRepository)
		{
			_unitOfWorkFactory = unitOfWorkFactory;
			_personSelectorReadOnlyRepository = personSelectorReadOnlyRepository;
		}

		public IList<IGroupPageLight> GetGroups(bool includeSkills)
		{
			var lst = new List<IGroupPageLight>{new GroupPageLight{Key = "Main",Name = Resources.Main},
					new GroupPageLight{Key = "Contracts",Name = Resources.Contract},
					new GroupPageLight{Key = "ContractSchedule",Name = Resources.ContractSchedule},
					new GroupPageLight{Key = "PartTimepercentages",Name = Resources.PartTimePercentage},
					new GroupPageLight{Key = "Note",Name = Resources.Note},
					new GroupPageLight{Key = "RuleSetBag",Name = Resources.RuleSetBag}};
			if (includeSkills)
				lst.Add(new GroupPageLight { Key = "Skill", Name = Resources.Skill });

			if (_userDefined == null)
			{
				using (_unitOfWorkFactory.CreateAndOpenUnitOfWork())
				{
					_userDefined = _personSelectorReadOnlyRepository.GetUserDefinedTabs();
				}
			}
			if(_userDefined != null)
			{
				foreach (var userDefinedTabLight in _userDefined)
				{
					lst.Add(new GroupPageLight { Key = userDefinedTabLight.Id.ToString(), Name = userDefinedTabLight.Name });
				}
			}
			return lst;
		}
	}

	
}