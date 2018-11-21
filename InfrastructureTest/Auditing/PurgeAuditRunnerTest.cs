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
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Auditing
{
	[UnitOfWorkTest]
	public class PurgeAuditRunnerTest
	{
		public IPurgeAuditRunner Target;
		public IStaffingAuditRepository StaffingAuditRepository;
		public IPersonAccessAuditRepository PersonAccessAuditRepository;
		public IApplicationRoleRepository ApplicationRoleRepository;
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public IPersonRepository PersonRepository;
		public MutableNow Now;

		[Test]
		public void RunAudits()
		{
			Now.Is(DateTime.UtcNow);
			insertSomeStaffingAudits();
			insertSomePersonAccessAudits();

			Target.Run();
			CurrentUnitOfWork.Current().PersistAll();

			StaffingAuditRepository.LoadAll().Count().Should().Be(0);
			PersonAccessAuditRepository.LoadAll().Count().Should().Be(0);
		}

		private void insertSomePersonAccessAudits()
		{
			var appRole = ApplicationRoleFactory.CreateRole("Superman", "The man");
			ApplicationRoleRepository.Add(appRole);
			dynamic role = new { RoleId = appRole.Id.GetValueOrDefault(), Name = appRole.DescriptionText };
			var person = PersonFactory.CreatePerson();
			PersonRepository.Add(person);
			CurrentUnitOfWork.Current().PersistAll();
			var personAccess = new PersonAccess(person,
				person,
				PersonAuditActionType.GrantRole.ToString(),
				PersonAuditActionResult.Change.ToString(),
				JsonConvert.SerializeObject(role));
			personAccess.TimeStamp = new DateTime(2017, 6, 14, 14, 0, 0, DateTimeKind.Utc);
			PersonAccessAuditRepository.Add(personAccess);
			CurrentUnitOfWork.Current().PersistAll();
		}

		private void insertSomeStaffingAudits()
		{
			var singaporeUser = PersonFactory.CreatePerson().WithId();
			StaffingAuditRepository.Add(new StaffingAudit(singaporeUser, StaffingAuditActionConstants.ImportStaffing, "BPO", "abc.txt") { TimeStamp = new DateTime(2017, 6, 14, 14, 0, 0, DateTimeKind.Utc) });
			CurrentUnitOfWork.Current().PersistAll();
		}

		
	}

}