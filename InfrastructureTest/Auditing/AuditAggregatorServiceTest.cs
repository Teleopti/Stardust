using System;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Auditing;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Audit;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


namespace Teleopti.Ccc.InfrastructureTest.Auditing
{
	[UnitOfWorkTest]
	[AllTogglesOn]
	public class AuditAggregatorServiceTest : IIsolateSystem
	{
		public IStaffingAuditRepository StaffingAuditRepository;
		public IPersonAccessAuditRepository PersonAccessAuditRepository;
		public IApplicationRoleRepository ApplicationRoleRepository;
		public AuditAggregatorService Target;
		public ISkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeUserCulture UserCulture;
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public IPersonRepository PersonRepository;
		public ResultCountOfAggregatedAudits ResultCountOfAggregatedAudits;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<FakeUserCulture>().For<IUserCulture>();
			isolate.UseTestDouble<AuditAggregatorService>().For<AuditAggregatorService>();
			isolate.UseTestDouble<ResultCountOfAggregatedAudits>().For<ResultCountOfAggregatedAudits>();

		}

		[Test]
		public void ShouldReturnStaffingAuditOnImportBpoAction()
		{
			var person = PersonFactory.CreatePersonWithId();
			StaffingAuditRepository.Add(new StaffingAudit(person, StaffingAuditActionConstants.ImportStaffing,  "BPO", "abc.txt", "") {TimeStamp = new DateTime(2018, 10, 14, 10, 0, 0,DateTimeKind.Utc) });
			var startDate = new DateTime(2018, 10, 13);
			var endDate = new DateTime(2018, 10, 15);
			CurrentUnitOfWork.Current().PersistAll();
			var audits = Target.Load(person.Id.GetValueOrDefault(), startDate, endDate);
			audits.Count.Should().Be.EqualTo(1);
		}

		[Test]
		public void ShouldReturnLogsInCurrentUserTimezone()
		{
			var stockholmUser = PersonFactory.CreatePerson(TimeZoneInfoFactory.StockholmTimeZoneInfo()).WithId();
			var stockholmTime = new DateTime(2018, 10, 14, 16, 0, 0);
			LoggedOnUser.SetFakeLoggedOnUser(stockholmUser);

			var singaporeUser = PersonFactory.CreatePerson().WithId();

			StaffingAuditRepository.Add(new StaffingAudit(singaporeUser, StaffingAuditActionConstants.ImportStaffing, "BPO", "abc.txt", "") { TimeStamp = new DateTime(2018, 10, 14, 14, 0, 0, DateTimeKind.Utc) });
			var startDate = new DateTime(2018, 10, 13);
			var endDate = new DateTime(2018, 10, 15);
			CurrentUnitOfWork.Current().PersistAll();
			var audits = Target.Load(singaporeUser.Id.GetValueOrDefault(), startDate, endDate);
			audits.Count.Should().Be.EqualTo(1);
			audits.First().TimeStamp.Should().Be.EqualTo(stockholmTime);
		}

		[Test]
		public void ShouldReturnStaffingAuditAndPersonAccessAudit()
		{
			UserCulture.IsSwedish();
			var person = PersonFactory.CreatePerson();
			PersonRepository.Add(person);
			CurrentUnitOfWork.Current().PersistAll();
			var staffingAudit = new StaffingAudit(person, StaffingAuditActionConstants.ImportStaffing, "abc.txt", "BPO", "")
				{ TimeStamp = new DateTime(2018, 10, 14, 10, 0, 0, DateTimeKind.Utc) };
			IApplicationRole role = new ApplicationRole() { Name = "Name" };
			ApplicationRoleRepository.Add(role);
			CurrentUnitOfWork.Current().PersistAll();
			dynamic role2 = new { RoleId = role.Id, Name = "Name" };
			var personAccessAudit = new PersonAccess(
					person,
					person,
					PersonAuditActionType.GrantRole.ToString(),
					PersonAuditActionResult.Change.ToString(),
					JsonConvert.SerializeObject(role2))
				{ TimeStamp = new DateTime(2018, 10, 14, 10, 0, 0, DateTimeKind.Utc) };
			
			StaffingAuditRepository.Add(staffingAudit);
			PersonAccessAuditRepository.Add(personAccessAudit);


			var startDate = new DateTime(2018, 10, 13);
			var endDate = new DateTime(2018, 10, 15);
			CurrentUnitOfWork.Current().PersistAll();
			var audits = Target.Load(person.Id.GetValueOrDefault(), startDate, endDate);
			audits.Count.Should().Be.EqualTo(2);
			audits.Count(f => f.Action == Resources.ResourceManager.GetString(staffingAudit.Action, UserCulture.GetCulture())).Should().Be(1);
			audits.Count(f => f.Action == Resources.ResourceManager.GetString(personAccessAudit.Action, UserCulture.GetCulture())).Should().Be(1);
		}

		[Test]
		public void ShouldReturnResultInSortedOrder()
		{
			var now = new DateTime(2018, 10, 14, 14, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson();
			PersonRepository.Add(person);
			CurrentUnitOfWork.Current().PersistAll();

			insertSomeStaffingAudits(now.AddDays(1), person, 1);
			insertSomeStaffingAudits(now.AddDays(2), person, 1);
			insertSomePersonAccessAudits(now.AddDays(-1), person, 1);
			insertSomePersonAccessAudits(now.AddDays(3), person, 1);
			CurrentUnitOfWork.Current().PersistAll();

			var audits = Target.Load(person.Id.GetValueOrDefault(), now.AddDays(-5), now.AddDays(+5));
			audits.Count.Should().Be(4);
			audits[0].TimeStamp.Should().Be(now.AddDays(3));
			audits[1].TimeStamp.Should().Be(now.AddDays(2));
			audits[2].TimeStamp.Should().Be(now.AddDays(1));
			audits[3].TimeStamp.Should().Be(now.AddDays(-1));
		}


		[Test]
		public void ShouldReturnOnlyFirst100Records()
		{
			var now = new DateTime(2018, 10, 14, 14, 0, 0, DateTimeKind.Utc);
			var person = PersonFactory.CreatePerson();
			PersonRepository.Add(person);
			CurrentUnitOfWork.Current().PersistAll();

			insertSomeStaffingAudits(now.AddDays(-10), person,10);
			insertSomeStaffingAudits(now.AddDays(1), person,50);
			insertSomePersonAccessAudits(now.AddDays(-10), person,10);
			insertSomePersonAccessAudits(now.AddDays(1), person,50);
			CurrentUnitOfWork.Current().PersistAll();

			var audits = Target.Load(person.Id.GetValueOrDefault(), now.AddDays(-20), now.AddDays(+20));
			audits.Count.Should().Be.EqualTo(ResultCountOfAggregatedAudits.Limit);
			audits.Any(x=>x.TimeStamp==now.AddDays(-10)).Should().Be.False();

		}

		private void insertSomePersonAccessAudits(DateTime now, IPerson person,int count)
		{
			var appRole = ApplicationRoleFactory.CreateRole("Superman", "The man");
			ApplicationRoleRepository.Add(appRole);
			dynamic role = new { RoleId = appRole.Id.GetValueOrDefault(), Name = appRole.DescriptionText };
			
			foreach (var i in Enumerable.Range(0, count))
			{
				var personAccess = new PersonAccess(person,
					person,
					PersonAuditActionType.GrantRole.ToString(),
					PersonAuditActionResult.Change.ToString(),
					JsonConvert.SerializeObject(role));
				personAccess.TimeStamp = now;
				PersonAccessAuditRepository.Add(personAccess);
			}
			
		}

		private void insertSomeStaffingAudits(DateTime now, IPerson person, int count)
		{
			foreach (var i in Enumerable.Range(0, count))
			{
				StaffingAuditRepository.Add(new StaffingAudit(person, StaffingAuditActionConstants.ImportStaffing, "BPO", "abc.txt","") { TimeStamp = now });
			}
			
		}
	}
}
