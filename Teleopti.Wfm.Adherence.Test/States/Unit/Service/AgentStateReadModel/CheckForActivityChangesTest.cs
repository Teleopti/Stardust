using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service.AgentStateReadModel
{
	[TestFixture]
	[RtaTest]
	public class CheckForActivityChangesTest
	{
		public FakeDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Rta Target;
		public RtaTestAttribute Context;
		public FakeAgentStateReadModelPersister ReadModels;

		[Test]
		public void ShouldKeepPreviousStateWhenNotifiedOfActivityChange1()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithStateCode("phone");
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Target.CheckForActivityChanges(Database.TenantName(), personId);

			ReadModels.Models.Single(x => x.PersonId == personId)
				.StateName.Should().Be("phone");
		}

		[Test]
		public void ShouldKeepPreviousStateWhenNotifiedOfActivityChange2()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 9:00", "2014-10-20 11:00")
				.WithStateGroup(null, "alarm")
				.WithStateCode("phone")
				.WithMappedRule("phone", activityId)
				;

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Target.CheckForActivityChanges(Database.TenantName(), personId);

			ReadModels.Models.Single(x => x.PersonId == personId)
				.StateName.Should().Be("alarm");
		}

		[Test]
		public void ShouldNoticeScheduleChange()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var lunch = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-09-21 09:00", "2015-09-21 11:00")
				.WithSchedule(personId, lunch, "2015-09-21 11:00", "2015-09-21 12:00")
				;

			Now.Is("2015-09-21 09:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2015-09-21 09:05");
			Database
				.ClearAssignments(personId)
				.WithSchedule(personId, phone, "2015-09-21 09:00", "2015-09-21 12:00")
				.WithSchedule(personId, lunch, "2015-09-21 12:00", "2015-09-21 13:00")
				;
			Target.CheckForActivityChanges(Database.TenantName());

			ReadModels.Models.Single(x => x.PersonId == personId)
				.NextActivityStartTime.Should().Be("2015-09-21 12:00".Utc());
		}
	}
}