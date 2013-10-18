using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
	public interface IGroupScheduleGroupPageDataProvider : IGroupPageDataProvider{}

	public class GroupScheduleGroupPageDataProvider : IGroupScheduleGroupPageDataProvider
    {
        private readonly ISchedulerStateHolder _stateHolder;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        private IList<IContract> _contractCollection;
        private IList<IContractSchedule> _contractScheduleCollection;
        private IList<IPartTimePercentage> _partTimePercentageCollection;
        private IList<IRuleSetBag> _ruleSetBagCollection;
        private DateOnlyPeriod _selectedPeriod;
        private IList<IGroupPage> _groupPageCollection;
        private IList<ISkill> _skillCollection;
        private IList<IPerson> _personCollection;
		private IList<IPerson> _allPersons;

		public GroupScheduleGroupPageDataProvider(ISchedulerStateHolder stateHolder, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _stateHolder = stateHolder;
            _repositoryFactory = repositoryFactory;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public IEnumerable<IPerson> PersonCollection
        {
            get { 
                if(_personCollection == null)
                {
                    IList<IPerson> validPersons = new List<IPerson>();
                    foreach (var person in _stateHolder.AllPermittedPersons)
                    {
                        if(person.TerminalDate.HasValue)
                        {
                            if(person.TerminalDate.Value >= SelectedPeriod.StartDate)
                                validPersons.Add(person);
                        }
                        else
                        {
                            validPersons.Add(person);
                        }
                    }
                    _personCollection = validPersons;
                }

                return _personCollection;
            }
        }

        public IEnumerable<IContract> ContractCollection
        {
            get
            {
                if (_contractCollection == null)
                {
                    using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                    {
                        using (uow.DisableFilter(QueryFilter.Deleted))
                        {
                            _contractCollection =
                            _repositoryFactory.CreateContractRepository(uow).LoadAll();
                        }
                    }
                }
                return _contractCollection;
            }
        }

        public IEnumerable<IContractSchedule> ContractScheduleCollection
        {
            get
            {
                if (_contractScheduleCollection == null)
                {
                    using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                    {
                        using (uow.DisableFilter(QueryFilter.Deleted))
                        {
                            _contractScheduleCollection =
                            _repositoryFactory.CreateContractScheduleRepository(uow).LoadAll();
                        }
                    }
                }
                return _contractScheduleCollection;
            }
        }

        public IEnumerable<IPartTimePercentage> PartTimePercentageCollection
        {
            get
            {
                if (_partTimePercentageCollection == null)
                {
                    using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                    {
                        using (uow.DisableFilter(QueryFilter.Deleted))
                        {
                            _partTimePercentageCollection =
                            _repositoryFactory.CreatePartTimePercentageRepository(uow).LoadAll();
                        }
                    }
                }
                return _partTimePercentageCollection;
            }
        }

        public IEnumerable<IRuleSetBag> RuleSetBagCollection
        {
            get
            {
                if (_ruleSetBagCollection == null)
                {
                    using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                    {
                        using (uow.DisableFilter(QueryFilter.Deleted))
                        {
                            _ruleSetBagCollection =
                            _repositoryFactory.CreateRuleSetBagRepository(uow).LoadAll();
                        }
                    }
                }
                return _ruleSetBagCollection;
            }
        }

        public IEnumerable<IGroupPage> UserDefinedGroupings
        {
            get
            {
                if (_groupPageCollection == null)
                {
                    using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                    {
                        uow.Reassociate(_stateHolder.Schedules.Keys.Where(x => !(x is IGroupPerson)).ToList());
                        IGroupPageRepository groupPageRepository = _repositoryFactory.CreateGroupPageRepository(uow);
                        _groupPageCollection = new List<IGroupPage>(groupPageRepository.LoadAllGroupPageWhenPersonCollectionReAssociated());
                    }
					RemoveNotLoadedPersonsFromCollection(_groupPageCollection);
                }
                return _groupPageCollection;
            }
        }

		public void RemoveNotLoadedPersonsFromCollection(IEnumerable<IGroupPage> groupPages)
		{
			if(groupPages == null) return;
			var keys = _stateHolder.Schedules.Keys;
			foreach (var groupPage in groupPages)
			{
				foreach (var rootGroupPage in groupPage.RootGroupCollection)
				{
					var toRemove = new List<IPerson>();
					foreach (var person in rootGroupPage.PersonCollection)
					{
						if (!keys.Contains(person))
							toRemove.Add(person);
					}
					foreach (var person in toRemove)
					{
						rootGroupPage.RemovePerson(person);
					}
					foreach (var childPersonGroup in rootGroupPage.ChildGroupCollection)
					{
						removeNotLoadedPersons(childPersonGroup,keys);
					}
				}
			}
		}

		private static void removeNotLoadedPersons(IChildPersonGroup personGroup, ICollection<IPerson> keys)
		{
			var toRemove = new List<IPerson>();
			foreach (var person in personGroup.PersonCollection)
			{
				if (!keys.Contains(person))
					toRemove.Add(person);
			}
			foreach (var person in toRemove)
			{
				personGroup.RemovePerson(person);
			}
			foreach (var childPersonGroup in personGroup.ChildGroupCollection)
			{
				removeNotLoadedPersons(childPersonGroup,keys);
			}	
		}

		
        public IEnumerable<IBusinessUnit> BusinessUnitCollection
        {
            get { yield return ((ITeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit; }
        }

        public DateOnlyPeriod SelectedPeriod
        {
            get { return _selectedPeriod; }
        }

        public IList<ISkill> SkillCollection
        {
            get
            {
                if (_skillCollection == null)
                {
                    using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
                    {
                       _skillCollection = _repositoryFactory.CreateSkillRepository(uow).LoadAll();
                    }
                }
                return _skillCollection;
            }
        }

		public IEnumerable<IPerson> AllLoadedPersons
		{
			get { 
				if(_allPersons == null)
                {
                    IList<IPerson> validPersons = new List<IPerson>();
                    foreach (var person in _stateHolder.SchedulingResultState.PersonsInOrganization)
                    {
                        if(person.TerminalDate.HasValue)
                        {
                            if(person.TerminalDate.Value >= SelectedPeriod.StartDate)
                                validPersons.Add(person);
                        }
                        else
                        {
                            validPersons.Add(person);
                        }
                    }
                    _allPersons = validPersons;
                }

                return _allPersons;
            }
		}

		public void SetSelectedPeriod(DateOnlyPeriod selectedPeriod)
        {
            _selectedPeriod = selectedPeriod;
        }

    }
}