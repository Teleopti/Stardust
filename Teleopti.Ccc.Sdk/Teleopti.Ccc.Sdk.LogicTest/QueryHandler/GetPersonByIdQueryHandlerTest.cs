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
	    private ICurrentUnitOfWorkFactory currentUnitOfWorkFactory;

	    [SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			assembler = mocks.StrictMock<IAssembler<IPerson, PersonDto>>();
			personRepository = mocks.StrictMock<IPersonRepository>();
            unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
            currentUnitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			unitOfWork = mocks.DynamicMock<IUnitOfWork>();
			target = new GetPersonByIdQueryHandler(assembler, personRepository, currentUnitOfWorkFactory);
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
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetPersonByIdQueryDto { PersonId= personId});
				result.Count.Should().Be.EqualTo(1);
			}
		}

		[Test]
		public void ShouldHandlePersonByIdNotFound()
		{
			var personId = Guid.NewGuid();
			using (mocks.Record())
			{
				Expect.Call(personRepository.Get(personId)).Return(null);
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
                Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
            }
			using (mocks.Playback())
			{
				var result = target.Handle(new GetPersonByIdQueryDto { PersonId = personId });
				result.Count.Should().Be.EqualTo(0);
			}
		}
	}
}