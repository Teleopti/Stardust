using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
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
	public class GetPeopleByGroupPageGroupQueryHandlerTest
	{
		private MockRepository mocks;
		private IGroupingReadOnlyRepository groupingReadOnlyRepository;
		private GetPeopleByGroupPageGroupQueryHandler target;
		private IAssembler<IPerson, PersonDto> assembler;
		private IPersonRepository personRepository;
		private ICurrentUnitOfWorkFactory unitOfWorkFactory;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			groupingReadOnlyRepository = mocks.DynamicMock<IGroupingReadOnlyRepository>();
			assembler = mocks.StrictMock<IAssembler<IPerson, PersonDto>>();
			personRepository = mocks.StrictMock<IPersonRepository>();
			unitOfWorkFactory = mocks.DynamicMock<ICurrentUnitOfWorkFactory>();
			target = new GetPeopleByGroupPageGroupQueryHandler(groupingReadOnlyRepository,personRepository,assembler,unitOfWorkFactory);
		}

		[Test]
		public void ShouldGetPeopleByGroupInGroupPage()
		{
			var groupPageGroupId = Guid.NewGuid();
			var dateOnly = new DateOnly(2012,4,30);
			var personId = Guid.NewGuid();
			var detailList = new List<ReadOnlyGroupDetail> {new ReadOnlyGroupDetail {PersonId = personId}};
			var personList = new List<IPerson> {PersonFactory.CreatePerson()};
			using (mocks.Record())
			{
			    Expect.Call(unitOfWorkFactory.LoggedOnUnitOfWorkFactory()).Return(mocks.DynamicMock<IUnitOfWorkFactory>());
				Expect.Call(groupingReadOnlyRepository.DetailsForGroup(groupPageGroupId, dateOnly)).Return(detailList);
				Expect.Call(personRepository.FindPeople((IEnumerable<Guid>)null)).Constraints(Rhino.Mocks.Constraints.List.Equal(new List<Guid> { personId })).Return(personList);
				Expect.Call(assembler.DomainEntitiesToDtos(personList)).Return(new[] {new PersonDto()});
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetPeopleByGroupPageGroupQueryDto{ GroupPageGroupId = groupPageGroupId, QueryDate = new DateOnlyDto { DateTime = dateOnly.Date } });
				result.Count.Should().Be.EqualTo(1);
			}
		}
	}
}