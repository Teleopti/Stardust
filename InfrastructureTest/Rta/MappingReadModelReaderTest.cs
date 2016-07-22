using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[DatabaseTest]
	[Toggle(Toggles.RTA_RuleMappingOptimization_39812)]
	public class MappingReadModelReaderTest : ISetup
	{
		public IRtaMapRepository Maps;
		public IRtaStateGroupRepository Groups;
		public IRtaRuleRepository Rules;
		public IActivityRepository Activities;
		public IMappingReader Target;
		public WithUnitOfWorkWithRecurringEvents WithUnitOfWork;
		public WithReadModelUnitOfWork WithReadModels;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddService<WithUnitOfWorkWithRecurringEvents>();
			system.AddService<SyncAllEventPublisher>();
		}

		[Test]
		public void ShouldReadEmptyMapping()
		{
			WithUnitOfWork.Do(() => 
				Maps.Add(new RtaMap(null, null)));

			WithReadModels.Get(() => Target.Read())
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

			WithReadModels.Get(() => Target.Read())
				.Select(x => x.ActivityId).Should().Contain(phone.Id.Value);
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

			var mapping = WithReadModels.Get(() => Target.Read()).Single(x => x.StateGroupId == group.Id.Value);

			mapping.StateGroupId.Should().Be(group.Id.Value);
			mapping.StateGroupName.Should().Be("Phone");
			mapping.StateCode.Should().Be("phone");
			mapping.PlatformTypeId.Should().Be(group.StateCollection.Single().PlatformTypeId);
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

			var mapping = WithReadModels.Get(() => Target.Read()).Single(x => x.StateGroupId == group.Id.Value);

			mapping.StateGroupId.Should().Be(group.Id.Value);
			mapping.StateGroupName.Should().Be("Phone");
			mapping.StateCode.Should().Be("phone");
			mapping.PlatformTypeId.Should().Be(group.StateCollection.Single().PlatformTypeId);
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

			var mapping = WithReadModels.Get(() => Target.Read()).Single();

			mapping.RuleId.Should().Be(rule.Id.Value);
			mapping.RuleName.Should().Be("InAdherence");
			mapping.DisplayColor.Should().Be(Color.Blue.ToArgb());
			mapping.StaffingEffect.Should().Be(1);
			mapping.Adherence.Should().Be(Adherence.In);
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

			var mapping = WithReadModels.Get(() => Target.Read()).Single();

			mapping.IsAlarm.Should().Be(true);
			mapping.AlarmColor.Should().Be(Color.Red.ToArgb());
			mapping.ThresholdTime.Should().Be(TimeSpan.FromSeconds(2).Ticks);
		}

		[Test]
		public void ShouldReadBusinessUnitFromMapping()
		{
			var mapping = new RtaMap(null, null);
			WithUnitOfWork.Do(() => Maps.Add(mapping));

			WithReadModels.Get(() => Target.Read())
				.Single().BusinessUnitId.Should().Be(mapping.BusinessUnit.Id.Value);
		}

		[Test]
		public void ShouldReadBusinessUnitFromState()
		{
			var group = new RtaStateGroup("Phone", true, true);
			WithUnitOfWork.Do(() =>
			{
				Groups.Add(group);
			});

			WithReadModels.Get(() => Target.Read())
				.Single(x => x.StateGroupId == group.Id.Value)
				.BusinessUnitId.Should().Be(group.BusinessUnit.Id.Value);
		}

	}
}