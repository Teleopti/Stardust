using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[DatabaseTest]
	public class MappingReaderTest
	{
		public IRtaMapRepository Maps;
		public IRtaStateGroupRepository Groups;
		public IRtaRuleRepository Rules;
		public IActivityRepository Activities;
		public IMappingReader Target;
		public WithUnitOfWork WithUnitOfWork;

		[Test]
		public void ShouldReadEmptyMapping()
		{
			WithUnitOfWork.Do(() => 
				Maps.Add(new RtaMap(null, null)));

			WithUnitOfWork.Get(() => Target.Read())
				.Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldReadActivity()
		{
			var phone = new Activity("Phone");
			WithUnitOfWork.Do(() =>
			{
				Activities.Add(phone);
				Maps.Add(new RtaMap(null, phone));
			});

			var result = WithUnitOfWork.Get(() => Target.Read())
				.SingleOrDefault(x => x.ActivityId != phone.Id.Value);

			result.Should().Not.Be.Null();
		}		

		[Test]
		public void ShouldReadStateFromMapping()
		{
			var group = new RtaStateGroup("Phone", true, true);
			group.AddState(".", "phone", Guid.NewGuid());
			WithUnitOfWork.Do(() =>
			{
				Groups.Add(group);
				Maps.Add(new RtaMap(group, null));
			});

			var result = WithUnitOfWork.Get(() => Target.Read())
				.First(x => x.StateGroupId == group.Id.Value);

			result.StateGroupId.Should().Be(group.Id.Value);
			result.StateGroupName.Should().Be("Phone");
			result.StateCode.Should().Be("phone");
			result.PlatformTypeId.Should().Be(group.StateCollection.Single().PlatformTypeId);
		}

		[Test]
		public void ShouldReadStateWithoutMapping()
		{
			var group = new RtaStateGroup("Phone", true, true);
			group.AddState(".", "phone", Guid.NewGuid());
			WithUnitOfWork.Do(() =>
			{
				Groups.Add(group);
			});

			var result = WithUnitOfWork.Get(() => Target.Read())
				.First(x => x.StateGroupId == group.Id.Value);

			result.StateGroupId.Should().Be(group.Id.Value);
			result.StateGroupName.Should().Be("Phone");
			result.StateCode.Should().Be("phone");
			result.PlatformTypeId.Should().Be(group.StateCollection.Single().PlatformTypeId);
		}

		[Test]
		public void ShouldIncludeStateGroupWithoutMapping()
		{
			var group = new RtaStateGroup("Phone", true, true);
			WithUnitOfWork.Do(() =>
			{
				Groups.Add(group);
			});

			var result = WithUnitOfWork.Get(() => Target.Read())
				.Single(x => x.StateGroupId == group.Id.Value);

			result.StateGroupId.Should().Be(group.Id.Value);
		}

		[Test]
		public void ShouldReadRule()
		{
			var rule = new RtaRule(
				new Description("InAdherence"), 
				Color.Blue, 
				TimeSpan.Zero, 
				1)
			{
				Adherence = Adherence.In,
			};
			WithUnitOfWork.Do(() =>
			{
				Rules.Add(rule);
				Maps.Add(new RtaMap(null, null) {RtaRule = rule});
			});

			var result = WithUnitOfWork.Get(() => Target.Read())
				.Single(x => x.ActivityId == null && x.StateGroupId == null);

			result.RuleId.Should().Be(rule.Id.Value);
			result.RuleName.Should().Be("InAdherence");
			result.DisplayColor.Should().Be(Color.Blue.ToArgb());
			result.StaffingEffect.Should().Be(1);
			result.Adherence.Should().Be(Adherence.In);
		}

		[Test]
		public void ShouldReadAlarm()
		{
			var rule = new RtaRule
			{
				Description = new Description("_"),
				IsAlarm = true,
				AlarmColor = Color.Red,
				ThresholdTime = TimeSpan.FromSeconds(2)
			};
			WithUnitOfWork.Do(() =>
			{
				Rules.Add(rule);
				Maps.Add(new RtaMap(null, null) { RtaRule = rule });
			});

			var result = WithUnitOfWork.Get(() => Target.Read())
				.Single(x => x.ActivityId == null && x.StateGroupId == null);

			result.IsAlarm.Should().Be(true);
			result.AlarmColor.Should().Be(Color.Red.ToArgb());
			result.ThresholdTime.Should().Be(TimeSpan.FromSeconds(2).Ticks);
		}

		[Test]
		public void ShouldReadBusinessUnitFromMapping()
		{
			var mapping = new RtaMap(null, null);
			WithUnitOfWork.Do(() => Maps.Add(mapping));

			var result = WithUnitOfWork.Get(() => Target.Read())
				.Single(x => x.ActivityId == null && x.StateGroupId == null);

			result.BusinessUnitId.Should().Be(mapping.BusinessUnit.Id.Value);
		}

		[Test]
		public void ShouldReadBusinessUnitFromState()
		{
			var group = new RtaStateGroup("Phone", true, true);
			WithUnitOfWork.Do(() =>
			{
				Groups.Add(group);
			});

			var result = WithUnitOfWork.Get(() => Target.Read())
				.Single(x => x.StateGroupId == group.Id.Value);

			result.BusinessUnitId.Should().Be(group.BusinessUnit.Id.Value);
		}

		[Test]
		public void ShouldReadMappingWithoutStateGroup()
		{
			WithUnitOfWork.Do(() => Maps.Add(new RtaMap(null, null)));

			WithUnitOfWork.Get(() => Target.Read())
				.Single().StateGroupId.Should().Be(null);
		}

		[Test]
		public void ShouldReadActivityWithoutMapping()
		{
			var phone = new Activity("Phone");
			WithUnitOfWork.Do(() =>
			{
				Activities.Add(phone);
			});

			WithUnitOfWork.Get(() => Target.Read())
				.Select(x => x.ActivityId).Should().Contain(phone.Id.Value);
		}

		[Test]
		public void ShouldAlwaysReadMappingForNoActivityAndNoState()
		{
			var result = WithUnitOfWork.Get(() => Target.Read()).Single();

			result.ActivityId.Should().Be(null);
			result.StateGroupId.Should().Be(null);
		}

		[Test]
		public void ShouldIncludeMissingMappingCombinations()
		{
			var group = new RtaStateGroup("Phone", true, true);
			var activity = new Activity("Phone");
			WithUnitOfWork.Do(() =>
			{
				Groups.Add(group);
				Activities.Add(activity);
			});

			var result = WithUnitOfWork.Get(() => Target.Read());

			result.Where(x => x.ActivityId == activity.Id.Value && x.StateGroupId == group.Id.Value).Should().Not.Be.Empty();
			result.Where(x => x.ActivityId == activity.Id.Value && x.StateGroupId == null).Should().Not.Be.Empty();
			result.Where(x => x.ActivityId == null && x.StateGroupId == group.Id.Value).Should().Not.Be.Empty();
			result.Where(x => x.ActivityId == null && x.StateGroupId == null).Should().Not.Be.Empty();
		}
	}
}