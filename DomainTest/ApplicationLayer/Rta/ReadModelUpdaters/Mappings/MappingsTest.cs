using System;
using System.Drawing;
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

			Target.Handle(new TenantMinuteTickEvent());

			Persister.Data.Select(x => x.ActivityId).Should().Contain(phone);
		}

		[Test]
		public void ShouldContainStateCode()
		{
			Database.WithStateCode("phone");

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
		public void ShouldContainAllNullCombinations()
		{
			var businessUnitId1 = Guid.NewGuid();
			var businessUnitId2 = Guid.NewGuid();
			Database
				.WithBusinessUnit(businessUnitId1)
				.WithBusinessUnit(businessUnitId2);

			Target.Handle(new TenantMinuteTickEvent());

			Persister.Data.Where(x => x.StateCode == null && x.ActivityId == null && x.BusinessUnitId == businessUnitId1)
				.Should().Have.Count.EqualTo(1);	
			Persister.Data.Where(x => x.StateCode == null && x.ActivityId == null && x.BusinessUnitId == businessUnitId2)
				.Should().Have.Count.EqualTo(1);	
		}

		[Test]
		public void ShouldContainRule()
		{
			var rule = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database.WithRule(rule, "off", null, phone, 1, "Out", Adherence.Out, Color.Red);

			Target.Handle(new TenantMinuteTickEvent());

			var mapping = Persister.Data.Single(x => x.StateCode == "off" && x.ActivityId == phone);
			mapping.RuleId.Should().Be(rule);
			mapping.RuleName.Should().Be("Out");
			mapping.StaffingEffect.Should().Be(1);
			mapping.Adherence.Should().Be(Adherence.Out);
			mapping.DisplayColor.Should().Be(Color.Red.ToArgb());
		}

		[Test]
		public void ShouldContainAlarm()
		{
			var rule = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database.WithRule(rule, "phone", null, phone, 0, "In", Adherence.In, Color.Green);
			Database.WithAlarm(TimeSpan.FromMinutes(2), Color.Red);

			Target.Handle(new TenantMinuteTickEvent());

			var mapping = Persister.Data.Single(x => x.StateCode == "phone" && x.ActivityId == phone);
			mapping.IsAlarm.Should().Be(true);
			mapping.ThresholdTime.Should().Be(TimeSpan.FromMinutes(2).TotalSeconds);
			mapping.AlarmColor.Should().Be(Color.Red.ToArgb());
		}

		[Test]
		public void ShouldContainStateGroup()
		{
			var phone = Guid.NewGuid();
			Database.WithStateGroup(phone, "phone");

			Target.Handle(new TenantMinuteTickEvent());

			Persister.Data.First(x => x.StateGroupId == phone).StateGroupName.Should().Be("phone");
		}

		[Test]
		public void ShouldContainPlatform()
		{
			var platform = Guid.NewGuid();
			Database
				.WithPlatform(platform)
				.WithStateCode("phone");

			Target.Handle(new TenantMinuteTickEvent());

			Persister.Data.First(x => x.StateCode == "phone").PlatformTypeId.Should().Be(platform);
		}

		[Test]
		public void ShouldContainBusinessUnitFromStateGroup()
		{
			var phone = Guid.NewGuid();
			Database
				.WithStateGroup(phone, "phone");

			Target.Handle(new TenantMinuteTickEvent());

			Persister.Data.First().BusinessUnitId.Should().Be(Database.CurrentBusinessUnitId());
		}

		[Test]
		public void ShouldContainBusinessUnitFromMapping()
		{
			var rule1 = Guid.NewGuid();
			var rule2 = Guid.NewGuid();
			var rule3 = Guid.NewGuid();
			Database
				.WithRule(rule1, "phone", null, null, 0, null, null, null)
				.WithRule(rule2, null, null, Guid.NewGuid(), 0, null, null, null)
				.WithRule(rule3, null, null, null, 0, null, null, null);

			Target.Handle(new TenantMinuteTickEvent());

			Persister.Data.Single(x => x.RuleId == rule1).BusinessUnitId.Should().Be(Database.CurrentBusinessUnitId());
			Persister.Data.Single(x => x.RuleId == rule2).BusinessUnitId.Should().Be(Database.CurrentBusinessUnitId());
			Persister.Data.Single(x => x.RuleId == rule3).BusinessUnitId.Should().Be(Database.CurrentBusinessUnitId());
		}

		[Test]
		public void ShouldContainNoDuplicates()
		{
			Database
				.WithActivity(Guid.NewGuid())
				.WithActivity(Guid.NewGuid())
				.WithStateCode("code1")
				.WithStateCode("code2")
				.WithRule(Guid.NewGuid(), "code3", null, null, 0, null, null, null)
				.WithRule(Guid.NewGuid(), null, null, Guid.NewGuid(), 0, null, null, null)
				.WithRule(Guid.NewGuid(), null, null, null, 0, null, null, null);

			Target.Handle(new TenantMinuteTickEvent());

			var actual = Persister.Data.Select(x => x.StateCode + x.ActivityId.GetValueOrDefault()).ToArray();
			var expected = actual.Distinct().ToArray();
			actual.Should().Have.SameSequenceAs(expected);
		}



		[Test]
		public void ShouldRegenerateOnUpdate()
		{
			var phone = Guid.NewGuid();
			Target.Handle(new TenantMinuteTickEvent());

			Database.WithActivity(phone);
			Target.Handle(new ActivityChangedEvent());
			Target.Handle(new TenantMinuteTickEvent());

			var actual = Persister.Data.Select(x => x.StateCode + x.ActivityId.GetValueOrDefault()).ToArray();
			var expected = actual.Distinct().ToArray();
			actual.Should().Have.SameSequenceAs(expected);
		}

		[Test]
		public void ShouldOnlyUpdateWhenInvalido()
		{
			var phone = Guid.NewGuid();
			Target.Handle(new TenantMinuteTickEvent());

			Database.WithActivity(phone);
			Target.Handle(new TenantMinuteTickEvent());

			Persister.Data.Select(x => x.ActivityId).Should().Not.Contain(phone);
		}

		[Test]
		public void ShouldNotUpdateWhenValidAgain()
		{
			var activity1 = Guid.NewGuid();
			var activity2 = Guid.NewGuid();
			Target.Handle(new TenantMinuteTickEvent());

			Database.WithActivity(activity1);
			Target.Handle(new ActivityChangedEvent());
			Target.Handle(new TenantMinuteTickEvent());
			Database.WithActivity(activity2);
			Target.Handle(new TenantMinuteTickEvent());

			Persister.Data.Select(x => x.ActivityId).Should().Not.Contain(activity2);
		}

		[Test]
		public void ShouldUpdateOnActivityChanges()
		{
			var phone = Guid.NewGuid();
			Target.Handle(new TenantMinuteTickEvent());

			Database.WithActivity(phone);
			Target.Handle(new ActivityChangedEvent());
			Target.Handle(new TenantMinuteTickEvent());

			Persister.Data.Select(x => x.ActivityId).Should().Contain(phone);
		}

		[Test]
		public void ShouldUpdateOnStateGroupChanges()
		{
			var ready = Guid.NewGuid();
			Target.Handle(new TenantMinuteTickEvent());

			Database.WithStateGroup(ready, null);
			Target.Handle(new ActivityChangedEvent());
			Target.Handle(new TenantMinuteTickEvent());

			Persister.Data.Select(x => x.StateGroupId).Should().Contain(ready);
		}

		[Test]
		public void ShouldUpdateOnMappingChanges()
		{
			var ready = Guid.NewGuid();
			Target.Handle(new TenantMinuteTickEvent());

			Database.WithStateGroup(ready, null);
			Target.Handle(new RtaMapChangedEvent());
			Target.Handle(new TenantMinuteTickEvent());

			Persister.Data.Select(x => x.StateGroupId).Should().Contain(ready);
		}

	}
}