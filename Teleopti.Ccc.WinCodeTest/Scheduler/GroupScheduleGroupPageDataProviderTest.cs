using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Scheduler
{
	[TestFixture]
	public class GroupScheduleGroupPageDataProviderTest
	{
		private GroupScheduleGroupPageDataProvider _target;
		private MockRepository _mocks;
		private ISchedulerStateHolder _stateHolder;
		private IRepositoryFactory _repositoryFactory;
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IUnitOfWork _uow;
	    private ISchedulingResultStateHolder _resultHolder;

	    [SetUp]
		public void Setup()
	    {
	        var persons = new List<IPerson>();
			_mocks = new MockRepository();
            _resultHolder = _mocks.StrictMock<ISchedulingResultStateHolder>();
            _repositoryFactory = _mocks.StrictMock<IRepositoryFactory>();
			_unitOfWorkFactory = _mocks.StrictMock<IUnitOfWorkFactory>();
			_uow = _mocks.StrictMock<IUnitOfWork>();
            
            Expect.Call(_resultHolder.PersonsInOrganization).Return(persons);
            _mocks.ReplayAll();
            _stateHolder = new SchedulerStateHolder(ScenarioFactory.CreateScenarioAggregate(), new DateOnlyPeriodAsDateTimePeriod(new DateOnlyPeriod(), TeleoptiPrincipal.Current.Regional.TimeZone), new List<IPerson>(), _resultHolder);
			_target = new GroupScheduleGroupPageDataProvider(_stateHolder, _repositoryFactory, _unitOfWorkFactory);
            _mocks.BackToRecordAll();
		}

		[Test]
		public void VerifyBusinessUnitCollection()
		{
			using (_mocks.Record())
			{
				
			}

            var expected = ((TeleoptiIdentity)TeleoptiPrincipal.Current.Identity).BusinessUnit;

			using (_mocks.Playback())
			{
				Assert.AreEqual(expected, _target.BusinessUnitCollection.FirstOrDefault());
			}
		}

		[Test]
		public void VerifyContractCollection()
		{
			var repository = _mocks.StrictMock<IContractRepository>();
			using (_mocks.Record())
			{
				commonMocks();
				Expect.Call(_repositoryFactory.CreateContractRepository(_uow)).Return(repository);
				Expect.Call(repository.LoadAll()).Return(new List<IContract>());
			}

			object expected = null;

			using (_mocks.Playback())
			{
				Assert.AreEqual(expected, _target.ContractCollection.FirstOrDefault());
			}
		}

		[Test]
		public void VerifyContractScheduleCollection()
		{
			var repository = _mocks.StrictMock<IContractScheduleRepository>();
			using (_mocks.Record())
			{
				commonMocks();
				Expect.Call(_repositoryFactory.CreateContractScheduleRepository(_uow)).Return(repository);
				Expect.Call(repository.LoadAll()).Return(new List<IContractSchedule>());
			}

			object expected = null;

			using (_mocks.Playback())
			{
				Assert.AreEqual(expected, _target.ContractScheduleCollection.FirstOrDefault());
			}
		}

		[Test]
		public void VerifyPartTimePercentageCollection()
		{
			var repository = _mocks.StrictMock<IPartTimePercentageRepository>();
			using (_mocks.Record())
			{
				commonMocks();
				Expect.Call(_repositoryFactory.CreatePartTimePercentageRepository(_uow)).Return(repository);
				Expect.Call(repository.LoadAll()).Return(new List<IPartTimePercentage>());
			}

			object expected = null;

			using (_mocks.Playback())
			{
				Assert.AreEqual(expected, _target.PartTimePercentageCollection.FirstOrDefault());
			}
		}

		[Test]
		public void VerifyPersonCollection()
		{
			using (_mocks.Record())
			{

			}

			object expected = null;

			using (_mocks.Playback())
			{
				Assert.AreEqual(expected, _target.PersonCollection.FirstOrDefault());
			}
		}

		[Test]
		public void VerifyRuleSetBagCollection()
		{
			var repository = _mocks.StrictMock<IRuleSetBagRepository>();
			using (_mocks.Record())
			{
				commonMocks();
				Expect.Call(_repositoryFactory.CreateRuleSetBagRepository(_uow)).Return(repository);
				Expect.Call(repository.LoadAll()).Return(new List<IRuleSetBag>());
			}

			object expected = null;

			using (_mocks.Playback())
			{
				Assert.AreEqual(expected, _target.RuleSetBagCollection.FirstOrDefault());
			}
		}

		[Test]
		public void VerifySelectedPeriod()
		{
			var period = new DateOnlyPeriod(new DateOnly(2000, 1, 1), new DateOnly(2010, 1, 1));
			((GroupScheduleGroupPageDataProvider)_target).SetSelectedPeriod(period);
			Assert.AreEqual(period, _target.SelectedPeriod);
		}

		[Test]
		public void VerifyUserDefinedGroupings()
		{
			var repository = _mocks.StrictMock<IGroupPageRepository>();
            var dic = _mocks.StrictMock<IScheduleDictionary>();
			using (_mocks.Record())
			{
				Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_uow);
                Expect.Call(_resultHolder.Schedules).Return(dic).Repeat.Twice();
                Expect.Call(dic.Keys).Return(new List<IPerson>()).Repeat.Twice();
				Expect.Call(() => _uow.Reassociate(new List<IPerson>()));
				Expect.Call(_repositoryFactory.CreateGroupPageRepository(_uow)).Return(repository);
				Expect.Call(repository.LoadAllGroupPageWhenPersonCollectionReAssociated()).Return(new List<IGroupPage>());
				Expect.Call(() => _uow.Dispose());

			}

			using (_mocks.Playback())
			{
				Assert.AreEqual(0, _target.UserDefinedGroupings.Count());
			}
		}

        [Test]
        public void VerifySkillCollection()
        {
            var repository = _mocks.StrictMock<ISkillRepository>();
            using (_mocks.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_uow);
                Expect.Call(_repositoryFactory.CreateSkillRepository(_uow)).Return(repository);
                Expect.Call(repository.LoadAll()).Return(new List<ISkill>());
                Expect.Call(() => _uow.Dispose());
            }

            object expected = null;

            using (_mocks.Playback())
            {
                Assert.AreEqual(expected, _target.SkillCollection.FirstOrDefault());
            }
        }

		[Test]
		public void ShouldRemoveUnloadedPersonsFromGroupPages()
		{
			var dic = _mocks.StrictMock<IScheduleDictionary>();
			var person = _mocks.StrictMock<IPerson>();
			var person2 = _mocks.StrictMock<IPerson>();
			var person3 = _mocks.StrictMock<IPerson>();
			ICollection<IPerson> keys = new List<IPerson> {person3};

			var groupPage = new GroupPage("root");

			var rootPersonGroup = new RootPersonGroup();
			groupPage.AddRootPersonGroup(rootPersonGroup);
			rootPersonGroup.AddPerson(person);
			var childGroup = new ChildPersonGroup();
			childGroup.AddPerson(person2);
			childGroup.AddPerson(person3);
			
			rootPersonGroup.AddChildGroup(childGroup);
			Expect.Call(_stateHolder.Schedules).Return(dic);
			Expect.Call(dic.Keys).Return(keys);
			_mocks.ReplayAll();
			_target.RemoveNotLoadedPersonsFromCollection(new List<IGroupPage>{groupPage});
			Assert.That(rootPersonGroup.PersonCollection,Is.Empty);
			Assert.That(childGroup.PersonCollection.Contains(person3));
			Assert.That(childGroup.PersonCollection.Contains(person2),Is.False);
			_mocks.VerifyAll();
		}
		private void commonMocks()
		{
			Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(_uow);
			Expect.Call(_uow.DisableFilter(QueryFilter.Deleted)).Return(_uow);

			Expect.Call(() => _uow.Dispose()).Repeat.Twice();
		}
	}
}