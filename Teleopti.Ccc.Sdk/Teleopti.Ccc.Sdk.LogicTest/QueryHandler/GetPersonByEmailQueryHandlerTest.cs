using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
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
	public class GetPersonByEmailQueryHandlerTest
	{
		[Test]
		public void ShouldGetPeopleByEmail()
		{
			var assembler = MockRepository.GenerateMock<IAssembler<IPerson, PersonDto>>();
			var personRepository = MockRepository.GenerateMock<IPersonRepository>();
			var currentUnitOfWorkFactory = MockRepository.GenerateMock<ICurrentUnitOfWorkFactory>();
			var person = PersonFactory.CreatePerson();
			personRepository.Stub(x => x.FindPeopleByEmail(person.Email)).Return(new []{person});
			var personDto = new PersonDto();
			assembler.Stub(x => x.DomainEntitiesToDtos(new []{person})).Return(new[] { personDto });
			var unitOfWorkFactory = MockRepository.GenerateMock<IUnitOfWorkFactory>();
			unitOfWorkFactory.Stub(x => x.CreateAndOpenUnitOfWork()).Return(MockRepository.GenerateMock<IUnitOfWork>());
			currentUnitOfWorkFactory.Stub(x => x.Current()).Return(unitOfWorkFactory);
			var target = new GetPersonByEmailQueryHandler(assembler, personRepository, currentUnitOfWorkFactory);

			var result = target.Handle(new GetPersonByEmailQueryDto
			{
				Email = person.Email
			});

			result.Count.Should().Be.EqualTo(1);
			result.First().Should().Be.EqualTo(personDto);
		}
	}
}