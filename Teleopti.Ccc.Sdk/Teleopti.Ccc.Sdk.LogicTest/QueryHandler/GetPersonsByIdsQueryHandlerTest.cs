using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
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
	public class GetPersonsByIdsQueryHandlerTest
	{
		[Test]
		public void ShouldGetPeopleByTheirIds()
		{
			var personRepository = new FakePersonRepositoryLegacy();

			var assembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy());
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var person1 = PersonFactory.CreatePerson().WithId(personId1);
			personRepository.Add(person1);
			var person2 = PersonFactory.CreatePerson().WithId(personId2);
			personRepository.Add(person2);

			var target = new GetPersonsByIdsQueryHandler(new PersonCredentialsAppender(assembler, new TenantPeopleLoader(new FakeTenantLogonDataManager())), personRepository, new FakeCurrentUnitOfWorkFactory(null));

			var result = target.Handle(new GetPersonsByIdsQueryDto
			{
				PersonIds = new [] { personId1, personId2 }
			});

			result.Count.Should().Be.EqualTo(2);
			result.Any(x => x.Id == personId1).Should().Be.True();
			result.Any(x => x.Id == personId2).Should().Be.True();
		}

		[Test]
		public void ShouldGetPeopleByTheirIdsIncludingIdentity()
		{
			var personRepository = new FakePersonRepositoryLegacy();

			var assembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy());
			var personId1 = Guid.NewGuid();
			var person1 = PersonFactory.CreatePerson().WithId(personId1);
			personRepository.Add(person1);

			var tenantDataManager = new FakeTenantLogonDataManager();
			tenantDataManager.SetLogon(personId1,"","Identity");
			var target = new GetPersonsByIdsQueryHandler(new PersonCredentialsAppender(assembler, new TenantPeopleLoader(tenantDataManager)), personRepository, new FakeCurrentUnitOfWorkFactory(null));

			var result = target.Handle(new GetPersonsByIdsQueryDto
			{
				PersonIds = new[] { personId1 }
			});

			result.Count.Should().Be.EqualTo(1);
			result.First().Identity.Should().Be.EqualTo("Identity");
		}
	}
}