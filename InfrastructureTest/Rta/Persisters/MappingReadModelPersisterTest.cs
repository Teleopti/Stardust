using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Rta.Persisters
{
	[Toggle(Toggles.RTA_RuleMappingOptimization_39812)]
	[TestFixture]
	[ReadModelUnitOfWorkTest]
	public class MappingReadModelPersisterTest
	{
		public IMappingReadModelPersister Target;
		public IMappingReader Reader;

		[Test]
		public void ShouldPersist()
		{
			var businessUnit = Guid.NewGuid();
			var platformType = Guid.NewGuid();
			var group = Guid.NewGuid();
			var activity = Guid.NewGuid();
			var rule = Guid.NewGuid();

			Target.Persist(new[]
			{
				new Mapping
				{
					BusinessUnitId = businessUnit,
					StateCode = "0",
					PlatformTypeId = platformType,
					StateGroupId = group,
					ActivityId = activity,
					RuleId = rule,
					RuleName = "phone",
					Adherence = Adherence.In,
					StaffingEffect = 0,
					DisplayColor = Color.Green.ToArgb(),
					IsAlarm = false,
					ThresholdTime = 100,
					AlarmColor = Color.Red.ToArgb()
				}
			});

			var model = Reader.Read().Single();
			model.BusinessUnitId.Should().Be(businessUnit);
			model.StateCode.Should().Be("0");
			model.PlatformTypeId.Should().Be(platformType);
			model.StateGroupId.Should().Be(group);
			model.ActivityId.Should().Be(activity);
			model.RuleId.Should().Be(rule);
			model.RuleName.Should().Be("phone");
			model.Adherence.Should().Be(Adherence.In);
			model.StaffingEffect.Should().Be(0);
			model.DisplayColor.Should().Be(Color.Green.ToArgb());
			model.IsAlarm.Should().Be(false);
			model.ThresholdTime.Should().Be(100);
			model.AlarmColor.Should().Be(Color.Red.ToArgb());
		}

		[Test]
		public void ShouldPersistWithNullValues()
		{
			Target.Persist(new[] { new Mapping() });

			var model = Reader.Read().Single();
			model.BusinessUnitId.Should().Be(Guid.Empty);
			model.StateCode.Should().Be.Null();
			model.PlatformTypeId.Should().Be(Guid.Empty);
			model.StateGroupId.Should().Be(Guid.Empty);
			model.ActivityId.Should().Be(null);
			model.RuleId.Should().Be(Guid.Empty);
			model.RuleName.Should().Be(null);
			model.Adherence.Should().Be(null);
			model.StaffingEffect.Should().Be(null);
			model.DisplayColor.Should().Be(0);
			model.IsAlarm.Should().Be(false);
			model.ThresholdTime.Should().Be(0);
			model.AlarmColor.Should().Be(0);
		}

		[Test]
		public void ShouldReplaceOnPersist()
		{
			var businessUnit = Guid.NewGuid();

			Target.Persist(new[]
			{
				new Mapping
				{
					BusinessUnitId = Guid.NewGuid()
				}
			});
			Target.Invalidate();
			Target.Persist(new[]
			{
				new Mapping
				{
					BusinessUnitId = businessUnit,
				}
			});

			Reader.Read().Single().BusinessUnitId.Should().Be(businessUnit);
		}

		[Test]
		public void ShouldBeValidAfterPersist()
		{
			Target.Invalidate();
			Target.Persist(new[] {new Mapping() });
			Target.Invalid().Should().Be(false);
		}

		[Test]
		public void ShouldNotThrowWhenInvalidatingInvalid()
		{
			Target.Invalidate();
			Target.Invalidate();
			Target.Invalid().Should().Be(true);
		}

		[Test]
		public void ShouldBeInvalidWhenReinvalidated()
		{
			Target.Invalidate();
			Target.Persist(new[] { new Mapping { } });
			Target.Invalidate();
			Target.Invalid().Should().Be(true);
		}

		[Test]
		public void ShouldBeInvalidWithEmptyMapping()
		{
			Target.Invalid().Should().Be(true);
		}
	}
}