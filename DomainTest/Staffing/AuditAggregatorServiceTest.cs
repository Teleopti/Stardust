﻿using System;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Staffing
{
	[DomainTest]
	[AllTogglesOn]
	public class AuditAggregatorServiceTest : IIsolateSystem
	{
		public IStaffingAuditRepository StaffingAuditRepository;
		public IPersonAccessAuditRepository PersonAccessAuditRepository;
		public IApplicationRoleRepository ApplicationRoleRepository;
		public AuditAggregatorService Target;
		public ISkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeLoggedOnUser LoggedOnUser;
		
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<AuditAggregatorService>().For<AuditAggregatorService>();
		}

		[Test]
		public void ShouldReturnStaffingAuditOnImportBpoAction()
		{
			var person = PersonFactory.CreatePersonWithId();
			StaffingAuditRepository.Add(new StaffingAudit(person, StaffingAuditActionConstants.ImportBpo,  "BPO", "abc.txt") {TimeStamp = new DateTime(2018, 10, 14,10,0,0,DateTimeKind.Utc) });
			var startDate = new DateTime(2018, 10, 13);
			var endDate = new DateTime(2018,10,15);
			var audits =  Target.Load(person.Id.GetValueOrDefault(), startDate,endDate);
			audits.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnLogsInCurrentUserTimezone()
		{
			var stockholmUser = PersonFactory.CreatePerson(TimeZoneInfoFactory.StockholmTimeZoneInfo()).WithId();
			var stockholmTime = new DateTime(2018, 10, 14, 16, 0, 0);
			LoggedOnUser.SetFakeLoggedOnUser(stockholmUser);
			
			var singaporeUser = PersonFactory.CreatePerson().WithId();

			StaffingAuditRepository.Add(new StaffingAudit(singaporeUser, StaffingAuditActionConstants.ImportBpo, "BPO", "abc.txt") { TimeStamp = new DateTime(2018, 10, 14, 14, 0, 0, DateTimeKind.Utc) });
			var startDate = new DateTime(2018, 10, 13);
			var endDate = new DateTime(2018, 10, 15);
			var audits = Target.Load(singaporeUser.Id.GetValueOrDefault(), startDate, endDate);
			audits.Count.Should().Be.EqualTo(1);
			audits.First().TimeStamp.Should().Be.EqualTo(stockholmTime);
		}

		[Test]
		public void ShouldReturnStaffingAuditAndPersonAccessAudit()
		{
			var person = PersonFactory.CreatePersonWithId();
			var staffingAudit = new StaffingAudit(person, StaffingAuditActionConstants.ImportBpo, "abc.txt", "BPO")
				{TimeStamp = new DateTime(2018, 10, 14, 10, 0, 0, DateTimeKind.Utc)};
			IApplicationRole role = new ApplicationRole(){Name = "Name" };
			role.SetId(Guid.NewGuid());
			dynamic role2 = new {RoleId = role.Id, Name = "Name"};
			var personAccessAudit = new PersonAccess(
				person,
				person,
				PersonAuditActionType.GrantRole.ToString(),
				PersonAuditActionResult.Change.ToString(),
				JsonConvert.SerializeObject(role2))
			{ TimeStamp = new DateTime(2018, 10, 14, 10, 0, 0, DateTimeKind.Utc) };

			StaffingAuditRepository.Add(staffingAudit);
			PersonAccessAuditRepository.Add(personAccessAudit);
			ApplicationRoleRepository.Add(role);

			var startDate = new DateTime(2018, 10, 13);
			var endDate = new DateTime(2018, 10, 15);
			var audits = Target.Load(person.Id.GetValueOrDefault(), startDate, endDate);
			audits.Count.Should().Be.EqualTo(2);
			audits.Count(f => f.Action == staffingAudit.Action).Should().Be(1);
			audits.Count(f => f.Action == personAccessAudit.Action).Should().Be(1);
		}

		[Test]
		public void ShouldPurgeStaffingAuditAndPersonAccessAudit()
		{
			var person = PersonFactory.CreatePersonWithId();
			var staffingAudit = createStaffingAudit(person);
			var staffingAuditOld = createStaffingAudit(person, DateTime.UtcNow.AddMonths(-5));
			IApplicationRole role = new ApplicationRole { Name = "Name" };
			var personAccessAudit = createPersonAccess(person, role);
			var personAccessAuditOld = createPersonAccess(person, role, DateTime.UtcNow.AddMonths(-4));

			StaffingAuditRepository.Add(staffingAudit);
			StaffingAuditRepository.Add(staffingAuditOld);
			PersonAccessAuditRepository.Add(personAccessAudit);
			PersonAccessAuditRepository.Add(personAccessAuditOld);
			ApplicationRoleRepository.Add(role);

			Target.PurgeOldAudits();

			var result = Target.Load(person.Id.GetValueOrDefault(), DateTime.MinValue, DateTime.MaxValue);

			result.Count.Should().Be(2);
		}

		private static PersonAccess createPersonAccess(IPerson person = null, IApplicationRole role = null, DateTime? timeStamp = null)
		{
			if(person == null)
				 person = PersonFactory.CreatePersonWithId();

			if(role == null)
				role = new ApplicationRole { Name = "Name" };

			dynamic role2 = new { RoleId = role.Id, Name = "Name" };

			if (timeStamp == null)
				timeStamp = DateTime.UtcNow;

			var personAccessAudit = new PersonAccess(
					person,
					person,
					PersonAuditActionType.GrantRole.ToString(),
					PersonAuditActionResult.Change.ToString(),
					JsonConvert.SerializeObject(role2))
				{ TimeStamp = timeStamp.GetValueOrDefault() };

			return personAccessAudit;
		}

		private static StaffingAudit createStaffingAudit(IPerson person = null, DateTime? timeStamp = null)
		{
			if (person == null)
				person = PersonFactory.CreatePersonWithId();

			if(timeStamp == null)
				timeStamp = DateTime.UtcNow;
			var staffingAudit = new StaffingAudit(person, StaffingAuditActionConstants.ImportBpo, "abc.txt", "BPO")
				{ TimeStamp = timeStamp.GetValueOrDefault() };

			return staffingAudit;
		}

	}
}
