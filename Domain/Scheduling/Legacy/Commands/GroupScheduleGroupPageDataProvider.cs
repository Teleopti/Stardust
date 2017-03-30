using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Legacy.Commands
{
	public class GroupScheduleGroupPageDataProvider : IGroupScheduleGroupPageDataProvider
    {
        private readonly Func<ISchedulerStateHolder> _stateHolder;
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly ICurrentUnitOfWorkFactory _unitOfWorkFactory;
		private readonly IDisableDeletedFilter _disableDeletedFilter;

		private IList<IContract> _contractCollection;
        private IList<IContractSchedule> _contractScheduleCollection;
        private IList<IPartTimePercentage> _partTimePercentageCollection;
        private IList<IRuleSetBag> _ruleSetBagCollection;
        private IList<IGroupPage> _groupPageCollection;
        private IBusinessUnit _businessUnit;
        private IList<ISkill> _skillCollection;
        private IList<IPerson> _personCollection;
		private IList<IPerson> _allPersons;
		private readonly object _lockObject = new Object();

		public GroupScheduleGroupPageDataProvider(Func<ISchedulerStateHolder> stateHolder, IRepositoryFactory repositoryFactory, ICurrentUnitOfWorkFactory unitOfWorkFactory, IDisableDeletedFilter disableDeletedFilter)
        {
            _stateHolder = stateHolder;
            _repositoryFactory = repositoryFactory;
            _unitOfWorkFactory = unitOfWorkFactory;
			_disableDeletedFilter = disableDeletedFilter;
        }

        public IEnumerable<IPerson> PersonCollection
        {
            get {
	            lock (_lockObject)
	            {
					if (_personCollection == null)
						_personCollection = new List<IPerson>(_stateHolder().AllPermittedPersons);
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
			var keys = schedules.Keys;
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

	    public void SetBusinessUnit_UseFromTestOnly(IBusinessUnit businessUnit)
	    {
		    _businessUnit = businessUnit;
	    }

		public void SetRuleSetBags_UseFromTestOnly(IEnumerable<IRuleSetBag> ruleSetBags)
		{
			_ruleSetBagCollection = ruleSetBags.ToArray();
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
							var businessUnit = repository.Get(ServiceLocatorForEntity.CurrentBusinessUnit.Current().Id.GetValueOrDefault());
							businessUnit = repository.LoadHierarchyInformation(businessUnit);
							_businessUnit = businessUnit;
						}
					}
				}

				return _businessUnit;
	        }
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
						using (var uow = maybeDisposableUnitOfWork.Create(_unitOfWorkFactory))
						{
							_skillCollection = _repositoryFactory.CreateSkillRepository(uow.Uow).LoadAll();
						}
					}
	            }
                
                return _skillCollection;
            }
        }

		public IList<IOptionalColumn> OptionalColumnCollectionAvailableAsGroupPage
		{
			get { return new List<IOptionalColumn>(); }
		}

		public IEnumerable<IPerson> AllLoadedPersons
		{
			get {
				lock (_lockObject)
				{
					if (_allPersons == null)
					{
						_allPersons = new List<IPerson>(_stateHolder().SchedulingResultState.PersonsInOrganization);
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
				try
				{
					return new emptyDisposable(unitOfWorkFactory.Current().CurrentUnitOfWork());
				}
				catch (Exception)
				{
					return new disposableUnitOfWork(unitOfWorkFactory.Current().CreateAndOpenUnitOfWork());
				}
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