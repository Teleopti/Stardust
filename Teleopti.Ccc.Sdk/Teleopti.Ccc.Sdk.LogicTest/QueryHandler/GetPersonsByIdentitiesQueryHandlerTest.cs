using System.Collections.Generic;
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
	public class GetPersonsByIdentitiesQueryHandlerTest
	{
		[Test]
		public void ShouldGetPeopleByIdentity()
		{
			var personRepository = new FakePersonRepositoryLegacy();

			var fakeTenantLogonDataManager = new FakeTenantLogonDataManager();
			var assembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy());

			var person = PersonFactory.CreatePerson().WithId();
			personRepository.Add(person);
			fakeTenantLogonDataManager.SetLogon(person.Id.Value,"","my@identi.ty");

			var target = new GetPersonsByIdentitiesQueryHandler(new PersonCredentialsAppender(assembler, new TenantPeopleLoader(fakeTenantLogonDataManager)), personRepository, new FakeCurrentUnitOfWorkFactory(null), fakeTenantLogonDataManager);

			var result = target.Handle(new GetPersonsByIdentitiesQueryDto
			{
				Identities = new List<string> { "my@identi.ty" } 
			});

			result.Count.Should().Be.EqualTo(1);
			result.First().Name.Should().Be.EqualTo(person.Name.ToString());
		}

		[Test]
		public void ShouldGetPeopleByIdentityIncludingIdentity()
		{
			var personRepository = new FakePersonRepositoryLegacy();

			var fakeTenantLogonDataManager = new FakeTenantLogonDataManager();
			var assembler = new PersonAssembler(personRepository,
				new WorkflowControlSetAssembler(new ShiftCategoryAssembler(new FakeShiftCategoryRepository()),
					new DayOffAssembler(new FakeDayOffTemplateRepository()), new ActivityAssembler(new FakeActivityRepository()),
					new AbsenceAssembler(new FakeAbsenceRepository())), new PersonAccountUpdaterDummy());

			var person = PersonFactory.CreatePerson().WithId();
			personRepository.Add(person);
			fakeTenantLogonDataManager.SetLogon(person.Id.Value, "", "my@identi.ty");

			var target = new GetPersonsByIdentitiesQueryHandler(new PersonCredentialsAppender(assembler, new TenantPeopleLoader(fakeTenantLogonDataManager)), personRepository, new FakeCurrentUnitOfWorkFactory(null), fakeTenantLogonDataManager);

			var result = target.Handle(new GetPersonsByIdentitiesQueryDto
			{
				Identities = new List<string> { "my@identi.ty" }
			});

			result.Count.Should().Be.EqualTo(1);
			result.First().Identity.Should().Be.EqualTo("my@identi.ty");
		}
	}
}