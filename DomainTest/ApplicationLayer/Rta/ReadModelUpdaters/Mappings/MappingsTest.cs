using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ReadModelUpdaters.Mappings
{
	[Toggle(Toggles.RTA_RuleMappingOptimization_39812)]
	[TestFixture]
	[ReadModelUpdaterTest]
	public class MappingsTest
	{
		public FakeDatabase Database;
		public MappingReadModelUpdater Target;
		public FakeMappingReadModelPersister Persister;

		[Test]
		public void ShouldContainActivity()
		{
			var phone = Guid.NewGuid();
			Database.WithActivity(phone);

			Target.Handle(new ActivityChangedEvent());
			Target.Handle(new TenantMinuteTickEvent());

			Persister.Data.Select(x => x.ActivityId).Should().Contain(phone);
		}

		[Test]
		public void ShouldContainStateCode()
		{
			Database.WithStateCode("phone");

			Target.Handle(new RtaStateGroupChangedEvent());
			Target.Handle(new TenantMinuteTickEvent());

			Persister.Data.Select(x => x.StateCode).Should().Contain("phone");
		}

		[Test]
		public void ShouldContainAllStateActivityCombinations()
		{
			var activity1 = Guid.NewGuid();
			var activity2 = Guid.NewGuid();
			Database
				.WithActivity(activity1)
				.WithActivity(activity2)
				.WithStateCode("code1")
				.WithStateCode("code2");

			Target.Handle(new TenantMinuteTickEvent());

			Persister.Data.Where(x => x.StateCode == "code1" && x.ActivityId == activity1).Should().Not.Be.Empty();
			Persister.Data.Where(x => x.StateCode == "code1" && x.ActivityId == activity2).Should().Not.Be.Empty();
			Persister.Data.Where(x => x.StateCode == "code2" && x.ActivityId == activity1).Should().Not.Be.Empty();
			Persister.Data.Where(x => x.StateCode == "code2" && x.ActivityId == activity2).Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldContainAllStateNullCombinations()
		{
			Database
				.WithStateCode("code1")
				.WithStateCode("code2");

			Target.Handle(new TenantMinuteTickEvent());

			Persister.Data.Where(x => x.StateCode == "code1" && x.ActivityId == null).Should().Not.Be.Empty();
			Persister.Data.Where(x => x.StateCode == "code2" && x.ActivityId == null).Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldContainAllActivityNullCombinations()
		{
			var activity1 = Guid.NewGuid();
			var activity2 = Guid.NewGuid();
			Database
				.WithActivity(activity1)
				.WithActivity(activity2);

			Target.Handle(new TenantMinuteTickEvent());

			Persister.Data.Where(x => x.StateCode == null && x.ActivityId == activity1).Should().Not.Be.Empty();
			Persister.Data.Where(x => x.StateCode == null && x.ActivityId == activity2).Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldContainRuleStuff()
		{
			var rule = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database.WithRule(rule, "off", null, phone, 1, "Out", Adherence.Out);

			Target.Handle(new TenantMinuteTickEvent());

			var mapping = Persister.Data.Single(x => x.StateCode == "off" && x.ActivityId == phone);

			mapping.RuleId.Should().Be(rule);
			mapping.RuleName.Should().Be("Out");
			mapping.StaffingEffect.Should().Be(1);
			mapping.Adherence.Should().Be(Adherence.Out);
		}
	}
}