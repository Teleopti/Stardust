using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.States.Events;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[TestFixture]
	[RtaTest]
	public class UnrecognizedStateTest
	{
		public FakeDatabase Database;
		public Rta Target;
		public FakeRtaStateGroupRepository StateGroups;
		public FakeEventPublisher Publisher;

		[Test]
		public void ShouldAddStateCodeToDatabase()
		{
			Database
				.WithAgent("usercode")
				.WithStateGroup(null, "default", true);

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "newStateCode"
			});

			StateGroups.LoadAll().SelectMany(x => x.StateCollection).Select(x => x.StateCode)
				.Should().Contain("newStateCode");
		}

		[Test]
		public void ShouldPublishEvent()
		{
			Database
				.WithAgent("usercode")
				.WithStateGroup(null, "default", true);

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "newStateCode"
			});

			Publisher.PublishedEvents.OfType<UnknownStateCodeReceviedEvent>().Select(x => x.StateCode)
				.Should().Contain("newStateCode");
		}

		[Test]
		public void ShouldAddStateCodeWithDescription()
		{
			Database
				.WithAgent("usercode")
				.WithStateGroup(null, "default", true);

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "newStateCode",
				StateDescription = "a new description"
			});

			Publisher.PublishedEvents.OfType<UnknownStateCodeReceviedEvent>().Select(x => x.StateDescription)
				.Should().Contain("a new description");
		}

		[Test]
		// this test seems a bit unclear, but saved us today
		public void ShouldNotAddStateCodeUnlessStateReceived()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithMappedRule("someStateCode");

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			StateGroups.LoadAll().SelectMany(x => x.StateCollection).Select(x => x.StateCode)
				.Should().Contain("someStateCode");
		}

		[Test]
		public void ShouldAddStateCodeToDatabaseEvenWhenSameStateGroup()
		{
			Database
				.WithAgent("usercode")
				.WithStateGroup(null, "default", true)
				.WithStateCode("existingStateCode");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "existingStateCode"
			});
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "newStateCode"
			});

			Publisher.PublishedEvents.OfType<UnknownStateCodeReceviedEvent>().Select(x => x.StateCode)
				.Should().Contain("newStateCode");
			StateGroups.LoadAll().SelectMany(x => x.StateCollection).Select(x => x.StateCode)
				.Should().Contain("newStateCode");
		}
	}
}