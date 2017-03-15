using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	[TestFixture]
	[Toggle(Toggles.RTA_SolidProofWhenManagingAgentAdherence_39351)]
	public class HistoricalAdherenceViewModelBuilderChangesTest : ISetup
	{
		public HistoricalAdherenceViewModelBuilder Target;
		public FakeHistoricalChangeReadModelPersister ReadModel;
		public FakeDatabase Database;
		public MutableNow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
		}

		[Test]
		public void ShouldGetByPersonId()
		{
			Now.Is("2017-03-07 14:00");
			var state = Guid.NewGuid();
			var person = Guid.NewGuid();
			var name = RandomName.Make();
			Database.WithAgent(person, name);

			ReadModel.Persist(new HistoricalChangeReadModel
			{
				PersonId = person,
				BelongsToDate = "2017-03-07".Date(),
				Timestamp = "2017-03-07 14:00".Utc(),
				StateName = "InCall",
				StateGroupId = state,
				ActivityName = "phone",
				ActivityColor = Color.Crimson.ToArgb(),
				RuleName = "in",
				RuleColor = Color.DarkKhaki.ToArgb(),
				Adherence = HistoricalChangeInternalAdherence.In
			});

			var result = Target.Build(person).Changes.Single();
			result.Time.Should().Be("14:00:00");
			result.Activity.Should().Be("phone");
			result.ActivityColor.Should().Be(ColorTranslator.ToHtml(Color.FromArgb(Color.Crimson.ToArgb())));
			result.State.Should().Be("InCall");
			result.Rule.Should().Be("in");
			result.RuleColor.Should().Be(ColorTranslator.ToHtml(Color.FromArgb(Color.DarkKhaki.ToArgb())));
			result.Adherence.Should().Be(UserTexts.Resources.InAdherence);
			result.AdherenceColor.Should().Be(ColorTranslator.ToHtml(Color.FromArgb(Color.DarkOliveGreen.ToArgb())));
		}

		[Test]
		public void ShouldHandleNulls()
		{
			Now.Is("2017-03-07 14:00");
			var person = Guid.NewGuid();
			var name = RandomName.Make();
			Database.WithAgent(person, name);

			ReadModel.Persist(new HistoricalChangeReadModel
			{
				PersonId = person,
				Timestamp = "2017-03-07 14:00".Utc()
			});

			var result = Target.Build(person).Changes.Single();
			result.Activity.Should().Be(null);
			result.ActivityColor.Should().Be(null);
			result.State.Should().Be(null);
			result.Rule.Should().Be(null);
			result.RuleColor.Should().Be(null);
			result.Adherence.Should().Be(null);
			result.AdherenceColor.Should().Be(null);
		}


		[Test]
		public void ShouldGetForCorrectDate()
		{
			Now.Is("2017-03-07 15:00");
			var person = Guid.NewGuid();

			Database
				.WithAgent(person, "name");

			ReadModel
				.Persist(new HistoricalChangeReadModel
				{
					PersonId = person,
					Timestamp = "2017-03-06 14:00".Utc()
				});
			ReadModel.Persist(new HistoricalChangeReadModel
			{
				PersonId = person,
				Timestamp = "2017-03-07 14:00".Utc()
			});

			var historicalData = Target.Build(person).Changes.Single();
			historicalData.Time.Should().Be("14:00:00");
		}

		[Test]
		public void ShouldGetChangesForAgentInChina()
		{
			Now.Is("2017-03-07 12:00");
			var person = Guid.NewGuid();
			Database.WithAgent(person, "nicklas", TimeZoneInfoFactory.ChinaTimeZoneInfo());
			ReadModel.Persist(new HistoricalChangeReadModel
			{
				PersonId = person,
				Timestamp = "2017-03-06 14:00".Utc()
			});

			var data = Target.Build(person);

			data.Changes.Single().Time.Should().Be("14:00:00");

			// "2017-03-07 12:00" utc
			// "2017-03-07 20:00" +8
			// "2017-03-07" agents date
			// "2017-03-07 00:00 - 2017-03-08 00:00" +8
			// "2017-03-06 16:00 - 2017-03-07 16:00" utc
			// "2017-03-06 14:00 - 2017-03-08 16:00" +extra
		}

		[Test]
		public void ShouldGetChangesForAgentInHawaii()
		{
			Now.Is("2017-03-07 09:00");
			var person = Guid.NewGuid();
			Database.WithAgent(person, "nicklas", TimeZoneInfoFactory.HawaiiTimeZoneInfo());
			ReadModel.Persist(new HistoricalChangeReadModel
			{
				PersonId = person,
				Timestamp = "2017-03-06 08:00".Utc()
			});

			var data = Target.Build(person);

			data.Changes.Single().Time.Should().Be("08:00:00");

			// "2017-03-07 09:00" utc
			// "2017-03-06 23:00" -10
			// "2017-03-06" agents date
			// "2017-03-06 00:00 - 2017-03-07 00:00" -10
			// "2017-03-06 10:00 - 2017-03-07 10:00" utc
			// "2017-03-06 08:00 - 2017-03-08 10:00" +extra
		}

		[Test]
		public void ShouldExcludeChangesEarlierThanOneHourBeforeShiftStart()
		{
			Now.Is("2017-03-14 09:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person, "nicklas")
				.WithSchedule(person, "2017-03-14 09:00", "2017-03-14 17:00");
			ReadModel
				.Persist(new HistoricalChangeReadModel
				{
					PersonId = person,
					Timestamp = "2017-03-14 07:59".Utc()
				});
			ReadModel.Persist(new HistoricalChangeReadModel
			{
				PersonId = person,
				Timestamp = "2017-03-14 08:00".Utc()
			});

			var data = Target.Build(person);

			data.Changes.Single().Time.Should().Be("08:00:00");
		}

		[Test]
		public void ShouldExcludeChangesLaterThanOneHourAfterShiftEnd()
		{
			Now.Is("2017-03-14 18:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person, "nicklas")
				.WithSchedule(person, "2017-03-14 09:00", "2017-03-14 17:00");
			ReadModel
				.Persist(new HistoricalChangeReadModel
				{
					PersonId = person,
					Timestamp = "2017-03-14 18:00".Utc()
				});
			ReadModel.Persist(new HistoricalChangeReadModel
			{
				PersonId = person,
				Timestamp = "2017-03-14 18:01".Utc()
			});

			var data = Target.Build(person);

			data.Changes.Single().Time.Should().Be("18:00:00");
		}
	}
}