using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
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
	public class GetPersonByIdentityQueryDtoHandlerTest
	{
		[Test]
		public void ShouldGetPeopleByIdentity()
		{
			var assembler = MockRepository.GenerateMock<IAssembler<IPerson, PersonDto>>();
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var findPersonInfoByIdentity = MockRepository.GenerateMock<IFindPersonInfoByIdentity>();
			var personInfo = new PersonInfo();
			findPersonInfoByIdentity.Stub(x => x.Find("identity1")).Return(personInfo);
			var person = PersonFactory.CreatePerson();
			person.SetId(personInfo.Id);
			var persons = new List<IPerson> { person };
			personRepository.Stub(x => x.FindPeople(new[] { personInfo.Id })).Return(persons);
			var personDto = new PersonDto();
			assembler.Stub(x => x.DomainEntitiesToDtos(persons)).Return(new[] { personDto });
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(MockRepository.GenerateMock<IUnitOfWork>());
			currentUnitOfWorkFactory.Stub(x => x.Current()).Return(unitOfWorkFactory);
			var target = new GetPersonByIdentityQueryDtoHandler(assembler, personRepository, currentUnitOfWorkFactory, findPersonInfoByIdentity);

			var result = target.Handle(new GetPersonByIdentityQueryDto
			{
				Identity = "identity1"
			});

			result.Count.Should().Be.EqualTo(1);
			result.First().Should().Be.EqualTo(personDto);
		}
	}
}