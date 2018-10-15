using System;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.Authentication;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Staffing
{
	[TestFixture]
	[DomainTest]
	[AllTogglesOn]
	public class StaffingContextAuditServiceTest : IIsolateSystem
	{
		public IStaffingAuditRepository StaffingAuditRepository;
		public StaffingContextAuditService Target;
		public ISkillCombinationResourceRepository SkillCombinationResourceRepository;
		public FakeLoggedOnUser LoggedOnUser;
		public FakeUserCulture UserCulture;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble<FakeUserCulture>().For<IUserCulture>();
			isolate.UseTestDouble<FakeLoggedOnUser>().For<ILoggedOnUser>();
		}

		[Test]
		public void ShouldLoadStaffingAuditContext()
		{
			StaffingAuditRepository.Add(new StaffingAudit(PersonFactory.CreatePersonWithId(), StaffingAuditActionConstants.ImportBPO, "", "BPO"));
			Target.LoadAll().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReturnStaffingAuditOnClearBPOAction()
		{
			UserCulture.IsSwedish();

			var person = PersonFactory.CreatePersonWithId();
			var loggedOnUser = PersonFactory.CreatePersonWithGuid("Ashley", "Aaron");
			LoggedOnUser.SetFakeLoggedOnUser(loggedOnUser);
			var expectedResult = "BPO name: telia" + Environment.NewLine + "Period from 2018-10-01 to 2019-10-01";
			var activeBPOModel = new ActiveBpoModel() {Id = Guid.NewGuid(), Source = "telia"};
			((FakeSkillCombinationResourceRepository)SkillCombinationResourceRepository).ActiveBpos.Add(activeBPOModel);
			var clearBPOAction = new ClearBpoActionObj();
			clearBPOAction.BpoGuid = activeBPOModel.Id;
			clearBPOAction.StartDate = new DateTime(2018,10,01);
			clearBPOAction.EndDate = new DateTime(2019,10,01);
			var data = JsonConvert.SerializeObject(clearBPOAction);
			StaffingAuditRepository.Add(new StaffingAudit(person, StaffingAuditActionConstants.ClearBPO, data, "BPO"));

			Target.LoadAll().FirstOrDefault().Data.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnStaffingAuditOnImportBpoAction()
		{
			var expectedResult = "File name: abc.txt";
			StaffingAuditRepository.Add(new StaffingAudit(PersonFactory.CreatePersonWithId(), StaffingAuditActionConstants.ImportBPO, "abc.txt", "BPO"));
			Target.LoadAll().FirstOrDefault().Data.Should().Be.EqualTo(expectedResult);
		}

		//[Test]
		//public void ShouldReturnStaffingAuditForTheGivenPeriod()
		//{

		//	var person = PersonFactory.CreatePersonWithId();
		//	var staffingAudit1 = new StaffingAudit(person, StaffingAuditActionConstants.ImportBPO, "", "BPO"){TimeStamp = new DateTime(2018, 10, 01, 08, 08, 08) };
		//	var staffingAudit2 = new StaffingAudit(person, StaffingAuditActionConstants.ImportBPO, "", "BPO"){TimeStamp = new DateTime(2018, 10, 02, 08, 08, 08) };
		//	var staffingAudit3 = new StaffingAudit(person, StaffingAuditActionConstants.ImportBPO, "", "BPO"){TimeStamp = new DateTime(2018, 10, 03, 08, 08, 08) };
		//	StaffingAuditRepository.Add(staffingAudit1);
		//	StaffingAuditRepository.Add(staffingAudit2);
		//	StaffingAuditRepository.Add(staffingAudit3);
		//	Target.LoadAuditsForPeriod(new DateTime(2018, 10, 01), new DateTime(2018, 10, 02)).Count().Should().Be
		//		.EqualTo(2);
		//}

		[Test]
		public void ShouldReturnStaffingAuditForTheGivenPerson()
		{
			var person1 = PersonFactory.CreatePersonWithId();
			var person2 = PersonFactory.CreatePersonWithId();
			//var personRelevant = PersonFactory.CreatePersonWithId();
			//var personIrellevant = PersonFactory.CreatePersonWithId();
			//var expectedResult = "File name: abc.txt";
			//StaffingAuditRepository.Add(new StaffingAudit(PersonFactory.CreatePersonWithId(), StaffingAuditActionConstants.ImportBPO, "abc.txt", "BPO"));
			//Target.LoadAll().FirstOrDefault().Data.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnStaffingAuditForTheGivenParameters()
		{
			var person = PersonFactory.CreatePersonWithId();
			var staffingAudit = new StaffingAudit(person, StaffingAuditActionConstants.ImportBPO, "", "BPO");
			staffingAudit.TimeStamp = DateTime.UtcNow;			
			StaffingAuditRepository.Add(staffingAudit);
			var list = Target.LoadAll();

			list.FirstOrDefault().TimeStamp.Should().Be.EqualTo(staffingAudit.TimeStamp);
			list.FirstOrDefault().Action.Should().Be.EqualTo(StaffingAuditActionConstants.ImportBPO);
			list.FirstOrDefault().ActionPerformedBy.Should().Be.EqualTo(person);
			list.FirstOrDefault().Context.Should().Be.EqualTo("Staffing");
			
		}
	}
}