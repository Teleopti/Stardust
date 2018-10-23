using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Audit;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Repositories.Audit;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Repositories.Audit
{
	[TestFixture]
	[InfrastructureTest]
	public class StaffingAuditRepositoryTest 
	{
		public IStaffingAuditRepository Target;
		public WithUnitOfWork WithUnitOfWork;
		public IPersonRepository PersonRepository;
		
		[Test]
		public void ShouldBeAbleToSaveAuditData()
		{

			StaffingAudit staffingAudit = null;
			var person = PersonFactory.CreatePerson(new Name("ash", "and"));
			var datetime = new DateTime(2018,10,11,10,10,10,DateTimeKind.Utc);
			WithUnitOfWork.Do(() =>
			{
				PersonRepository.Add(person);
				staffingAudit = new StaffingAudit(person, "ClearBpoStaffing","This is the data", "BPO");
				staffingAudit.TimeStamp = datetime;
				Target.Add(staffingAudit);
			});

			WithUnitOfWork.Do(() =>
			{
				var loadedStaffingAudit =  Target.LoadAll().First();
				loadedStaffingAudit.Action.Should().Be.EqualTo(staffingAudit.Action);
				loadedStaffingAudit.ActionPerformedBy.Id.GetValueOrDefault().Should().Be.EqualTo(person.Id.GetValueOrDefault());
				loadedStaffingAudit.TimeStamp.Should().Be.EqualTo(staffingAudit.TimeStamp);
				loadedStaffingAudit.Id.HasValue.Should().Be.True();
			});
		}

		[Test]
		public void ShouldBeAbleToSaveContext()
		{

			StaffingAudit staffingAudit = null;
			var person = PersonFactory.CreatePerson(new Name("ash", "and"));
			var datetime = new DateTime(2018, 10, 11, 10, 10, 10, DateTimeKind.Utc);
			WithUnitOfWork.Do(() =>
			{
				PersonRepository.Add(person);
				staffingAudit = new StaffingAudit(person, "ClearBpoStaffing", "This is the data", "BPO");
				staffingAudit.TimeStamp = datetime;
				Target.Add(staffingAudit);
			});

			WithUnitOfWork.Do(() =>
			{
				var loadedStaffingAudit = Target.LoadAll().First();
				loadedStaffingAudit.Area.Should().Be.EqualTo("BPO");
			});
		}

		[Test]
		public void ShouldReturnStaffingAuditForTheGivenPeriod()
		{

			StaffingAudit staffingAudit = null;
			var person = PersonFactory.CreatePerson(new Name("ash", "and"));
			WithUnitOfWork.Do(() =>
			{
				PersonRepository.Add(person);
				var staffingAudit1 = new StaffingAudit(person, StaffingAuditActionConstants.ImportBpo, "", "BPO") { TimeStamp = new DateTime(2018, 10, 01, 08, 08, 08, DateTimeKind.Utc) };
				var staffingAudit2 = new StaffingAudit(person, StaffingAuditActionConstants.ImportBpo, "", "BPO") { TimeStamp = new DateTime(2018, 10, 02, 08, 08, 08, DateTimeKind.Utc) };
				var staffingAudit3 = new StaffingAudit(person, StaffingAuditActionConstants.ImportBpo, "", "BPO") { TimeStamp = new DateTime(2018, 10, 03, 08, 08, 08, DateTimeKind.Utc) };
				Target.Add(staffingAudit1);
				Target.Add(staffingAudit2);
				Target.Add(staffingAudit3);

			});

			WithUnitOfWork.Do(() =>
			{
				var loadedStaffingAudit =
					Target.LoadAudits(person, new DateTime(2018, 10, 01), new DateTime(2018, 10, 02));
				loadedStaffingAudit.Count().Should().Be.EqualTo(2);
			});
		}

		[Test]
		public void ShouldReturnStaffingAuditForTheGivenPerson()
		{
			StaffingAudit staffingAudit = null;
			var person1 = PersonFactory.CreatePerson(new Name("ash", "and"));
			var person2 = PersonFactory.CreatePerson(new Name("ashley", "and"));
			WithUnitOfWork.Do(() =>
			{
				PersonRepository.Add(person1);
				PersonRepository.Add(person2);
				var staffingAudit1 = new StaffingAudit(person1, StaffingAuditActionConstants.ImportBpo, "", "BPO") { TimeStamp = new DateTime(2018, 10, 01, 08, 08, 08, DateTimeKind.Utc) };
				var staffingAudit2 = new StaffingAudit(person2, StaffingAuditActionConstants.ImportBpo, "", "BPO") { TimeStamp = new DateTime(2018, 10, 02, 08, 08, 08, DateTimeKind.Utc) };
				Target.Add(staffingAudit1);
				Target.Add(staffingAudit2);

			});

			WithUnitOfWork.Do(() =>
			{
				var loadedStaffingAudit =
					Target.LoadAudits(person1, new DateTime(2018, 10, 01), new DateTime(2018, 10, 02));
				loadedStaffingAudit.Count().Should().Be.EqualTo(1);

				loadedStaffingAudit.First().ActionPerformedBy.Should().Be.EqualTo(person1);
			});
		}

	}
}
