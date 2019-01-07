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
	public class GetPersonsByEmploymentNumbersQueryHandlerTest
	{
		[Test]
		public void ShouldGetPeopleByEmploymentNumbers()
		{
			var personRepository = new FakePersonRepositoryLegacy();

			var assembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy());

			var person = PersonFactory.CreatePerson();
			person.SetEmploymentNumber("1234");
			personRepository.Add(person);
			person = PersonFactory.CreatePerson();
			person.SetEmploymentNumber("2234");
			personRepository.Add(person);

			var target = new GetPersonsByEmploymentNumbersQueryHandler(new PersonCredentialsAppender(assembler, new TenantPeopleLoader(new FakeTenantLogonDataManager())), personRepository, new FakeCurrentUnitOfWorkFactory(null));

			var result = target.Handle(new GetPersonsByEmploymentNumbersQueryDto()
			{
				EmploymentNumbers = new[] { "1234", "2234" }
			});

			result.Count.Should().Be.EqualTo(2);
			result.Last().Name.Should().Be.EqualTo(person.Name.ToString());
		}

		[Test]
		public void ShouldGetPeopleByEmploymentNumbersIncludingUsername()
		{
			var personRepository = new FakePersonRepositoryLegacy();

			var assembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy());
			
			var person = PersonFactory.CreatePerson().WithId();
			person.SetEmploymentNumber("2234");
			personRepository.Add(person);

			var tenantLogonDataManager = new FakeTenantLogonDataManager();
			tenantLogonDataManager.SetLogon(person.Id.Value, "2234", "2234");
			var target = new GetPersonsByEmploymentNumbersQueryHandler(new PersonCredentialsAppender(assembler, new TenantPeopleLoader(tenantLogonDataManager)), personRepository, new FakeCurrentUnitOfWorkFactory(null));

			var result = target.Handle(new GetPersonsByEmploymentNumbersQueryDto()
			{
				EmploymentNumbers = new[] { "2234" }
			});

			result.Count.Should().Be.EqualTo(1);
			result.First().ApplicationLogOnName.Should().Be.EqualTo("2234");
			result.First().Identity.Should().Be.EqualTo("2234");
		}
	}
}