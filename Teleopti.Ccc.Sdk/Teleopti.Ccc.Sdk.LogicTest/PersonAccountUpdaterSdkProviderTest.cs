using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Logic;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest
{
	[TestFixture]
	public class PersonAccountUpdaterSdkProviderTest
	{
		private PersonAccountUpdaterSdkProvider _target;
		private MockRepository _mocks;

		private IRepositoryFactory _repositoryFactory;
		private ICurrentUnitOfWork _unitOfWorkFactory;

		[SetUp]
		public void Setup()
		{
			_mocks = new MockRepository();
			_repositoryFactory = _mocks.Stub<IRepositoryFactory>();
			_unitOfWorkFactory = _mocks.Stub<ICurrentUnitOfWork>();
			_target = new PersonAccountUpdaterSdkProvider(_repositoryFactory, _unitOfWorkFactory);
		}

		[Test]
		public void ShouldGetRefreshService()
		{
			var scenarioDepository = _mocks.Stub<IScenarioRepository>();
			var unitOfWork = _mocks.Stub<IUnitOfWork>();

			using (_mocks.Record())
			{
				_unitOfWorkFactory.Stub(s => s.Current()).Return(unitOfWork);
				_repositoryFactory.Stub(s => s.CreateScenarioRepository(unitOfWork)).Return(scenarioDepository);
				scenarioDepository.Stub(s => s.LoadDefaultScenario());
			}
			using (_mocks.Playback())
			{
				Assert.IsNotNull(_target.GetRefreshService());
			}
		}

		[Test]
		public void VerifyGetPersonAccounts()
		{
			var personAccounts = _mocks.Stub<IPersonAccountCollection>();
			var unitOfWork = _mocks.Stub<IUnitOfWork>();
			var person = PersonFactory.CreatePerson();
			var personAccountRepository = _mocks.Stub<IPersonAbsenceAccountRepository>();

			using (_mocks.Record())
			{
				_unitOfWorkFactory.Stub(s => s.Current()).Return(unitOfWork);
				_repositoryFactory.Stub(s => s.CreatePersonAbsenceAccountRepository(unitOfWork)).Return(personAccountRepository);
				personAccountRepository.Stub(s => s.Find(person)).Return(personAccounts);

			}
			using (_mocks.Playback())
			{
				Assert.IsNotNull(_target.GetPersonAccounts(person));
			}
		}
	}
}
