using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Rta.AdherenceChange;
using Teleopti.Ccc.Domain.Rta.AgentAdherenceDay;
using Teleopti.Ccc.Domain.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.InfrastructureTest.Rta.ReadModels.HistoricalAdherence
{
	[TestFixture]
	[DatabaseTest]
	public class HistoricalChangeReadModelReaderTest
	{
		public AdherenceChange AdherenceChange;
		public IHistoricalChangeReadModelReader Reader;
		public WithReadModelUnitOfWork WithReadModelUnitOfWork;

		[Test]
		public void ShouldRead()
		{
			var person = Guid.NewGuid();
			WithReadModelUnitOfWork.Do(() =>
			{
				AdherenceChange.Out(person, "2016-10-18 08:00".Utc());
				AdherenceChange.In(person, "2016-10-18 08:05".Utc());
				AdherenceChange.Out(person, "2016-10-18 09:00".Utc());
				AdherenceChange.Neutral(person, "2016-10-18 09:15".Utc());
			});

			var result = WithReadModelUnitOfWork.Get(() => Reader.Read(person, "2016-10-18 00:00".Utc(), "2016-10-19 00:00".Utc()));

			result.Select(x => x.Timestamp)
				.Should().Have
				.SameValuesAs("2016-10-18 08:00".Utc(), "2016-10-18 08:05".Utc(), "2016-10-18 09:00".Utc(), "2016-10-18 09:15".Utc());
		}

		[Test]
		public void ShouldReadWithProperties()
		{
			var personId = Guid.NewGuid();
			var state = Guid.NewGuid();
			WithReadModelUnitOfWork.Do(() =>
				AdherenceChange.Change(new HistoricalChange
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
					Adherence = HistoricalChangeAdherence.In
				})
			);

			var change = WithReadModelUnitOfWork.Get(() => Reader.Read(personId, "2017-03-07 00:00".Utc(), "2017-03-08 00:00".Utc()).Single());
			change.PersonId.Should().Be(personId);
			change.BelongsToDate.Should().Be("2017-03-07".Date());
			change.Timestamp.Should().Be("2017-03-07 10:00".Utc());
			change.Timestamp.Kind.Should().Be(DateTimeKind.Utc);
			change.StateName.Should().Be("ready");
			change.StateGroupId.Should().Be(state);
			change.ActivityName.Should().Be("phone");
			change.ActivityColor.Should().Be(Color.DarkGoldenrod.ToArgb());
			change.RuleName.Should().Be("in");
			change.RuleColor.Should().Be(Color.Azure.ToArgb());
			change.Adherence.Should().Be(HistoricalChangeAdherence.In);
		}

		[Test]
		public void ShouldReadWithTimestampAsUtc()
		{
			var person = Guid.NewGuid();
			WithReadModelUnitOfWork.Do(() => { AdherenceChange.Out(person, "2016-10-18 08:00".Utc()); });

			var result = WithReadModelUnitOfWork.Get(() => Reader.Read(person, "2016-10-18 00:00".Utc(), "2016-10-19 00:00".Utc()));

			result.Single().Timestamp.Kind.Should().Be(DateTimeKind.Utc);
		}

		[Test]
		public void ShouldReadFromYesterday()
		{
			var person = Guid.NewGuid();
			WithReadModelUnitOfWork.Do(() =>
			{
				AdherenceChange.Out(person, "2016-10-17 08:00".Utc());
				AdherenceChange.In(person, "2016-10-18 08:05".Utc());
			});

			var result = WithReadModelUnitOfWork.Get(() => Reader.ReadLastBefore(person, "2016-10-18 08:05".Utc()));

			result.Timestamp
				.Should().Be("2016-10-17 08:00".Utc());
		}

		[Test]
		public void ShouldReadLatestFromYesterday()
		{
			var person = Guid.NewGuid();
			WithReadModelUnitOfWork.Do(() =>
			{
				AdherenceChange.Neutral(person, "2016-10-17 07:59".Utc());
				AdherenceChange.Out(person, "2016-10-17 08:00".Utc());
				AdherenceChange.In(person, "2016-10-18 08:05".Utc());
			});

			var result = WithReadModelUnitOfWork.Get(() => Reader.ReadLastBefore(person, "2016-10-18 08:05".Utc()));

			result.Timestamp
				.Should().Be("2016-10-17 08:00".Utc());
		}
	}
}