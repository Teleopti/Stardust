using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.WinCode.Common.PersonPicker;
using Teleopti.Ccc.WinCodeTest.Common.Commands;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WinCodeTest.Common.PersonPicker
{
    [TestFixture]
    public class PersonPickerViewModelTest
    {
        private PersonPickerViewModel _target;
        private IRepositoryFactory _repositoryFactory;
        private MockRepository _mocker;
        private TesterForCommandModels _testerForCommandModels;
        private IUnitOfWorkFactory _unitOfWorkFactory;

        [SetUp]
        public void Setup()
        {
            _testerForCommandModels = new TesterForCommandModels();
            _mocker = new MockRepository();
            _repositoryFactory = _mocker.StrictMock<IRepositoryFactory>();
            _unitOfWorkFactory = _mocker.StrictMock<IUnitOfWorkFactory>();
            _target = new PersonPickerViewModel(_repositoryFactory, _unitOfWorkFactory);
        }

        [Test]
        public void VerifyRepositoryFactoryIsCreated()
        {
            _target = new PersonPickerViewModel();
            Assert.IsNotNull(_target.RepFactory);
        }

        [Test]
        public void VerifyLoadAllCommand()
        {
            _target.People.Add(new SelectablePersonViewModel(PersonFactory.CreatePerson("should be cleared when loaded")));

            IPersonRepository repository = _mocker.StrictMock<IPersonRepository>();
            IUnitOfWork unitOfWork = _mocker.StrictMock<IUnitOfWork>();
            IPerson person1 = PersonFactory.CreatePerson();
            IPerson person2 = PersonFactory.CreatePerson();

            IList<IPerson> loadedPeople = new List<IPerson> { person1, person2 };

            using (_mocker.Record())
            {
                Expect.Call(_unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(_repositoryFactory.CreatePersonRepository(unitOfWork)).Return(repository);
                Expect.Call(repository.LoadAll()).Return(loadedPeople);
                unitOfWork.Dispose();
            }
            using (_mocker.Playback())
            {
                _testerForCommandModels.ExecuteCommandModel(_target.LoadAllCommand);
                Assert.AreEqual(2,_target.People.Count);
            }
            SetupFixtureForAssembly.ResetStateHolder();
        }

        [Test]
        public void VerifyCanAddListOfPeople()
        {
            IList<IPerson> peopleToAdd = new List<IPerson> {PersonFactory.CreatePerson(),PersonFactory.CreatePerson(),PersonFactory.CreatePerson()};
            Assert.IsEmpty(_target.People);
            _target.SetPeople(peopleToAdd);
            Assert.AreEqual(peopleToAdd.Count,_target.People.Count);
        }
    }
}
