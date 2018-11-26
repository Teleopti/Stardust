using System;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Audit;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Auditing
{
	[TestFixture]
	[UnitOfWorkTest]
	[AllTogglesOn]
	public class PersonAccessContextReaderServiceTest
	{
		public IPersonAccessAuditRepository PersonAccessAuditRepository;
		public PersonAccessContextReaderService Target;
		public IApplicationRoleRepository ApplicationRoleRepository;
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public IPersonRepository PersonRepository;
		public MutableNow Now;

		[Test]
		public void ShouldLoadPersonAccessAuditContext()
		{
			var appRole = ApplicationRoleFactory.CreateRole("Superman", "The man");
			ApplicationRoleRepository.Add(appRole);
			dynamic role = new {RoleId = appRole.Id.GetValueOrDefault(), Name = appRole.Name};
			var person = PersonFactory.CreatePerson();
			PersonRepository.Add(person);
			CurrentUnitOfWork.Current().PersistAll();

			PersonAccessAuditRepository.Add(
				new PersonAccess(person, 
					person, 
					PersonAuditActionType.GrantRole.ToString(), 
					PersonAuditActionResult.Change.ToString(),
					JsonConvert.SerializeObject(role)));
			CurrentUnitOfWork.Current().PersistAll();
			var model = Target.LoadAudits(person, DateTime.Now.AddDays(-100), DateTime.Now).Single();

			model.Data.Should().Be.EqualTo("Person: arne arne, Role: Superman");
		}
	}
}