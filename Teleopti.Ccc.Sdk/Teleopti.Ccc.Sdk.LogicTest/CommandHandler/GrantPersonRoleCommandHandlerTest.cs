using System;
using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	public class GrantPersonRoleCommandHandlerTest
    {
        private IUnitOfWorkFactory _unitOfWorkFactory;
        private IPersonRepository _personRepository;
		private GrantPersonRoleCommandHandler _target;
        private IPerson _person;
        private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
	    private IApplicationRoleRepository _applicationRoleRepository;
	    private ApplicationRole _role;
	    private GrantPersonRoleCommandDto _commandDto;

	    [SetUp]
        public void Setup()
        {
            _unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
            _currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
		    _currentUnitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(_unitOfWorkFactory);
            _personRepository = MockRepository.GenerateMock<IPersonRepository>();
            _applicationRoleRepository = MockRepository.GenerateMock<IApplicationRoleRepository>();
            _target = new GrantPersonRoleCommandHandler(_currentUnitOfWorkFactory,_personRepository, _applicationRoleRepository);

            _person = PersonFactory.CreatePerson("test");
            _person.SetId(Guid.NewGuid());
	        _role = ApplicationRoleFactory.CreateRole("rolf", "walking");
			_role.SetId(Guid.NewGuid());

            _commandDto = new GrantPersonRoleCommandDto
            {
                PersonId = _person.Id.GetValueOrDefault(),
				RoleId = _role.Id.GetValueOrDefault()
            };

        }

        [Test]
        public void ShouldHandleGrantPersonRoleCommand()
        {
            var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
	        _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
	        _personRepository.Stub(x => x.Get(_person.Id.GetValueOrDefault())).Return(_person);
	        _applicationRoleRepository.Stub(x => x.Get(_role.Id.GetValueOrDefault())).Return(_role);

			_target.Handle(_commandDto);

	        _person.PermissionInformation.ApplicationRoleCollection.Should().Contain(_role);
            _commandDto.Result.AffectedItems.Should().Be.EqualTo(1);
            _commandDto.Result.AffectedId.Should().Be.EqualTo(_person.Id.GetValueOrDefault());
			unitOfWork.AssertWasCalled(x => x.PersistAll());
        }

        [Test]
        public void ShouldHandleGrantPersonRoleCommandWhenPersonNotFound()
        {
            var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            _personRepository.Stub(x => x.Get(_person.Id.GetValueOrDefault())).Return(null);
            _applicationRoleRepository.Stub(x => x.Get(_role.Id.GetValueOrDefault())).Return(_role);

            _target.Handle(_commandDto);

            _commandDto.Result.AffectedItems.Should().Be.EqualTo(0);
            unitOfWork.AssertWasNotCalled(x => x.PersistAll());
        }

        [Test]
        public void ShouldHandleGrantPersonRoleCommandWhenRoleNotFound()
        {
            var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            _personRepository.Stub(x => x.Get(_person.Id.GetValueOrDefault())).Return(_person);
            _applicationRoleRepository.Stub(x => x.Get(_role.Id.GetValueOrDefault())).Return(null);

            _target.Handle(_commandDto);

            _commandDto.Result.AffectedItems.Should().Be.EqualTo(0);
            unitOfWork.AssertWasNotCalled(x => x.PersistAll());
        }

		[Test]
		public void ShouldNotGrantPersonRoleWhenNotPermitted()
		{
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			_personRepository.Stub(x => x.Get(_person.Id.GetValueOrDefault())).Return(_person);
			_applicationRoleRepository.Stub(x => x.Get(_role.Id.GetValueOrDefault())).Return(_role);

			using (new CustomAuthorizationContext(new PrincipalAuthorizationWithNoPermission()))
			{
				Assert.Throws<FaultException>(()=> _target.Handle(_commandDto));
			}

			unitOfWork.AssertWasNotCalled(x => x.PersistAll());
		}
    }
}
