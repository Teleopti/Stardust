using System;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetPersonRolesQueryHandlerTest
	{
		[Test]
		public void ShouldGetRolesForPerson()
		{
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			person.SetId(Guid.NewGuid());

			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			currentUnitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(unitOfWorkFactory);
			personRepository.Stub(x => x.Load(person.Id.GetValueOrDefault())).Return(person);

			var target = new GetPersonRolesQueryHandler(personRepository, currentUnitOfWorkFactory);

			var result = target.Handle(new GetPersonRolesQueryDto{PersonId = person.Id.GetValueOrDefault()});

			result.First().Id.Should().Be.EqualTo(person.PermissionInformation.ApplicationRoleCollection.First().Id);
		}

		[Test]
		public void ShouldGetDeletedRolesForPerson()
		{
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var unitOfWork = MockRepository.GenerateMock<IUnitOfWork>();
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			var person = PersonFactory.CreatePersonWithApplicationRolesAndFunctions();
			((IDeleteTag)person.PermissionInformation.ApplicationRoleCollection.First()).SetDeleted();
			person.SetId(Guid.NewGuid());

			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			currentUnitOfWorkFactory.Stub(x => x.LoggedOnUnitOfWorkFactory()).Return(unitOfWorkFactory);
			personRepository.Stub(x => x.Load(person.Id.GetValueOrDefault())).Return(person);

			var target = new GetPersonRolesQueryHandler(personRepository, currentUnitOfWorkFactory);

			var result = target.Handle(new GetPersonRolesQueryDto { PersonId = person.Id.GetValueOrDefault() });

			result.First().IsDeleted.Should().Be.True();
		}
	}

	

	
}