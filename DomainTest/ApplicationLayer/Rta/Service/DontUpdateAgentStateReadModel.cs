using NUnit.Framework;
using System;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	[TestFixture]
	public class DontUpdateAgentStateReadModel
	{
		public FakeAgentStateReadModelPersister Persister;
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public AgentStateReadModelMaintainer Maintainer; 
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldNotUpdateIfPersonIsTerminated()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithRule("phone")
				.WithRule("loggedOff");
			
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Maintainer.Handle(new PersonAssociationChangedEvent
			{
				PersonId = person,
				TeamId = null
			});

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedOff"
			});

			Persister.Load(person).StateCode.Should().Be("phone");
		}

		[Test]
		public void ShouldNotUpdateIfPersonIsDeleted()
		{
			var person = Guid.NewGuid();
			Database
				.WithAgent("usercode", person)
				.WithRule("phone")
				.WithRule("loggedOff");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Maintainer.Handle(new PersonDeletedEvent
			{
				PersonId = person
			});

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedOff"
			});

			Persister.Load(person).StateCode.Should().Be("phone");
		}
	}
}