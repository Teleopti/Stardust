using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetAllRolesQueryHandlerTest
	{
		[Test]
		public void ShouldGetAllRolesAvailable()
		{
			var applicationRoleRepository = MockRepository.GenerateMock<IApplicationRoleRepository>();
			var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var role = ApplicationRoleFactory.CreateRole("testRole", "Role for test");
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();

			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			currentUnitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(unitOfWorkFactory);
			applicationRoleRepository.Stub(x => x.LoadAllApplicationRolesSortedByName()).Return(new IApplicationRole[] {role});

			var target = new GetAllRolesQueryHandler(applicationRoleRepository, currentUnitOfWorkFactory);

			var result = target.Handle(new GetAllRolesQueryDto());

			result.First().Id.Should().Be.EqualTo(role.Id);
		}

		[Test]
		public void ShouldGetAllRolesIncludingDeleted()
		{
			var applicationRoleRepository = MockRepository.GenerateMock<IApplicationRoleRepository>();
			var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var role = ApplicationRoleFactory.CreateRole("testRole", "Role for test");
			role.SetDeleted();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();

			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			currentUnitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(unitOfWorkFactory);
			applicationRoleRepository.Stub(x => x.LoadAllApplicationRolesSortedByName()).Return(new IApplicationRole[] { role });

			var target = new GetAllRolesQueryHandler(applicationRoleRepository, currentUnitOfWorkFactory);

			var result = target.Handle(new GetAllRolesQueryDto{LoadDeleted = true});

			result.First().Id.Should().Be.EqualTo(role.Id);
			result.First().IsDeleted.Should().Be.True();

			unitOfWork.AssertWasCalled(x => x.DisableFilter(QueryFilter.Deleted));
		}
	}
}