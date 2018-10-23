using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
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
			StaffingAuditRepository.Add(new StaffingAudit(person, StaffingAuditActionConstants.ImportBpo, "abc.txt", "BPO"){TimeStamp = new DateTime(2018, 10, 14,10,0,0,DateTimeKind.Utc) });
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

	}
}