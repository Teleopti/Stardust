using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Domain.WorkflowControl.ShiftTrades;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;


namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetPeopleForShiftTradeByGroupPageGroupQueryHandlerTest
	{
		[Test]
		public void ShouldGetPeopleByGroupInGroupPage()
		{
			var groupingReadOnlyRepository = new FakeGroupingReadOnlyRepository();
			var personRepository = new FakePersonRepository(new FakeStorage());
			var assembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()),
					new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy());

			var unitOfWorkFactory = new FakeCurrentUnitOfWorkFactory(new FakeStorage());
			var shiftTradeValidationList = new List<IShiftTradeLightSpecification>();
			var shiftTradeLightValidator = new ShiftTradeLightValidator(shiftTradeValidationList);
			var dataManager = new FakeTenantLogonDataManager();
			var target = new GetPeopleForShiftTradeByGroupPageGroupQueryHandler(
				new PersonCredentialsAppender(assembler, new TenantPeopleLoader(dataManager)),
				groupingReadOnlyRepository, personRepository, unitOfWorkFactory, shiftTradeLightValidator);
			
			var groupPageGroupId = Guid.NewGuid();
			var dateOnly = new DateOnly(2012,4,30);
			var queryPerson = PersonFactory.CreatePerson().WithId();
			var person = PersonFactory.CreatePerson().WithId();

			dataManager.SetLogon(person.Id.Value,"aaa","");

			personRepository.Add(queryPerson);
			personRepository.Add(person);

			groupingReadOnlyRepository.Has(new ReadOnlyGroupDetail {PersonId = person.Id.Value, GroupId = groupPageGroupId});

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var result = target.Handle(new GetPeopleForShiftTradeByGroupPageGroupQueryDto
				{
					GroupPageGroupId = groupPageGroupId, QueryDate = new DateOnlyDto {DateTime = dateOnly.Date},
					PersonId = queryPerson.Id.Value
				});
				result.Count.Should().Be.EqualTo(1);
				result.First().ApplicationLogOnName.Should().Be.EqualTo("aaa");
			}
		}
	}
}