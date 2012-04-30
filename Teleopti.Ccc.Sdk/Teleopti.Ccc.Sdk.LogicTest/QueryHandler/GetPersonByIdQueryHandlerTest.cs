using System;
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
	public class GetPersonByIdQueryHandlerTest
	{
		private MockRepository mocks;
		private GetPersonByIdQueryHandler target;
		private IAssembler<IPerson, PersonDto> assembler;
		private IPersonRepository personRepository;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private IUnitOfWork unitOfWork;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			assembler = mocks.StrictMock<IAssembler<IPerson, PersonDto>>();
			personRepository = mocks.StrictMock<IPersonRepository>();
			unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			target = new GetPersonByIdQueryHandler(assembler, personRepository, unitOfWorkFactory);
		}

		[Test]
		public void ShouldGetPersonById()
		{
			var personId = Guid.NewGuid();
			var person  = PersonFactory.CreatePerson();
			using (mocks.Record())
			{
				Expect.Call(personRepository.Get(personId)).Return(person);
				Expect.Call(assembler.DomainEntityToDto(person)).Return(new PersonDto());
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetPersonByIdQueryDto { PersonId= personId});
				result.Count.Should().Be.EqualTo(1);
			}
		}
	}
}