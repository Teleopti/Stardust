using System;
using System.Linq;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCode.Common
{
    /// <summary>
    /// Represents the holder for GroupPage data loadings.
    /// </summary>
    public class GroupPageHelper : IGroupPageHelper
    {
        private readonly IRepositoryFactory _repositoryFactory;
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;
        private IList<IPerson> _personCollection;
        private IList<IGroupPage> _groupPageCollection;
        private IList<IContract> _contractCollection;
        private IList<IPartTimePercentage> _partTimePercentageCollection;
        private IList<IContractSchedule> _contractScheduleCollection;
        private IList<IRuleSetBag> _ruleSetBagCollection;
        private IList<ISkill> _skillCollection;

        public GroupPageHelper(IRepositoryFactory repositoryFactory, IUnitOfWorkFactory unitOfWorkFactory)
        {
            _repositoryFactory = repositoryFactory;
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public IEnumerable<IPartTimePercentage> PartTimePercentageCollection
        {
            get
            {
                CheckLoaded();
                return _partTimePercentageCollection;
            }
        }

        /// <summary>
        /// Gets the person collection.
        /// </summary>
        /// <value>The person collection.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-06-26
        /// </remarks>
        public IEnumerable<IPerson> PersonCollection
        {
            get
            {
                CheckLoaded();
                return _personCollection;
            }
        }

        public IEnumerable<IContract> ContractCollection
        {
            get
            {
                CheckLoaded();
                return _contractCollection;
            }
        }

        public IEnumerable<IGroupPage> UserDefinedGroupings
        {
            get
            {
                CheckLoaded();
                return new List<IGroupPage>(_groupPageCollection);
            }
        }

        private void CheckLoaded()
        {
            if (_personCollection == null)
            {
                throw new InvalidOperationException("The data in the provider must be loaded before the first use.");
            }
        }

        public IEnumerable<IBusinessUnit> BusinessUnitCollection
        {
            get { yield return ((ITeleoptiIdentity)TeleoptiPrincipal.CurrentPrincipal.Identity).BusinessUnit; }
        }

        public DateOnlyPeriod SelectedPeriod { get; private set; }

        public IList<ISkill> SkillCollection
        {
            get
            {
                CheckLoaded();
                return _skillCollection;
            }
        }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public IEnumerable<IPerson> AllLoadedPersons
    	{
    		get { throw new NotImplementedException(); }
    	}

    	public void SetSelectedPeriod(DateOnlyPeriod dateOnlyPeriod)
        {
            SelectedPeriod = dateOnlyPeriod;
        }

        /// <summary>
        /// Gets or sets the current group page.
        /// </summary>
        /// <value>The current group page.</value>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-07-04
        /// </remarks>
        public IGroupPage CurrentGroupPage { get; set; }

        /// <summary>
        /// Loads all persons.
        /// </summary>
        /// <remarks>
        /// Created by: Madhuranga Pinnagoda
        /// Created date: 2008-06-26
        /// </remarks>
        public void LoadAll()
        {
            using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {

                using (uow.DisableFilter(QueryFilter.Deleted))
                {
                    IContractRepository repository = _repositoryFactory.CreateContractRepository(uow);
                    _contractCollection = new List<IContract>(repository.FindAllContractByDescription());

                    IPartTimePercentageRepository partTimePercentageRepository =
                        _repositoryFactory.CreatePartTimePercentageRepository(uow);
                    _partTimePercentageCollection =
                        new List<IPartTimePercentage>(
                            partTimePercentageRepository.FindAllPartTimePercentageByDescription());

                    IContractScheduleRepository contractScheduleRepository =
                        _repositoryFactory.CreateContractScheduleRepository(uow);
                    _contractScheduleCollection =
                        new List<IContractSchedule>(contractScheduleRepository.FindAllContractScheduleByDescription());

                    IRuleSetBagRepository ruleSetBagRepository = _repositoryFactory.CreateRuleSetBagRepository(uow);
                    _ruleSetBagCollection = new List<IRuleSetBag>(ruleSetBagRepository.LoadAll());

                    ISkillRepository skillRep = _repositoryFactory.CreateSkillRepository(uow);
                    _skillCollection = skillRep.LoadAll();
                }
                IPersonRepository rep = _repositoryFactory.CreatePersonRepository(uow);
                _personCollection = rep.LoadAllPeopleWithHierarchyDataSortByName(SelectedPeriod.StartDate).Where(p => !p.BuiltIn).ToList();

                //Clear Collections.
                if (_groupPageCollection != null) _groupPageCollection.Clear();

                IGroupPageRepository groupPageRepository = _repositoryFactory.CreateGroupPageRepository(uow);

                //Copy Group page list.
                _groupPageCollection = new List<IGroupPage>(groupPageRepository.LoadAllGroupPageWhenPersonCollectionReAssociated());
            }
        }

        public void ReloadCurrentGroupPageFromDatabase(IUnitOfWork uow)
        {
            if (CurrentGroupPage.Id.HasValue)
            {
                var groupPageRepository = _repositoryFactory.CreateGroupPageRepository(uow);
                CurrentGroupPage = groupPageRepository.Get(CurrentGroupPage.Id.Value);
            }
        }

        public IEnumerable<IContractSchedule> ContractScheduleCollection
        {
            get { return _contractScheduleCollection; }
        }

        public IEnumerable<IRuleSetBag> RuleSetBagCollection
        {
            get { return _ruleSetBagCollection; }
        }

        /// <summary>
        /// Removes the group page.
        /// </summary>
        /// <param name="groupPage">The group page.</param>
        /// <remarks>
        /// Created by: Muhamad Risath
        /// Created date: 2008-07-10
        /// </remarks>
        public IEnumerable<IRootChangeInfo> RemoveGroupPage(IGroupPage groupPage)
        {
            using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var groupPageRepository = _repositoryFactory.CreateGroupPageRepository(uow);
                groupPageRepository.Remove(groupPage);
                return uow.PersistAll();
            }
        }

        public void RemoveGroupPageById(Guid id)
        {
            using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {

                var groupPageRepository = _repositoryFactory.CreateGroupPageRepository(uow);
                var groupPage = groupPageRepository.Load(id);
                groupPageRepository.Remove(groupPage);
                uow.PersistAll();
            }
        }

        public void SetCurrentGroupPageById(Guid groupPageId)
        {
            using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var groupPageRepository = _repositoryFactory.CreateGroupPageRepository(uow);
                CurrentGroupPage = groupPageRepository.Load(groupPageId);
            }
        }

        public void RenameGroupPage(Guid groupPageId, string newName)
        {
            using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var groupPageRepository = _repositoryFactory.CreateGroupPageRepository(uow);
                var gPage =  groupPageRepository.Load(groupPageId);
                gPage.Description = new Description(newName);
                uow.PersistAll();
            }
        }
        public void UpdateCurrent()
        {
            using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                uow.Reassociate(_personCollection);
                CurrentGroupPage = uow.Merge(CurrentGroupPage);
                uow.PersistAll();
            }
        }

        /// <summary>
        /// Adds the or update group page.
        /// </summary>
        /// <param name="groupPage">The group page.</param>
        public IEnumerable<IRootChangeInfo> AddOrUpdateGroupPage(IGroupPage groupPage)
        {
            if (groupPage == null) return null;

            using (IUnitOfWork uow = _unitOfWorkFactory.CreateAndOpenUnitOfWork())
            {
                var groupPageRepository = _repositoryFactory.CreateGroupPageRepository(uow);
                groupPageRepository.Add(groupPage);
                return uow.PersistAll();
            }
        }

        public IGroupingsCreator CreateGroupingsCreator()
        {
            return new GroupingsCreator(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_groupPageCollection != null)
                {
                    _groupPageCollection.Clear();
                    _groupPageCollection = null;
                }
                if (_personCollection != null)
                {
                    _personCollection.Clear();
                    _personCollection = null;
                }
            }
        }
    }
}