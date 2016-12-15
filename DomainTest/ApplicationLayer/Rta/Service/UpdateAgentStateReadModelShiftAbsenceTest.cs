using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Interfaces;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class UpdateAgentStateReadModelShiftAbsenceTest
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public IJsonDeserializer Deserializer;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

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
			
			var shift = Database.PersistedReadModel.Shift.Single();
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

			var shift = Database.PersistedReadModel.Shift.Single();
			shift.StartTime.Should().Be("2016-10-14 09:00".Utc());
			shift.EndTime.Should().Be("2016-10-14 17:00".Utc());
		}

	}

}