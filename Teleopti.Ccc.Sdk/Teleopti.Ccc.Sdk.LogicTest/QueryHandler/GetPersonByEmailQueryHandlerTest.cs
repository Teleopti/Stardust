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
	public class GetPersonByEmailQueryHandlerTest
	{
		[Test]
		public void ShouldGetPeopleByEmail()
		{
			var personRepository = new FakePersonRepositoryLegacy();
			
			var assembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy());
			
			var person = PersonFactory.CreatePerson().WithId();
			person.Email = "a@b.com";
			personRepository.Add(person);

			var dataManager = new FakeTenantLogonDataManager();
			dataManager.SetLogon(person.Id.Value,"aaa","");

			var target = new GetPersonByEmailQueryHandler(new PersonCredentialsAppender(assembler, new TenantPeopleLoader(dataManager)), personRepository, new FakeCurrentUnitOfWorkFactory(null));

			var result = target.Handle(new GetPersonByEmailQueryDto
			{
				Email = "a@b.com"
			});

			result.Count.Should().Be.EqualTo(1);
			result.First().Name.Should().Be.EqualTo(person.Name.ToString());
			result.First().ApplicationLogOnName.Should().Be.EqualTo("aaa");
		}
	}
}