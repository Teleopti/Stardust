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
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Staffing
{
	[TestFixture]
	[UnitOfWorkTest]
	[AllTogglesOn]
	public class StaffingContextReaderServiceTest : IIsolateSystem
	{
		public IStaffingAuditRepository StaffingAuditRepository;
		public IStaffingContextReaderService Target;
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
			StaffingAuditRepository.Add(new StaffingAudit(person, StaffingAuditActionConstants.ImportBpo,  "BPO", "abc.txt"));
			CurrentUnitOfWork.Current().PersistAll();
			Target.LoadAll().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReturnStaffingAuditOnClearBpoAction()
		{
			UserCulture.IsSwedish();

			var person = PersonFactory.CreatePerson();
			PersonRepository.Add(person);
			var loggedOnUser = PersonFactory.CreatePersonWithGuid("Ashley", "Aaron");
			LoggedOnUser.SetFakeLoggedOnUser(loggedOnUser);
			var expectedResult = "BPO name: telia" + Environment.NewLine + "Period from 2018-10-01 to 2019-10-01";
			var bpoGuid = addBpo("telia");
			var clearBPOAction = new ClearBpoActionObj
			{
				BpoGuid = bpoGuid,
				StartDate = new DateTime(2018, 10, 01, 0, 0, 0, DateTimeKind.Utc),
				EndDate = new DateTime(2019, 10, 01, 0, 0, 0, DateTimeKind.Utc)
			};
			StaffingAuditRepository.Add(
				new StaffingAudit(person, StaffingAuditActionConstants.ClearBpo,  "BPO", "",
					clearBPOAction.BpoGuid,clearBPOAction.StartDate,clearBPOAction.EndDate));
			CurrentUnitOfWork.Current().PersistAll();

			Target.LoadAll().FirstOrDefault().Data.Should().Be.EqualTo(expectedResult);
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
			UserCulture.IsSwedish();

			var person = PersonFactory.CreatePerson();
			PersonRepository.Add(person);
			var loggedOnUser = PersonFactory.CreatePersonWithGuid("Ashley", "Aaron");
			LoggedOnUser.SetFakeLoggedOnUser(loggedOnUser);
			var staffingAudit =
				new StaffingAudit(person, StaffingAuditActionConstants.ImportBpo, "BPO", "abc.txt") {TimeStamp = DateTime.UtcNow};
			StaffingAuditRepository.Add(staffingAudit);
			CurrentUnitOfWork.Current().PersistAll();

			var list = Target.LoadAll().ToList();

			list.FirstOrDefault().TimeStamp.Should().Be.EqualTo(staffingAudit.TimeStamp);
			list.FirstOrDefault().Action.Should().Be.EqualTo(StaffingAuditActionConstants.ImportBpo);
			list.FirstOrDefault().ActionPerformedBy.Should().Be.EqualTo(person.Name.ToString(NameOrderOption.FirstNameLastName));
			list.FirstOrDefault().Context.Should().Be.EqualTo("Staffing");
		}

		[Test]
		public void ShouldPurgeAccordingToSetting()
		{
			Now.Is(DateTime.UtcNow);
			var person = PersonFactory.CreatePerson();
			PersonRepository.Add(person);
			var loggedOnUser = PersonFactory.CreatePersonWithGuid("Ashley", "Aaron");
			LoggedOnUser.SetFakeLoggedOnUser(loggedOnUser);
			var staffingAudit =
				new StaffingAudit(person, StaffingAuditActionConstants.ImportBpo, "BPO", "abc.txt") { TimeStamp = DateTime.UtcNow };
			StaffingAuditRepository.Add(staffingAudit);
			CurrentUnitOfWork.Current().PersistAll();
			var staffingAudit2 =
				new StaffingAudit(person, StaffingAuditActionConstants.ImportBpo, "BPO", "abc.txt") { TimeStamp = DateTime.UtcNow.AddMonths(-4) };
			StaffingAuditRepository.Add(staffingAudit);
			StaffingAuditRepository.Add(staffingAudit2);
			CurrentUnitOfWork.Current().PersistAll();

			Target.PurgeAudits();
			CurrentUnitOfWork.Current().PersistAll();
			Target.LoadAll().Count().Should().Be(1);
		}

		private void addPurgeSetting(int purgeMonthValue)
		{
			using (var uow = CurrentUnitOfWork.Current())
			{
				var session = uow.FetchSession();
				session.CreateSQLQuery(
						$"update dbo.purgeSetting set [Value] = {purgeMonthValue} where [key]='MonthsToKeepAudit'")
					.ExecuteUpdate();
				uow.PersistAll();
			}
		}
	}
}
