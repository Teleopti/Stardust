using System.Collections.Generic;
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
	public class GetPersonByEmploymentNumberQueryHandlerTest
	{
		private MockRepository mocks;
		private GetPersonByEmploymentNumberQueryHandler target;
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
			target = new GetPersonByEmploymentNumberQueryHandler(assembler, personRepository, currentUnitOfWorkFactory);
		}

		[Test]
		public void ShouldGetPeopleByEmploymentNumber()
		{
			var personList = new List<IPerson> {PersonFactory.CreatePerson()};
			using (mocks.Record())
			{
				Expect.Call(personRepository.FindPeopleByEmploymentNumber("127")).Return(personList);
				Expect.Call(assembler.DomainEntitiesToDtos(personList)).Return(new[] {new PersonDto()});
				Expect.Call(unitOfWorkFactory.CreateAndOpenUnitOfWork()).Return(unitOfWork);
				Expect.Call(currentUnitOfWorkFactory.Current()).Return(unitOfWorkFactory);
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetPersonByEmploymentNumberQueryDto { EmploymentNumber = "127"});
				result.Count.Should().Be.EqualTo(1);
			}
		}
	}
}