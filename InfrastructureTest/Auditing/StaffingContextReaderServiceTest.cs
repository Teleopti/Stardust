using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Audit;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.UserTexts;


// ReSharper disable PossibleNullReferenceException

namespace Teleopti.Ccc.InfrastructureTest.Auditing
{
	[TestFixture]
	[UnitOfWorkTest]
	[AllTogglesOn]
	public class StaffingContextReaderServiceTest : IIsolateSystem
	{
		public IStaffingAuditRepository StaffingAuditRepository;
		public StaffingContextReaderService Target;
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
		
		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeUserCulture>().For<IUserCulture>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}

		[Test]
		public void ShouldLoadStaffingAuditContext()
		{
			var person = PersonFactory.CreatePerson();
			PersonRepository.Add(person);
			StaffingAuditRepository.Add(new StaffingAudit(person, StaffingAuditActionConstants.ImportStaffing,  "BPO", "abc.txt", ""));
			CurrentUnitOfWork.Current().PersistAll();
			Target.LoadAudits(person, DateTime.Now.AddDays(-100), DateTime.Now).Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReturnStaffingAuditOnClearBpoAction()
		{
			UserCulture.IsSwedish();

			var person = PersonFactory.CreatePerson();
			PersonRepository.Add(person);
			var expectedResult = "BPO name: telia, Period between 2018-10-01 and 2019-10-01";
			var bpoGuid = addBpo("telia");
			var clearBpoAction = new ClearBpoActionObj
			{
				BpoGuid = bpoGuid,
				StartDate = new DateTime(2018, 10, 01, 0, 0, 0, DateTimeKind.Utc),
				EndDate = new DateTime(2019, 10, 01, 0, 0, 0, DateTimeKind.Utc)
			};
			var bpoName = SkillCombinationResourceRepository.GetSourceBpoByGuid(clearBpoAction.BpoGuid);
			StaffingAuditRepository.Add(
				new StaffingAudit(person, StaffingAuditActionConstants.ClearStaffing,  "BPO", "",
					bpoName, clearBpoAction.StartDate,clearBpoAction.EndDate));
			CurrentUnitOfWork.Current().PersistAll();

			Target.LoadAudits(person, DateTime.Now.AddDays(-100), DateTime.Now).FirstOrDefault().Data.Should().Be.EqualTo(expectedResult);
		}

		private Guid addBpo(string bpoName)
		{
			var skill = persistSkill();

			var combinationResources = new[]
			{
				new ImportSkillCombinationResourceBpo
				{
					StartDateTime = DateTime.UtcNow,
					EndDateTime = DateTime.UtcNow,
					Resources = 1,
					SkillIds = new List<Guid>{skill.Id.GetValueOrDefault()},
					Source=bpoName
				}
			};

			SkillCombinationResourceRepository.PersistSkillCombinationResourceBpo(combinationResources.ToList());
			CurrentUnitOfWork.Current().PersistAll();
			return SkillCombinationResourceRepository.LoadActiveBpos().First().Id;
		}

		private ISkill persistSkill()
		{
			var activity = new Activity("act");
			var skillType = SkillTypeFactory.CreateSkillType();
			var skill = new Skill("skill", "skill", Color.Blue, 15, skillType)
			{
				TimeZone = TimeZoneInfo.Utc,
				Activity = activity
			};
			SkillTypeRepository.Add(skillType);
			ActivityRepository.Add(activity);
			SkillRepository.Add(skill);
			CurrentUnitOfWork.Current().PersistAll();
			return skill;
		}
		
		[Test]
		public void ShouldReturnStaffingAuditForTheGivenParameters()
		{
			var loggedOnUser = PersonFactory.CreatePersonWithGuid("Ashley", "Aaron");
			LoggedOnUser.SetFakeLoggedOnUser(loggedOnUser);
			var staffingAudit =
				new StaffingAudit(loggedOnUser, StaffingAuditActionConstants.ImportStaffing, "BPO", "abc.txt", "") {TimeStamp = DateTime.UtcNow};
			StaffingAuditRepository.Add(staffingAudit);
			CurrentUnitOfWork.Current().PersistAll();

			var list = Target.LoadAudits(loggedOnUser, DateTime.Now.AddDays(-100), DateTime.Now).ToList();

			list.FirstOrDefault().TimeStamp.Should().Be.EqualTo(staffingAudit.TimeStamp);
			list.FirstOrDefault().Action.Should().Be.EqualTo(Resources.AuditTrailImportStaffing);
			list.FirstOrDefault().ActionPerformedBy.Should().Be.EqualTo("Ashley Aaron");
			list.FirstOrDefault().Context.Should().Be.EqualTo("Staffing");
		}

	}
}
