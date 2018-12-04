using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.States.Infrastructure.ReadModels
{
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
			var group = Guid.NewGuid();
			var activity = Guid.NewGuid();
			var rule = Guid.NewGuid();

			Target.Persist(new[]
			{
				new Mapping
				{
					BusinessUnitId = businessUnit,
					StateCode = "0",
					StateGroupId = group,
					ActivityId = activity,
					RuleId = rule,
					RuleName = "phone",
					Adherence = Adherence.Configuration.Adherence.In,
					StaffingEffect = 0,
					DisplayColor = Color.Green.ToArgb(),
					IsAlarm = false,
					ThresholdTime = 100,
					AlarmColor = Color.Red.ToArgb(),
					IsLoggedOut = true
				}
			});

			var model = Reader.Read().Single();
			model.BusinessUnitId.Should().Be(businessUnit);
			model.StateCode.Should().Be("0");
			model.StateGroupId.Should().Be(group);
			model.ActivityId.Should().Be(activity);
			model.RuleId.Should().Be(rule);
			model.RuleName.Should().Be("phone");
			model.Adherence.Should().Be(Adherence.Configuration.Adherence.In);
			model.StaffingEffect.Should().Be(0);
			model.DisplayColor.Should().Be(Color.Green.ToArgb());
			model.IsAlarm.Should().Be(false);
			model.ThresholdTime.Should().Be(100);
			model.AlarmColor.Should().Be(Color.Red.ToArgb());
			model.IsLoggedOut.Should().Be(true);
		}

		[Test]
		public void ShouldPersistMany()
		{
			Target.Persist(
				Enumerable.Range(0, 237)
				.Select(i => new Mapping {BusinessUnitId = Guid.NewGuid()})
				);

			Reader.Read().Should().Have.Count.EqualTo(237);
		}

		[Test]
		public void ShouldPersistWithNullValues()
		{
			Target.Persist(new[] { new Mapping() });

			var model = Reader.Read().Single();
			model.BusinessUnitId.Should().Be(Guid.Empty);
			model.StateCode.Should().Be.Null();
			model.StateGroupId.Should().Be(null);
			model.ActivityId.Should().Be(null);
			model.RuleId.Should().Be(null);
			model.RuleName.Should().Be(null);
			model.Adherence.Should().Be(null);
			model.StaffingEffect.Should().Be(null);
			model.DisplayColor.Should().Be(0);
			model.IsAlarm.Should().Be(false);
			model.ThresholdTime.Should().Be(0);
			model.AlarmColor.Should().Be(0);
			model.IsLoggedOut.Should().Be(true);
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