using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.RealTimeAdherence;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
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
		public MappingReader Target;
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

			WithUnitOfWork.Get(() => Target.Read())
				.Single().ActivityId
				.Should().Be(phone.Id.Value);
		}		

		[Test]
		public void ShouldReadStateFromMapping()
		{
			var group = new RtaStateGroup("Phone", true, true);
			group.IsLogOutState = true;
			group.AddState(".", "phone", Guid.NewGuid());
			WithUnitOfWork.Do(() =>
			{
				Groups.Add(group);
				Maps.Add(new RtaMap(group, null));
			});

			var mapping = WithUnitOfWork.Get(() => Target.Read()).Single();

			mapping.StateGroupId.Should().Be(group.Id.Value);
			mapping.StateGroupName.Should().Be("Phone");
			mapping.IsLogOutState.Should().Be(true);
			mapping.StateCode.Should().Be("phone");
			mapping.PlatformTypeId.Should().Be(group.StateCollection.Single().PlatformTypeId);
		}

		[Test]
		public void ShouldReadStateWithoutMapping()
		{
			var group = new RtaStateGroup("Phone", true, true);
			group.IsLogOutState = true;
			group.AddState(".", "phone", Guid.NewGuid());
			WithUnitOfWork.Do(() =>
			{
				Groups.Add(group);
			});

			var mapping = WithUnitOfWork.Get(() => Target.Read()).Single();

			mapping.StateGroupId.Should().Be(group.Id.Value);
			mapping.StateGroupName.Should().Be("Phone");
			mapping.IsLogOutState.Should().Be(true);
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

			var mapping = WithUnitOfWork.Get(() => Target.Read()).Single();

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

			var mapping = WithUnitOfWork.Get(() => Target.Read()).Single();

			mapping.IsAlarm.Should().Be(true);
			mapping.AlarmColor.Should().Be(Color.Red.ToArgb());
			mapping.ThresholdTime.Should().Be(TimeSpan.FromSeconds(2).Ticks);
		}

		[Test]
		public void ShouldReadBusinessUnitFromMapping()
		{
			var mapping = new RtaMap(null, null);
			WithUnitOfWork.Do(() => Maps.Add(mapping));

			WithUnitOfWork.Get(() => Target.Read())
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

			WithUnitOfWork.Get(() => Target.Read())
				.Single().BusinessUnitId.Should().Be(group.BusinessUnit.Id.Value);
		}
	}
}