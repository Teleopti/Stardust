using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
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
		public Database Database;
		public IHistoricalChangeReadModelReader Reader;
		public WithReadModelUnitOfWork WithReadModelUnitOfWork;

		[Test]
		public void ShouldRead()
		{
			Database.WithAgent();
			Database.WithAdherenceOut("2016-10-18 08:00");
			Database.WithAdherenceIn("2016-10-18 08:05");
			Database.WithAdherenceOut("2016-10-18 09:00");
			Database.WithAdherenceNeutral("2016-10-18 09:15");
			var personId = Database.CurrentPersonId();

			var result = WithReadModelUnitOfWork.Get(() => Reader.Read(personId, "2016-10-18 00:00".Utc(), "2016-10-19 00:00".Utc()));

			result.Select(x => x.Timestamp)
				.Should().Have
				.SameValuesAs("2016-10-18 08:00".Utc(), "2016-10-18 08:05".Utc(), "2016-10-18 09:00".Utc(), "2016-10-18 09:15".Utc());
		}

		[Test]
		public void ShouldReadWithProperties()
		{
			Database
				.WithAgent()
				.WithStateGroup("ready")
				.WithStateCode("ready")
				.WithActivity("phone", Color.DarkGoldenrod)
				.WithRule("in", 0, Adherence.In, Color.Azure)
				.WithHistoricalChange("2017-03-07 10:00");
			var personId = Database.CurrentPersonId();
			var state = Database.CurrentStateGroupId();

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
			Database
				.WithAgent()
				.WithAdherenceOut("2016-10-18 08:00");
			var personId = Database.CurrentPersonId();
			
			var result = WithReadModelUnitOfWork.Get(() => Reader.Read(personId, "2016-10-18 00:00".Utc(), "2016-10-19 00:00".Utc()));

			result.Single().Timestamp.Kind.Should().Be(DateTimeKind.Utc);
		}

		[Test]
		public void ShouldReadFromYesterday()
		{
			Database
				.WithAgent()
				.WithAdherenceOut("2016-10-17 08:00")
				.WithAdherenceIn("2016-10-18 08:05");
			var personId = Database.CurrentPersonId();
			
			var result = WithReadModelUnitOfWork.Get(() => Reader.ReadLastBefore(personId, "2016-10-18 08:05".Utc()));

			result.Timestamp
				.Should().Be("2016-10-17 08:00".Utc());
		}

		[Test]
		public void ShouldReadLatestFromYesterday()
		{
			Database
				.WithAgent()
				.WithAdherenceNeutral("2016-10-17 07:59")
				.WithAdherenceOut("2016-10-17 08:00")
				.WithAdherenceIn("2016-10-18 08:05");
			var personId = Database.CurrentPersonId();
			
			var result = WithReadModelUnitOfWork.Get(() => Reader.ReadLastBefore(personId, "2016-10-18 08:05".Utc()));

			result.Timestamp
				.Should().Be("2016-10-17 08:00".Utc());
		}
	}
}