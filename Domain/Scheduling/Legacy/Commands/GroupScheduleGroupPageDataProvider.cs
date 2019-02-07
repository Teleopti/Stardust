using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class GroupScheduleGroupPageDataProvider : IGroupScheduleGroupPageDataProvider
    {
        private readonly Func<ISchedulerStateHolder> _stateHolder;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IDisableDeletedFilter _disableDeletedFilter;
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		private IEnumerable<IContract> _contractCollection;
        private IEnumerable<IContractSchedule> _contractScheduleCollection;
        private IEnumerable<IPartTimePercentage> _partTimePercentageCollection;
        private IEnumerable<IRuleSetBag> _ruleSetBagCollection;
        private IEnumerable<IGroupPage> _groupPageCollection;
        private IBusinessUnit _businessUnit;
        private IList<ISkill> _skillCollection;
		private IEnumerable<IPerson> _allPersons;
		private readonly object _lockObject = new Object();

		public GroupScheduleGroupPageDataProvider(Func<ISchedulerStateHolder> stateHolder, IRepositoryFactory repositoryFactory, ICurrentUnitOfWorkFactory unitOfWorkFactory, IDisableDeletedFilter disableDeletedFilter, ICurrentBusinessUnit currentBusinessUnit)
        {
            _stateHolder = stateHolder;
            _repositoryFactory = repositoryFactory;
            _unitOfWorkFactory = unitOfWorkFactory;
			_disableDeletedFilter = disableDeletedFilter;
			_currentBusinessUnit = currentBusinessUnit;
		}

        public IEnumerable<IContract> ContractCollection
        {
            get
            {
	            lock (_lockObject)
	            {
					if (_contractCollection == null)
					{
						using (var uow = maybeDisposableUnitOfWork.Create(_unitOfWorkFactory))
						{
							using (_disableDeletedFilter.Disable())
							{
								_contractCollection = _repositoryFactory.CreateContractRepository(uow.Uow).LoadAll();
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
						using (var uow = maybeDisposableUnitOfWork.Create(_unitOfWorkFactory))
						{
							using (_disableDeletedFilter.Disable())
							{
								_contractScheduleCollection =
								_repositoryFactory.CreateContractScheduleRepository(uow.Uow).LoadAll();
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
						using (var uow = maybeDisposableUnitOfWork.Create(_unitOfWorkFactory))
						{
							using (_disableDeletedFilter.Disable())
							{
								_partTimePercentageCollection =
								_repositoryFactory.CreatePartTimePercentageRepository(uow.Uow).LoadAll();
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
						using (var uow = maybeDisposableUnitOfWork.Create(_unitOfWorkFactory))
						{
							using (_disableDeletedFilter.Disable())
							{
								_ruleSetBagCollection =
								_repositoryFactory.CreateRuleSetBagRepository(uow.Uow).LoadAll();
							}
						}
					}
	            }
                
                return _ruleSetBagCollection;
            }
        }

	    public IEnumerable<IGroupPage> UserDefinedGroupings(IScheduleDictionary schedules)
	    {
		    lock (_lockObject)
		    {
			    if (_groupPageCollection == null)
			    {
				    using (var uow = maybeDisposableUnitOfWork.Create(_unitOfWorkFactory))
				    {
					    uow.Uow.Reassociate(schedules.Keys);
					    IGroupPageRepository groupPageRepository = _repositoryFactory.CreateGroupPageRepository(uow.Uow);
					    _groupPageCollection = new List<IGroupPage>(groupPageRepository.LoadAllGroupPageWhenPersonCollectionReAssociated());
				    }
				    RemoveNotLoadedPersonsFromCollection(_groupPageCollection, schedules);
			    }
		    }

		    return _groupPageCollection;
	    }

	    public void RemoveNotLoadedPersonsFromCollection(IEnumerable<IGroupPage> groupPages, IScheduleDictionary schedules)
		{
			if(groupPages == null) return;
			var keys = schedules.Keys.ToHashSet();
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

		private static void removeNotLoadedPersons(IChildPersonGroup personGroup, HashSet<IPerson> keys)
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

		public IBusinessUnit BusinessUnit
        {
	        get
			{
				lock (_lockObject)
				{
					if (_businessUnit == null)
					{
						using (var uow = maybeDisposableUnitOfWork.Create(_unitOfWorkFactory))
						{
							var repository = _repositoryFactory.CreateBusinessUnitRepository(uow.Uow);
							var businessUnit = repository.Get(_currentBusinessUnit.CurrentId().Value);
							businessUnit = repository.LoadHierarchyInformation(businessUnit);
							_businessUnit = businessUnit;
						}
					}
				}

				return _businessUnit;
	        }
        }

		public DateOnlyPeriod SelectedPeriod => new DateOnlyPeriod(DateOnly.MaxValue, DateOnly.MinValue);

	    public IList<ISkill> SkillCollection
        {
            get
            {
	            lock (_lockObject)
	            {
					if (_skillCollection == null)
					{
						using (var uow = maybeDisposableUnitOfWork.Create(_unitOfWorkFactory))
						{
							_skillCollection = _repositoryFactory.CreateSkillRepository(uow.Uow).LoadAll().ToList();
						}
					}
	            }
                
                return _skillCollection;
            }
        }

		public IList<IOptionalColumn> OptionalColumnCollectionAvailableAsGroupPage => new List<IOptionalColumn>();

	    public IEnumerable<IPerson> AllLoadedPersons
		{
			get {
				lock (_lockObject)
				{
					if (_allPersons == null)
					{
						_allPersons = new List<IPerson>(_stateHolder().SchedulingResultState.LoadedAgents);
					}
				}
				
                return _allPersons;
            }
		}

		interface IDisposableWithUow : IDisposable
		{
			IUnitOfWork Uow { get; }
		}

		static class maybeDisposableUnitOfWork
		{
			public static IDisposableWithUow Create(ICurrentUnitOfWorkFactory unitOfWorkFactory)
			{
				var currUowFactory = unitOfWorkFactory.Current();
				return currUowFactory.HasCurrentUnitOfWork()
					? (IDisposableWithUow) new emptyDisposable(currUowFactory.CurrentUnitOfWork())
					: new disposableUnitOfWork(currUowFactory.CreateAndOpenUnitOfWork());
			}

			class disposableUnitOfWork : IDisposableWithUow
			{
				public disposableUnitOfWork(IUnitOfWork unitOfWork)
				{
					Uow = unitOfWork;
				}

				public void Dispose()
				{
					Uow.Dispose();
					Uow = null;
				}

				public IUnitOfWork Uow { get; private set; }
			}

			class emptyDisposable : IDisposableWithUow
			{
				public emptyDisposable(IUnitOfWork unitOfWork)
				{
					Uow = unitOfWork;
				}

				public void Dispose()
				{
					Uow = null;
				}

				public IUnitOfWork Uow { get; private set; }
			}
		}
    }
}