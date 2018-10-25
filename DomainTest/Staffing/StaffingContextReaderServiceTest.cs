using System;
using System.Linq;
using Newtonsoft.Json;
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
	[TestFixture]
	[DomainTest]
	[AllTogglesOn]
	public class StaffingContextReaderServiceTest : IIsolateSystem
	{
		public IStaffingAuditRepository StaffingAuditRepository;
		public StaffingContextReaderService Target;
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
			StaffingAuditRepository.Add(new StaffingAudit(PersonFactory.CreatePersonWithId(), StaffingAuditActionConstants.ImportBpo, "", "BPO"));
			Target.LoadAll().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReturnStaffingAuditOnClearBpoAction()
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
			StaffingAuditRepository.Add(new StaffingAudit(person, StaffingAuditActionConstants.ClearBpo, data, "BPO"));

			Target.LoadAll().FirstOrDefault().Data.Should().Be.EqualTo(expectedResult);
		}

		[Test]
		public void ShouldReturnStaffingAuditOnImportBpoAction()
		{
			var expectedResult = "File name: abc.txt";
			StaffingAuditRepository.Add(new StaffingAudit(PersonFactory.CreatePersonWithId(), StaffingAuditActionConstants.ImportBpo, "abc.txt", "BPO"));
			Target.LoadAll().FirstOrDefault().Data.Should().Be.EqualTo(expectedResult);
		}

		
		[Test]
		public void ShouldReturnStaffingAuditForTheGivenParameters()
		{
			var person = PersonFactory.CreatePersonWithId();
			var staffingAudit = new StaffingAudit(person, StaffingAuditActionConstants.ImportBpo, "", "BPO");
			staffingAudit.TimeStamp = DateTime.UtcNow;			
			StaffingAuditRepository.Add(staffingAudit);
			var list = Target.LoadAll();

			list.FirstOrDefault().TimeStamp.Should().Be.EqualTo(staffingAudit.TimeStamp);
			list.FirstOrDefault().Action.Should().Be.EqualTo(StaffingAuditActionConstants.ImportBpo);
			list.FirstOrDefault().ActionPerformedBy.Should().Be.EqualTo(person.Name.ToString(NameOrderOption.FirstNameLastName));
			list.FirstOrDefault().Context.Should().Be.EqualTo("Staffing");
		}
	}
}