using System;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
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
		public AuditAggregatorService Target;
		public ISkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeApplicationRoleRepository ApplicationRoleRepository;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<AuditAggregatorService>().For<AuditAggregatorService>();
		}

		[Test]
		public void ShouldReturnStaffingAuditOnImportBpoAction()
		{
			var person = PersonFactory.CreatePersonWithId();
			StaffingAuditRepository.Add(createStaffingAudit(person));

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

			StaffingAuditRepository.Add(new StaffingAudit(singaporeUser, StaffingAuditActionConstants.ImportBpo, "abc.txt", "BPO") { TimeStamp = new DateTime(2018, 10, 14, 14, 0, 0, DateTimeKind.Utc) });
			var startDate = new DateTime(2018, 10, 13);
			var endDate = new DateTime(2018, 10, 15);
			var audits = Target.Load(singaporeUser.Id.GetValueOrDefault(), startDate, endDate);
			audits.Count.Should().Be.EqualTo(1);
			audits.First().TimeStamp.Should().Be.EqualTo(stockholmTime);
		}


		[Test]
		public void ShouldReturnPersonAccessAudit()
		{
			var person = PersonFactory.CreatePersonWithId();
			
			var personAccess = createPersonAccess(person);
			personAccess.TimeStamp = new DateTime(2018, 10, 23, 16, 0, 0);
			PersonAccessAuditRepository.Add(personAccess);

			var startDate = new DateTime(2018, 10, 22);
			var endDate = new DateTime(2018, 10, 24);
			var audits = Target.Load(person.Id.GetValueOrDefault(), startDate, endDate);
			audits.Count.Should().Be.EqualTo(1);
		}


		[Test]
		public void ShouldReturnPersonAccessAndStaffingAudits()
		{
			var person = PersonFactory.CreatePersonWithId();
			var personAccess = createPersonAccess(person);
			var staffingAudit = createStaffingAudit(person);

			staffingAudit.TimeStamp  = new DateTime(2018, 10, 23, 16, 0, 0);
			StaffingAuditRepository.Add(staffingAudit);
			
			personAccess.TimeStamp = new DateTime(2018, 10, 23, 16, 0, 0);
			PersonAccessAuditRepository.Add(personAccess);

			var startDate = new DateTime(2018, 10, 22);
			var endDate = new DateTime(2018, 10, 24);
			var audits = Target.Load(person.Id.GetValueOrDefault(), startDate, endDate);
			audits.Count.Should().Be.EqualTo(2);
		}

		private PersonAccess createPersonAccess(IPerson person)
		{
			var personOn = PersonFactory.CreatePersonWithId();
			var appRole = ApplicationRoleFactory.CreateRole("Superman", "The man").WithId();
			ApplicationRoleRepository.Add(appRole);
			dynamic role = new {RoleId = appRole.Id.GetValueOrDefault(), Name = appRole.DescriptionText};
			var personAccess = new PersonAccess(person, personOn, PersonAuditActionType.MultiGrantRole.ToString(),
				PersonAuditActionResult.Change.ToString(), JsonConvert.SerializeObject(role));
			return personAccess;
		}

		private IStaffingAudit createStaffingAudit(IPerson person)
		{
			return new StaffingAudit(person, StaffingAuditActionConstants.ImportBpo, "abc.txt", "BPO")
			{
				TimeStamp = new DateTime(2018, 10, 14, 10, 0, 0, DateTimeKind.Utc)
			};
		}
	}
}