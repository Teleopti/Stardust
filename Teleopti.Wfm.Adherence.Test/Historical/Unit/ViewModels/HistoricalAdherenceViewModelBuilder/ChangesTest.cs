using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Wfm.Adherence.Test.Historical.Unit.ViewModels.HistoricalAdherenceViewModelBuilder
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class ChangesTest
	{
		public Adherence.ApplicationLayer.ViewModels.HistoricalAdherenceViewModelBuilder Target;
		public FakeDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldGetByPersonId()
		{
			Now.Is("2017-03-07 14:00");
			var personId = Guid.NewGuid();
			var name = RandomName.Make();
			Database
				.WithAgent(personId, name)
				.WithStateGroup(null, "InCall")
				.WithStateCode("InCall")
				.WithActivity(null, "phone", Color.Crimson)
				.WithRule(null, "in", 0, Domain.Configuration.Adherence.In, Color.DarkKhaki)
				.WithHistoricalStateChange("2017-03-07 14:00")
				;

			var result = Target.Build(personId).Changes.Single();

			result.Time.Should().Be("2017-03-07T14:00:00");
			result.Activity.Should().Be("phone");
			result.ActivityColor.Should().Be(ColorTranslator.ToHtml(Color.FromArgb(Color.Crimson.ToArgb())));
			result.State.Should().Be("InCall");
			result.Rule.Should().Be("in");
			result.RuleColor.Should().Be(ColorTranslator.ToHtml(Color.FromArgb(Color.DarkKhaki.ToArgb())));
			result.Adherence.Should().Be("InAdherence");
			result.AdherenceColor.Should().Be(ColorTranslator.ToHtml(Color.FromArgb(Color.DarkOliveGreen.ToArgb())));
		}

		[Test]
		public void ShouldHandleNulls()
		{
			Now.Is("2017-03-07 14:00");
			var personId = Guid.NewGuid();
			var name = RandomName.Make();
			Database
				.WithAgent(personId, name)
				.WithHistoricalStateChange("2017-03-07 14:00");

			var result = Target.Build(personId).Changes.Single();

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
			var personId = Guid.NewGuid();
			Database
				.WithAgent(personId, "name")
				.WithHistoricalStateChange("2017-03-06 14:00")
				.WithHistoricalStateChange("2017-03-07 14:00");

			var historicalData = Target.Build(personId).Changes.Single();

			historicalData.Time.Should().Be("2017-03-07T14:00:00");
		}

		[Test]
		public void ShouldExcludeChangesEarlierThanOneHourBeforeShiftStart()
		{
			Now.Is("2017-03-14 09:00");
			var personId = Guid.NewGuid();
			Database
				.WithAgent(personId, "nicklas")
				.WithSchedule(personId, "2017-03-14 09:00", "2017-03-14 17:00")
				.WithHistoricalStateChange("2017-03-14 07:59")
				.WithHistoricalStateChange("2017-03-14 08:00");

			var data = Target.Build(personId);

			data.Changes.Single().Time.Should().Be("2017-03-14T08:00:00");
		}

		[Test]
		public void ShouldExcludeChangesLaterThanOneHourAfterShiftEnd()
		{
			Now.Is("2017-03-14 18:00");
			var personId = Guid.NewGuid();
			Database
				.WithAgent(personId, "nicklas")
				.WithSchedule(personId, "2017-03-14 09:00", "2017-03-14 17:00")
				.WithHistoricalStateChange("2017-03-14 18:00")
				.WithHistoricalStateChange("2017-03-14 18:01");

			var data = Target.Build(personId);

			data.Changes.Single().Time.Should().Be("2017-03-14T18:00:00");
		}

		[Test]
		public void ShouldNotGetDuplicateByPersonId()
		{
			Now.Is("2017-03-07 14:00");
			var state = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var name = RandomName.Make();
			Database.WithAgent(personId, name);

			Database
				.WithAgent(personId, name)
				.WithStateGroup(state, "InCall")
				.WithStateCode("InCall")
				.WithActivity(null, "phone", Color.Crimson)
				.WithRule(null, "in", 0, Domain.Configuration.Adherence.In, Color.DarkKhaki)
				.WithHistoricalStateChange("2017-03-07 14:00")
				.WithHistoricalStateChange("2017-03-07 14:00")
				.WithHistoricalStateChange("2017-03-07 14:00")
				.WithHistoricalStateChange("2017-03-07 14:00")
				;

			var result = Target.Build(personId).Changes.Single();

			result.Time.Should().Be("2017-03-07T14:00:00");
			result.Activity.Should().Be("phone");
			result.ActivityColor.Should().Be(ColorTranslator.ToHtml(Color.FromArgb(Color.Crimson.ToArgb())));
			result.State.Should().Be("InCall");
			result.Rule.Should().Be("in");
			result.RuleColor.Should().Be(ColorTranslator.ToHtml(Color.FromArgb(Color.DarkKhaki.ToArgb())));
			result.Adherence.Should().Be("InAdherence");
			result.AdherenceColor.Should().Be(ColorTranslator.ToHtml(Color.FromArgb(Color.DarkOliveGreen.ToArgb())));
		}
	}
}