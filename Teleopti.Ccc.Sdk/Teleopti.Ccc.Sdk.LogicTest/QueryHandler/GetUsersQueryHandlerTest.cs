using System.Linq;
using System.ServiceModel;
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
	public class GetUsersQueryHandlerTest
	{
		[Test]
		public void ShouldGetUsers()
		{
			var personRepository = new FakePersonRepositoryLegacy2();

			var fakeTenantLogonDataManager = new FakeTenantLogonDataManager();
			var assembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy(),
				new TenantPeopleLoader(fakeTenantLogonDataManager));

			var person = PersonFactory.CreatePerson().WithId();
			personRepository.Add(person);

			var target = new GetUsersQueryHandler(assembler, personRepository, new FakeCurrentUnitOfWorkFactory(new FakeStorage()), new FullPermission());
			var result = target.Handle(new GetUsersQueryDto { LoadDeleted = false});

			result.Count.Should().Be.EqualTo(1);
			result.First().Name.Should().Be.EqualTo(person.Name.ToString());
		}

		[Test]
		public void ShouldNotGetUsersWhenNotPermitted()
		{
			var personRepository = new FakePersonRepositoryLegacy2();

			var fakeTenantLogonDataManager = new FakeTenantLogonDataManager();
			var assembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy(),
				new TenantPeopleLoader(fakeTenantLogonDataManager));

			var person = PersonFactory.CreatePerson().WithId();
			personRepository.Add(person);

			var target = new GetUsersQueryHandler(assembler, personRepository, new FakeCurrentUnitOfWorkFactory(new FakeStorage()), new NoPermission());
			Assert.Throws<FaultException>(()=> target.Handle(new GetUsersQueryDto { LoadDeleted = false }));
		}
	}
}