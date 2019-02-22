using System.ServiceModel;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.Commands;
using Teleopti.Ccc.Sdk.Logic.CommandHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.Sdk.LogicTest.CommandHandler
{
	[TestFixture]
	public class RevokePersonRoleCommandHandlerTest
	{
		private IUnitOfWorkFactory _unitOfWorkFactory;
		private IPersonRepository _personRepository;
		private IPerson _person;
		private ICurrentUnitOfWorkFactory _currentUnitOfWorkFactory;
		private IApplicationRoleRepository _applicationRoleRepository;
		private ApplicationRole _role;
		private RevokePersonRoleCommandDto _commandDto;

		[SetUp]
		public void Setup()
		{
			_unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			_currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			_currentUnitOfWorkFactory.Stub(x => x.Current()).Return(_unitOfWorkFactory);
			_personRepository = MockRepository.GenerateMock<IPersonRepository>();
			_applicationRoleRepository = MockRepository.GenerateMock<IApplicationRoleRepository>();
			
			_person = PersonFactory.CreatePerson("test").WithId();
			_role = ApplicationRoleFactory.CreateRole("rolf", "walking").WithId();

			_commandDto = new RevokePersonRoleCommandDto
			{
				PersonId = _person.Id.GetValueOrDefault(),
				RoleId = _role.Id.GetValueOrDefault()
			};

		}

		[Test]
		public void ShouldHandleRevokePersonRoleCommand()
		{
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			_personRepository.Stub(x => x.Get(_person.Id.GetValueOrDefault())).Return(_person);
			_applicationRoleRepository.Stub(x => x.Get(_role.Id.GetValueOrDefault())).Return(_role);
			_person.PermissionInformation.AddApplicationRole(_role);

			var target = new RevokePersonRoleCommandHandler(_currentUnitOfWorkFactory, _personRepository, _applicationRoleRepository, new FullPermission());
			target.Handle(_commandDto);

			_commandDto.Result.AffectedItems.Should().Be.EqualTo(1);
            _commandDto.Result.AffectedId.Should().Be.EqualTo(_person.Id.GetValueOrDefault());
            _person.PermissionInformation.ApplicationRoleCollection.Should().Not.Contain(_role);
			unitOfWork.AssertWasCalled(x => x.PersistAll());
		}

        [Test]
        public void ShouldHandleRevokePersonRoleCommandWhenPersonNotFound()
        {
            var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            _personRepository.Stub(x => x.Get(_person.Id.GetValueOrDefault())).Return(null);
            _applicationRoleRepository.Stub(x => x.Get(_role.Id.GetValueOrDefault())).Return(_role);

			var target = new RevokePersonRoleCommandHandler(_currentUnitOfWorkFactory, _personRepository, _applicationRoleRepository, new FullPermission());
			target.Handle(_commandDto);
			
			_commandDto.Result.AffectedItems.Should().Be.EqualTo(0);
            unitOfWork.AssertWasNotCalled(x => x.PersistAll());
        }

        [Test]
        public void ShouldHandleRevokePersonRoleCommandWhenRoleNotFound()
        {
            var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
            _unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
            _personRepository.Stub(x => x.Get(_person.Id.GetValueOrDefault())).Return(_person);
            _applicationRoleRepository.Stub(x => x.Get(_role.Id.GetValueOrDefault())).Return(null);

			var target = new RevokePersonRoleCommandHandler(_currentUnitOfWorkFactory, _personRepository, _applicationRoleRepository, new FullPermission());
			target.Handle(_commandDto);
			
			_commandDto.Result.AffectedItems.Should().Be.EqualTo(0);
            unitOfWork.AssertWasNotCalled(x => x.PersistAll());
        }

		[Test]
		public void ShouldNotAllowRevokePersonRoleWhenNotPermitted()
		{
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			_unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			_personRepository.Stub(x => x.Get(_person.Id.GetValueOrDefault())).Return(_person);
			_applicationRoleRepository.Stub(x => x.Get(_role.Id.GetValueOrDefault())).Return(_role);
			_person.PermissionInformation.AddApplicationRole(_role);

			var target = new RevokePersonRoleCommandHandler(_currentUnitOfWorkFactory, _personRepository, _applicationRoleRepository, new NoPermission());
			Assert.Throws<FaultException>(() => target.Handle(_commandDto));

			unitOfWork.AssertWasNotCalled(x => x.PersistAll());
		}
	}
}