using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Audit;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;


namespace Teleopti.Ccc.InfrastructureTest.Auditing
{
	[TestFixture]
	[UnitOfWorkTest]
	[AllTogglesOn]
	public class StaffingContextPurgeServiceTest : IIsolateSystem
	{
		public IStaffingAuditRepository StaffingAuditRepository;
		public ISkillCombinationResourceRepository SkillCombinationResourceRepository;
		public IPersonRepository PersonRepository;
		public ICurrentUnitOfWork CurrentUnitOfWork;
		public ISkillRepository SkillRepository;
		public ISkillTypeRepository SkillTypeRepository;
		public IActivityRepository ActivityRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeUserCulture UserCulture;
		public IPurgeSettingRepository PurgeSettingRepository;
		public MutableNow Now;
		public StaffingContextPurgeService Target;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeUserCulture>().For<IUserCulture>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
			isolate.UseTestDouble<StaffingContextPurgeService>().For<StaffingContextPurgeService>();
			
		}

		
		[Test]
		public void ShouldPurgeAccordingToDefaultSetting()
		{
			Now.Is(DateTime.UtcNow);
			var person = PersonFactory.CreatePerson();
			PersonRepository.Add(person);

			var staffingAudit1 =
				new StaffingAudit(person, StaffingAuditActionConstants.ImportStaffing, "BPO", "abc.txt", "")
					{ TimeStamp = DateTime.UtcNow };
			var staffingAudit2 =
				new StaffingAudit(person, StaffingAuditActionConstants.ImportStaffing, "BPO", "abc.txt", "")
					{ TimeStamp = DateTime.UtcNow.AddMonths(-4) };

			StaffingAuditRepository.Add(staffingAudit1);
			StaffingAuditRepository.Add(staffingAudit2);
			CurrentUnitOfWork.Current().PersistAll();

			Target.PurgeAudits();
			CurrentUnitOfWork.Current().PersistAll();
			var loadedAudits = StaffingAuditRepository.LoadAudits(person, DateTime.Now.AddDays(-100), DateTime.Now);
			loadedAudits.Count().Should().Be(1);
			loadedAudits.FirstOrDefault().TimeStamp.Should().Be.EqualTo(staffingAudit1.TimeStamp);
		}

	}
}