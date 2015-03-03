using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Scheduling
{
	public class GroupScheduleGroupPageDataProvider : IGroupScheduleGroupPageDataProvider
    {
        private readonly ISchedulerStateHolder _stateHolder;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        private IList<IContract> _contractCollection;
        private IList<IContractSchedule> _contractScheduleCollection;
        private IList<IPartTimePercentage> _partTimePercentageCollection;
        private IList<IRuleSetBag> _ruleSetBagCollection;
        private IList<IGroupPage> _groupPageCollection;
        private IList<ISkill> _skillCollection;
        private IList<IPerson> _personCollection;
		private IList<IPerson> _allPersons;
		private readonly object _lockObject = new Object();

		public GroupScheduleGroupPageDataProvider(ISchedulerStateHolder stateHolder, IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _stateHolder = stateHolder;
            _repositoryFactory = repositoryFactory;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public IEnumerable<IPerson> PersonCollection
        {
            get {
	            lock (_lockObject)
	            {
					if (_personCollection == null)
						_personCollection = new List<IPerson>(_stateHolder.AllPermittedPersons);
	            }
                
                return _personCollection;
            }
        }

        public IEnumerable<IContract> ContractCollection
        {
            get
            {
	            lock (_lockObject)
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
	            }
               
                return _contractCollection;
            }
        }

        public IEnumerable<IContractSchedule> ContractScheduleCollection
        {
            get
            {
	            lock (_lockObject)
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
	            }
               
                return _contractScheduleCollection;
            }
        }

        public IEnumerable<IPartTimePercentage> PartTimePercentageCollection
        {
            get
            {
	            lock (_lockObject)
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
	            }
                
                return _partTimePercentageCollection;
            }
        }

        public IEnumerable<IRuleSetBag> RuleSetBagCollection
        {
            get
            {
	            lock (_lockObject)
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
	            }
                
                return _ruleSetBagCollection;
            }
        }

        public IEnumerable<IGroupPage> UserDefinedGroupings
        {
            get
            {
	            lock (_lockObject)
	            {
					if (_groupPageCollection == null)
					{
						using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
						{
							uow.Reassociate(_stateHolder.Schedules.Keys);
							IGroupPageRepository groupPageRepository = _repositoryFactory.CreateGroupPageRepository(uow);
							_groupPageCollection = new List<IGroupPage>(groupPageRepository.LoadAllGroupPageWhenPersonCollectionReAssociated());
						}
						RemoveNotLoadedPersonsFromCollection(_groupPageCollection);
					}
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
            get { yield return ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity).BusinessUnit; }
        }

		public DateOnlyPeriod SelectedPeriod
		{
			//never call this when using this instance
			get { return new DateOnlyPeriod(DateOnly.MaxValue, DateOnly.MinValue);}
		}

		public IList<ISkill> SkillCollection
        {
            get
            {
	            lock (_lockObject)
	            {
					if (_skillCollection == null)
					{
						using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
						{
							_skillCollection = _repositoryFactory.CreateSkillRepository(uow).LoadAll();
						}
					}
	            }
                
                return _skillCollection;
            }
        }

		public IEnumerable<IPerson> AllLoadedPersons
		{
			get {
				lock (_lockObject)
				{
					if (_allPersons == null)
					{
						_allPersons = new List<IPerson>(_stateHolder.SchedulingResultState.PersonsInOrganization);
					}
				}
				
                return _allPersons;
            }
		}
    }
}