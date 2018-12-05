using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service.AgentStateReadModel
{
	[TestFixture]
	[RtaTest]
	public class UpdateAgentStateReadModelShiftAbsenceTest
	{
		public FakeDatabase Database;
		public MutableNow Now;
		public Rta Target;
		public FakeAgentStateReadModelPersister ReadModels;

		[Test]
		public void ShouldPersistAbsence()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Now.Is("2016-10-14 07:00");
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "2016-10-14 09:00", "2016-10-14 17:00")
				.WithAbsence("vacation")
				.WithPersonAbsence("2016-10-14 00:00", "2016-10-15 00:00")
				;

			Target.CheckForActivityChanges(Database.TenantName());
			
			var shift = ReadModels.Models.Single(x => x.PersonId == person)
				.Shift.Single();
			shift.StartTime.Should().Be("2016-10-14 09:00".Utc());
			shift.EndTime.Should().Be("2016-10-14 17:00".Utc());
			shift.Name.Should().Be("vacation");
		}

		[Test]
		public void ShouldPersistConfidentialAbsence()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Now.Is("2016-10-14 07:00");
			Database
				.WithAgent("usercode", person)
				.WithSchedule(person, phone, "2016-10-14 09:00", "2016-10-14 17:00")
				.WithConfidentialAbsence()
				.WithPersonAbsence("2016-10-14 00:00", "2016-10-15 00:00")
				;

			Target.CheckForActivityChanges(Database.TenantName());

			var shift = ReadModels.Models.Single(x => x.PersonId == person)
				.Shift.Single();
			shift.StartTime.Should().Be("2016-10-14 09:00".Utc());
			shift.EndTime.Should().Be("2016-10-14 17:00".Utc());
		}

	}

}