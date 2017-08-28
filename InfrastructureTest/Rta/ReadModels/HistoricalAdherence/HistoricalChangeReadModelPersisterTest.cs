using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.HistoricalAdherence
{
	[TestFixture]
	[ReadModelUnitOfWorkTest]
	public class HistoricalChangeReadModelPersisterTest
	{
		public IHistoricalChangeReadModelPersister Target;
		public IHistoricalChangeReadModelReader Reader;

		[Test]
		public void ShouldPersist()
		{
			var personId = Guid.NewGuid();
			var state = Guid.NewGuid();

			Target.Persist(new HistoricalChangeReadModel
			{
				PersonId = personId,
				BelongsToDate = "2017-03-07".Date(),
				Timestamp = "2017-03-07 10:00".Utc(),
				StateName = "ready",
				StateGroupId = state,
				ActivityName = "phone",
				ActivityColor = Color.DarkGoldenrod.ToArgb(),
				RuleName = "in",
				RuleColor = Color.Azure.ToArgb(),
				Adherence = HistoricalChangeInternalAdherence.In
			});

			var change = Reader.Read(personId, "2017-03-07 00:00".Utc(), "2017-03-08 00:00".Utc()).Single();
			change.PersonId.Should().Be(personId);
			change.BelongsToDate.Should().Be("2017-03-07".Date());
			change.Timestamp.Should().Be("2017-03-07 10:00".Utc());
			change.StateName.Should().Be("ready");
			change.StateGroupId.Should().Be(state);
			change.ActivityName.Should().Be("phone");
			change.ActivityColor.Should().Be(Color.DarkGoldenrod.ToArgb());
			change.RuleName.Should().Be("in");
			change.RuleColor.Should().Be(Color.Azure.ToArgb());
			change.Adherence.Should().Be(HistoricalChangeInternalAdherence.In);
		}
	}
}