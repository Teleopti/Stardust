using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Configuration;
using Teleopti.Wfm.Adherence.States;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.States.Infrastructure.ReadModels
{
	public class WithUnitOfWorkWithRecurringEvents
	{
		private readonly WithUnitOfWork _unitOfWork;
		private readonly IEventPublisher _publisher;

		public WithUnitOfWorkWithRecurringEvents(WithUnitOfWork unitOfWork, IEventPublisher publisher)
		{
			_unitOfWork = unitOfWork;
			_publisher = publisher;
		}

		public void Do(Action action)
		{
			_unitOfWork.Do(action);
			_publisher.Publish(new TenantMinuteTickEvent());
		}
	}

	[TestFixture]
	[DatabaseTest]
	public class MappingReadModelReaderTest : IExtendSystem
	{
		public IRtaMapRepository Maps;
		public IRtaStateGroupRepository Groups;
		public IRtaRuleRepository Rules;
		public IActivityRepository Activities;
		public IMappingReader Target;
		public WithReadModelUnitOfWork WithReadModels;
		public WithUnitOfWorkWithRecurringEvents WithUnitOfWork;
		public IEventPublisher EventPublisher;
		public ICurrentBusinessUnit CurrentBusinessUnit;

		public void Extend(IExtend extend, IocConfiguration configuration)
		{
			extend.AddService<WithUnitOfWorkWithRecurringEvents>();
			extend.AddService<SyncAllEventPublisher>();
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
			ensureBusinessUnitInAnalytics();

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
			group.AddState("phone", ".");
			WithUnitOfWork.Do(() =>
			{
				Groups.Add(group);
				Maps.Add(new RtaMap(group, null));
			});

			var mapping = WithReadModels.Get(() => Target.Read()).Single(x => x.StateGroupId == group.Id.Value);

			mapping.StateGroupId.Should().Be(group.Id.Value);
			mapping.StateGroupName.Should().Be("Phone");
			mapping.StateCode.Should().Be("phone");
		}

		[Test]
		public void ShouldReadStateWithoutMapping()
		{
			var group = new RtaStateGroup("Phone", true, true);
			group.AddState("phone", ".");
			WithUnitOfWork.Do(() => { Groups.Add(group); });

			var mapping = WithReadModels.Get(() => Target.Read()).Single(x => x.StateGroupId == group.Id.Value);

			mapping.StateGroupId.Should().Be(group.Id.Value);
			mapping.StateGroupName.Should().Be("Phone");
			mapping.StateCode.Should().Be("phone");
		}

		[Test]
		public void ShouldReadRule()
		{
			var rule = new RtaRule(
				new Description("InAdherence"),
				Color.Blue,
				0,
				1)
			{
				Adherence = Adherence.Configuration.Adherence.In,
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
			mapping.Adherence.Should().Be(Adherence.Configuration.Adherence.In);
		}

		[Test]
		public void ShouldReadAlarm()
		{
			var rule = new RtaRule
			{
				Description = new Description("_"),
				IsAlarm = true,
				AlarmColor = Color.Red,
				ThresholdTime = 2
			};
			WithUnitOfWork.Do(() =>
			{
				Rules.Add(rule);
				Maps.Add(new RtaMap(null, null) {RtaRule = rule});
			});

			var mapping = WithReadModels.Get(() => Target.Read()).Single();

			mapping.IsAlarm.Should().Be(true);
			mapping.AlarmColor.Should().Be(Color.Red.ToArgb());
			mapping.ThresholdTime.Should().Be(2);
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
			group.AddState("Phone");
			WithUnitOfWork.Do(() => { Groups.Add(group); });

			WithReadModels.Get(() => Target.Read())
				.Single(x => x.StateGroupId == group.Id.Value)
				.BusinessUnitId.Should().Be(group.BusinessUnit.Value);
		}

		private void ensureBusinessUnitInAnalytics()
		{
			// To prevend analytics handlers from failing we need a BU in analytics, this is a slightly backwards way of achieving that
			WithUnitOfWork.Do(() =>
			{
				EventPublisher.Publish(new BusinessUnitChangedEvent
				{
					BusinessUnitId = CurrentBusinessUnit.Current().Id.GetValueOrDefault(),
					BusinessUnitName = CurrentBusinessUnit.Current().Name,
					UpdatedOn = DateTime.Now
				});
			});
		}
	}
}