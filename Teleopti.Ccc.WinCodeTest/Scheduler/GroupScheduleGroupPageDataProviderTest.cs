using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.TestCommon.FakeData;


namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class GroupScheduleGroupPageDataProviderTest
	{
		private GroupScheduleGroupPageDataProvider _target;
		private ISchedulerStateHolder _stateHolder;
		private IRepositoryFactory _repositoryFactory;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IUnitOfWork _uow;
	    private ISchedulingResultStateHolder _resultHolder;
		private IPerson _person1;
		private IPerson _person2;
	    private IPersonAccountUpdater _personAccountUpdater;
		private IDisableDeletedFilter _disableDeletedFilter;
		private IList<IPerson> _people;
		private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;

		[SetUp]
		public void Setup()
	    {
			_people = new List<IPerson>();
			_person1 = PersonFactory.CreatePerson();
			_person2 = PersonFactory.CreatePerson();
			_person2.TerminatePerson(new DateOnly(2005, 1, 1), new PersonAccountUpdaterDummy());
			
			_personAccountUpdater = MockRepository.GenerateMock<IPersonAccountUpdater>();
			_resultHolder = MockRepository.GenerateMock<ISchedulingResultStateHolder>();
			_repositoryFactory = MockRepository.GenerateMock<IRepositoryFactory>();
			_disableDeletedFilter = MockRepository.GenerateMock<IDisableDeletedFilter>();
			_currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			_unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			_uow = MockRepository.GenerateMock<IUnitOfWork>();
            
            _resultHolder.Stub(x => x.LoadedAgents).Return(_people);
			_currentUnitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);

            _stateHolder = new SchedulerStateHolder(ScenarioFactory.CreateScenarioAggregate(), new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(),
				TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Regional.TimeZone), new List<IPerson> { _person1, _person2 }, _disableDeletedFilter, _resultHolder);
			_target = new GroupScheduleGroupPageDataProvider(()=>_stateHolder, _repositoryFactory, _currentUnitOfWorkFactory, _disableDeletedFilter, CurrentBusinessUnit.Make());
		}

		[Test]
		public void VerifyBusinessUnitCollection()
		{
            var expected = ((ITeleoptiIdentityWithUnsafeBusinessUnit)TeleoptiPrincipalLocator_DONTUSE_REALLYDONTUSE.CurrentPrincipal.Identity).BusinessUnit();
			var repository = MockRepository.GenerateMock<IBusinessUnitRepository>();

			commonMocks();
			_repositoryFactory.Stub(x => x.CreateBusinessUnitRepository(_uow)).Return(repository);
			repository.Stub(x => x.Get(expected.Id.GetValueOrDefault())).Return(expected);
			repository.Stub(x => x.LoadHierarchyInformation(expected)).Return(expected);

			Assert.AreEqual(expected, _target.BusinessUnit);
		}

		[Test]
		public void VerifyContractCollection()
		{
			var repository = MockRepository.GenerateMock<IContractRepository>();

			commonMocks();
			_repositoryFactory.Stub(x => x.CreateContractRepository(_uow)).Return(repository);
			repository.Stub(x => x.LoadAll()).Return(new List<IContract>());

			Assert.IsNull(_target.ContractCollection.FirstOrDefault());
		}

		[Test]
		public void VerifyContractScheduleCollection()
		{
			var repository = MockRepository.GenerateMock<IContractScheduleRepository>();

			commonMocks();
			_repositoryFactory.Stub(x => x.CreateContractScheduleRepository(_uow)).Return(repository);
			repository.Stub(x => x.LoadAll()).Return(new List<IContractSchedule>());

			Assert.IsNull(_target.ContractScheduleCollection.FirstOrDefault());
		}

		[Test]
		public void VerifyPartTimePercentageCollection()
		{
			var repository = MockRepository.GenerateMock<IPartTimePercentageRepository>();

			commonMocks();
			_repositoryFactory.Stub(x => x.CreatePartTimePercentageRepository(_uow)).Return(repository);
			repository.Stub(x => x.LoadAll()).Return(new List<IPartTimePercentage>());

			Assert.IsNull(_target.PartTimePercentageCollection.FirstOrDefault());
		}

		[Test]
		public void VerifyRuleSetBagCollection()
		{
			var repository = MockRepository.GenerateMock<IRuleSetBagRepository>();

			commonMocks();
			_repositoryFactory.Stub(x => x.CreateRuleSetBagRepository(_uow)).Return(repository);
			repository.Stub(x => x.LoadAll()).Return(new List<IRuleSetBag>());

			Assert.IsNull(_target.RuleSetBagCollection.FirstOrDefault());
		}

		[Test]
		public void VerifyUserDefinedGroupings()
		{
			var repository = MockRepository.GenerateMock<IGroupPageRepository>();
			var dic = MockRepository.GenerateMock<IScheduleDictionary>();

			commonMocks();
			_resultHolder.Stub(x => x.Schedules).Return(dic).Repeat.Twice();
			dic.Stub(x => x.Keys).Return(new List<IPerson>()).Repeat.Twice();
			_repositoryFactory.Stub(x => x.CreateGroupPageRepository(_uow)).Return(repository);
			repository.Stub(x => x.LoadAllGroupPageWhenPersonCollectionReAssociated()).Return(new List<IGroupPage>());

			Assert.AreEqual(0, _target.UserDefinedGroupings(dic).Count());
			_uow.AssertWasCalled(x => x.Reassociate(new List<IPerson>()));
		}

		[Test]
		public void VerifySkillCollection()
		{
			var repository = MockRepository.GenerateMock<ISkillRepository>();
			commonMocks();
			_repositoryFactory.Stub(x => x.CreateSkillRepository(_uow)).Return(repository);
			repository.Stub(x => x.LoadAll()).Return(new List<ISkill>());

			Assert.IsNull(_target.SkillCollection.FirstOrDefault());
		}

		[Test]
		public void ShouldRemoveUnloadedPersonsFromGroupPages()
		{
			var dic = MockRepository.GenerateMock<IScheduleDictionary>();
			var person = MockRepository.GenerateMock<IPerson>();
			var person2 = MockRepository.GenerateMock<IPerson>();
			var person3 = MockRepository.GenerateMock<IPerson>();
			ICollection<IPerson> keys = new List<IPerson> {person3};

			var groupPage = new GroupPage("root");

			var rootPersonGroup = new RootPersonGroup();
			groupPage.AddRootPersonGroup(rootPersonGroup);
			rootPersonGroup.AddPerson(person);
			var childGroup = new ChildPersonGroup();
			childGroup.AddPerson(person2);
			childGroup.AddPerson(person3);
			
			rootPersonGroup.AddChildGroup(childGroup);
			_resultHolder.Stub(x => x.Schedules).Return(dic);
			dic.Stub(x => x.Keys).Return(keys);

			_target.RemoveNotLoadedPersonsFromCollection(new List<IGroupPage>{groupPage}, dic);
			Assert.That(rootPersonGroup.PersonCollection,Is.Empty);
			Assert.That(childGroup.PersonCollection.Contains(person3));
			Assert.That(childGroup.PersonCollection.Contains(person2),Is.False);
		}

		private void commonMocks()
		{
			_unitOfWorkFactory.Stub(x => x.CurrentUnitOfWork()).Throw(new Exception("Error, no current uow!"));
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(_uow);
			_disableDeletedFilter.Stub(x => x.Disable()).Return(_uow);
		}

		[Test]
		public void ShouldGetAllLoadedFromStateHolder()
		{
			var person1 = PersonFactory.CreatePerson("ittan");
			var person2 = PersonFactory.CreatePerson("tvåan");
			var person3 = PersonFactory.CreatePerson("trean");
			
			_people.Clear();
			_people.Add(person1);
			_people.Add(person2);
			_people.Add(person3);
			
            person2.TerminatePerson(new DateOnly(2015, 1, 1), _personAccountUpdater);
            person3.TerminatePerson(new DateOnly(2005, 1, 1), _personAccountUpdater);

			var result = _target.AllLoadedPersons;
			Assert.That(result.Count(),Is.EqualTo(3));
		}
	}
}