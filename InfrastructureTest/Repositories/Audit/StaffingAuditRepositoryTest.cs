using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Staffing;
using Teleopti.Ccc.Domain.UnitOfWork;
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

	}
}
