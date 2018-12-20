using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;


namespace Teleopti.Ccc.SmartClientPortal.Shell.WinCode.Common
{
	public class GroupPageHelper : IGroupPageHelper
	{
		private IRepositoryFactory _repositoryFactory;
		private IUnitOfWorkFactory _unitOfWorkFactory;
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

		public IEnumerable<IGroupPage> UserDefinedGroupings(IScheduleDictionary schedules)
		{
			CheckLoaded();
			return new List<IGroupPage>(_groupPageCollection);
		}

		private void CheckLoaded()
		{
			if (_personCollection == null)
			{
				throw new InvalidOperationException("The data in the provider must be loaded before the first use.");
			}
		}

		public IBusinessUnit BusinessUnit
		{
			get { return ((ITeleoptiIdentity)TeleoptiPrincipalForLegacy.CurrentPrincipal.Identity).BusinessUnit; }
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

		public IList<IOptionalColumn> OptionalColumnCollectionAvailableAsGroupPage
		{
			get { return new List<IOptionalColumn>(); }
		}

		public void SetSelectedPeriod(DateOnlyPeriod dateOnlyPeriod)
		{
			SelectedPeriod = dateOnlyPeriod;
		}

		public IGroupPage CurrentGroupPage { get; set; }

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
					_skillCollection = skillRep.LoadAll().ToList();
				}
				IPersonRepository rep = _repositoryFactory.CreatePersonRepository(uow);
				_personCollection = rep.LoadAllPeopleWithHierarchyDataSortByName(SelectedPeriod.StartDate).ToList();

				//Clear Collections.
				_groupPageCollection = new List<IGroupPage>();
				//Copy Group page list.
				// bug 34650 Seems like we don't need this
				//IGroupPageRepository groupPageRepository = _repositoryFactory.CreateGroupPageRepository(uow);
				//_groupPageCollection = new List<IGroupPage>(groupPageRepository.LoadAllGroupPageWhenPersonCollectionReAssociated());
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
				var gPage = groupPageRepository.Load(groupPageId);
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
				if (_partTimePercentageCollection != null)
				{
					_partTimePercentageCollection.Clear();
					_partTimePercentageCollection = null;
				}
				if (_contractCollection != null)
				{
					_contractCollection.Clear();
					_contractCollection = null;
				}
				if (_contractScheduleCollection != null)
				{
					_contractScheduleCollection.Clear();
					_contractScheduleCollection = null;
				}
				if (_ruleSetBagCollection != null)
				{
					_ruleSetBagCollection.Clear();
					_ruleSetBagCollection = null;
				}
				if (_skillCollection != null)
				{
					_skillCollection.Clear();
					_skillCollection = null;
				}
				_repositoryFactory = null;
				_unitOfWorkFactory = null;

			}
		}
	}
}