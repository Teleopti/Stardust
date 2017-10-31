using System;
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
	public class GetPersonByIdQueryHandlerTest
	{
		[Test]
		public void ShouldGetPersonById()
		{
			var personRepository = new FakePersonRepositoryLegacy();

			var assembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy(),
				new TenantPeopleLoader(new FakeTenantLogonDataManager()));

			var person = PersonFactory.CreatePerson().WithId();
			personRepository.Add(person);

			var target = new GetPersonByIdQueryHandler(assembler, personRepository, new FakeCurrentUnitOfWorkFactory(new FakeStorage()));
			var result = target.Handle(new GetPersonByIdQueryDto {PersonId = person.Id.Value});
			result.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldHandlePersonByIdNotFound()
		{
			var personRepository = new FakePersonRepositoryLegacy();

			var assembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy(),
				new TenantPeopleLoader(new FakeTenantLogonDataManager()));

			var person = PersonFactory.CreatePerson().WithId();
			personRepository.Add(person);

			var target = new GetPersonByIdQueryHandler(assembler, personRepository, new FakeCurrentUnitOfWorkFactory(new FakeStorage()));
			var result = target.Handle(new GetPersonByIdQueryDto { PersonId = Guid.NewGuid() });
			result.Count.Should().Be.EqualTo(0);
		}
	}
}