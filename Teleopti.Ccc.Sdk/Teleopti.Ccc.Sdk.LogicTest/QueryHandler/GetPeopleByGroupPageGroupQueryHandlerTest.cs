using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Principal;
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
	public class GetPeopleByGroupPageGroupQueryHandlerTest
	{
		[Test]
		public void ShouldGetPeopleByGroupInGroupPage()
		{
			var groupPageGroupId = Guid.NewGuid();
			var personWithId = PersonFactory.CreatePersonWithId();
			var personRepository = new FakePersonRepositoryLegacy();
			personRepository.Add(personWithId);
			var groupingReadOnlyRepository = new FakeGroupingReadOnlyRepository(new ReadOnlyGroupDetail {PersonId = personWithId.Id.GetValueOrDefault()});
			var assembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy());

			var dataManager = new FakeTenantLogonDataManager();
			dataManager.SetLogon(personWithId.Id.Value,"aaa","");

			var unitOfWorkFactory = new FakeCurrentUnitOfWorkFactory(null);
			var target = new GetPeopleByGroupPageGroupQueryHandler(groupingReadOnlyRepository, personRepository,
				new PersonCredentialsAppender(assembler, new TenantPeopleLoader(dataManager)), unitOfWorkFactory);
			var dateOnly = new DateOnly(2012, 4, 30);

			using (CurrentAuthorization.ThreadlyUse(new FullPermission()))
			{
				var result =
					target.Handle(new GetPeopleByGroupPageGroupQueryDto
					{
						GroupPageGroupId = groupPageGroupId,
						QueryDate = new DateOnlyDto {DateTime = dateOnly.Date}
					});
				result.Count.Should().Be.EqualTo(1);
				result.First().ApplicationLogOnName.Should().Be.EqualTo("aaa");
			}
		}
	}
}