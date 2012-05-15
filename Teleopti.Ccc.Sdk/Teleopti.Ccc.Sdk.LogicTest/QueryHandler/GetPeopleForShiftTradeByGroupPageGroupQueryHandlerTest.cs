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
using Teleopti.Ccc.Sdk.Logic.Restrictions;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetPeopleForShiftTradeByGroupPageGroupQueryHandlerTest
	{
		private MockRepository mocks;
		private IGroupingReadOnlyRepository groupingReadOnlyRepository;
		private GetPeopleForShiftTradeByGroupPageGroupQueryHandler target;
		private IAssembler<IPerson, PersonDto> assembler;
		private IPersonRepository personRepository;
		private IUnitOfWorkFactory unitOfWorkFactory;
		private IList<ISpecification<IShiftTradeAvailableCheckItem>> shiftTradeValidationList;

		[SetUp]
		public void Setup()
		{
			mocks = new MockRepository();
			groupingReadOnlyRepository = mocks.DynamicMock<IGroupingReadOnlyRepository>();
			assembler = mocks.StrictMock<IAssembler<IPerson, PersonDto>>();
			personRepository = mocks.StrictMock<IPersonRepository>();
			unitOfWorkFactory = mocks.DynamicMock<IUnitOfWorkFactory>();
			shiftTradeValidationList = new List<ISpecification<IShiftTradeAvailableCheckItem>>();
			target = new GetPeopleForShiftTradeByGroupPageGroupQueryHandler(assembler, groupingReadOnlyRepository, personRepository, unitOfWorkFactory, shiftTradeValidationList);
		}

		[Test]
		public void ShouldGetPeopleByGroupInGroupPage()
		{
			var groupPageGroupId = Guid.NewGuid();
			var dateOnly = new DateOnly(2012,4,30);
			var personId = Guid.NewGuid();
			var queryPersonId = Guid.NewGuid();
			var queryPerson = PersonFactory.CreatePerson();
			var detailList = new List<ReadOnlyGroupDetail> {new ReadOnlyGroupDetail {PersonId = personId}};
			var personList = new List<IPerson> {PersonFactory.CreatePerson()};
			using (mocks.Record())
			{
				Expect.Call(groupingReadOnlyRepository.DetailsForGroup(groupPageGroupId, dateOnly)).Return(detailList);
				Expect.Call(personRepository.FindPeople((IEnumerable<Guid>)null)).Constraints(Rhino.Mocks.Constraints.List.Equal(new List<Guid> { personId })).Return(personList);
				Expect.Call(personRepository.Get(queryPersonId)).Return(queryPerson);
				Expect.Call(assembler.DomainEntitiesToDtos(personList)).Return(new[] {new PersonDto()});
			}
			using (mocks.Playback())
			{
				var result = target.Handle(new GetPeopleForShiftTradeByGroupPageGroupQueryDto{ GroupPageGroupId = groupPageGroupId, QueryDate = new DateOnlyDto { DateTime = dateOnly }, PersonId = queryPersonId});
				result.Count.Should().Be.EqualTo(1);
			}
		}
	}
}