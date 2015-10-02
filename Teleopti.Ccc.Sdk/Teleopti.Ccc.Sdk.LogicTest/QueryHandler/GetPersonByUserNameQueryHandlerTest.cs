using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.MultiTenancyAuthentication;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetPersonByUserNameQueryHandlerTest
	{
		[Test]
		public void ShouldGetPeopleByUserName()
		{
			var assembler = MockRepository.GenerateMock<IAssembler<IPerson, PersonDto>>();
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var personInfo = new PersonInfo();
			const string logonName = "username1";
			var person = PersonFactory.CreatePerson();
			person.SetId(personInfo.Id);
			var persons = new List<IPerson> { person };
			personRepository.Stub(x => x.FindPeople(new[] { personInfo.Id})).Return(persons);
			var personDto = new PersonDto();
			assembler.Stub(x => x.DomainEntitiesToDtos(persons)).Return(new[] {personDto});
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(MockRepository.GenerateMock<IUnitOfWork>());
			currentUnitOfWorkFactory.Stub(x => x.Current()).Return(unitOfWorkFactory);
			var tenantLogonDataManager = MockRepository.GenerateMock<ITenantLogonDataManager>();
			tenantLogonDataManager.Stub(x => x.GetLogonInfoForLogonName(logonName)).Return(new LogonInfoModel
			{
				LogonName = logonName,
				PersonId = personInfo.Id
			});
			var target = new GetPersonByUserNameQueryHandler(assembler, personRepository, currentUnitOfWorkFactory, tenantLogonDataManager);

			var result = target.Handle(new GetPersonByUserNameQueryDto
			{
				UserName = logonName
			});

			result.Count.Should().Be.EqualTo(1);
			result.First().Should().Be.EqualTo(personDto);
		}
	}
}