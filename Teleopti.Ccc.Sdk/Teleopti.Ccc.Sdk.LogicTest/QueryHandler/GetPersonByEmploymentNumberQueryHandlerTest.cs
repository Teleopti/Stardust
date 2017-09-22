using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Sdk.Common.DataTransferObject.QueryDtos;
using Teleopti.Ccc.Sdk.Logic.Assemblers;
using Teleopti.Ccc.Sdk.Logic.MultiTenancy;
using Teleopti.Ccc.Sdk.Logic.QueryHandler;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;

namespace Teleopti.Ccc.Sdk.LogicTest.QueryHandler
{
	[TestFixture]
	public class GetPersonByEmploymentNumberQueryHandlerTest
	{
		[Test]
		public void ShouldGetPeopleByEmploymentNumber()
		{
			var personRepository = new FakePersonRepositoryLegacy();

			var assembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy(),
				new TenantPeopleLoader(new FakeTenantLogonDataManager()));

			var person = PersonFactory.CreatePerson();
			person.SetEmploymentNumber("1234");
			personRepository.Add(person);

			var target = new GetPersonByEmploymentNumberQueryHandler(assembler, personRepository, new FakeCurrentUnitOfWorkFactory());

			var result = target.Handle(new GetPersonByEmploymentNumberQueryDto
			{
				EmploymentNumber = "1234"
			});

			result.Count.Should().Be.EqualTo(1);
			result.First().Name.Should().Be.EqualTo(person.Name.ToString());
		}
	}
}