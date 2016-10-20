using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.TimeLayer;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Scheduling.Assignment
{
	[TestFixture]
	public class ProjectionMidnightSplitterMergerTest
	{
		private IProjectionMerger target;
		private IActivity activity;
		private IPerson person;
		private IVisualLayerFactory visualLayerFactory;
		private TimeSpan tzDiffTime;

		[SetUp]
		public void Setup()
		{
			var userZone = StateHolderReader.Instance.StateReader.UserTimeZone;
			tzDiffTime = userZone.BaseUtcOffset;
			target = new ProjectionMidnightSplitterMerger(userZone);
            activity = ActivityFactory.CreateActivity("f");
            person = PersonFactory.CreatePerson();
			visualLayerFactory = new VisualLayerFactory();
		}

		[Test]
		public void ShouldSplitOverMidnight()
		{
			var start = new DateTime(2000, 1, 1, 20, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 2, 7, 0, 0, DateTimeKind.Utc);
			var midnightInUtc = new DateTime(2000, 1, 2, 0, 0, 0, DateTimeKind.Utc);

			var layers = new IVisualLayer[]
			             	{
			             		createLayer(new DateTimePeriod(start, end)),
			             	};
			var res = target.MergedCollection(layers, person);
			res.Length.Should().Be.EqualTo(2);
			var period1 = res.First().Period;
			var period2 = res.Last().Period;

			period1.Should().Be.EqualTo(new DateTimePeriod(start, midnightInUtc.Add(-tzDiffTime)));
			period2.Should().Be.EqualTo(new DateTimePeriod(midnightInUtc.Add(-tzDiffTime), end));
		}

		[Test]
		public void ShouldKeepDefinitionSetWhenSplit()
		{
			var start = new DateTime(2000, 1, 1, 20, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 2, 7, 0, 0, DateTimeKind.Utc);
			var layer = createLayer(new DateTimePeriod(start, end));
			layer.DefinitionSet = new MultiplicatorDefinitionSet("sdf", MultiplicatorType.OBTime);
			var layers = new IVisualLayer[] { layer };

			var res = target.MergedCollection(layers, person);
			res.Length.Should().Be.EqualTo(2);
			foreach (var visualLayer in res)
			{
				visualLayer.DefinitionSet
					.Should().Be.SameInstanceAs(layer.DefinitionSet);
			}
		}

        [Test]
        public void ShouldKeepPersonWhenSplit()
        {
            var start = new DateTime(2000, 1, 1, 20, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2000, 1, 2, 7, 0, 0, DateTimeKind.Utc);
            var layer = createLayer(new DateTimePeriod(start, end));

            var layers = new IVisualLayer[] { layer };

            var res = target.MergedCollection(layers, person);
            res.Length.Should().Be.EqualTo(2);
            foreach (VisualLayer visualLayer in res)
            {
                visualLayer.Person.Should().Not.Be.Null();
                visualLayer.Person
                    .Should().Be.SameInstanceAs(person);
            }
        }

		[Test]
		public void ShouldKeepAbsenceWhenSplit()
		{
			var start = new DateTime(2000, 1, 1, 20, 0, 0, DateTimeKind.Utc);
			var end = new DateTime(2000, 1, 2, 7, 0, 0, DateTimeKind.Utc);
			var layer = createLayer(new DateTimePeriod(start, end));
			layer.HighestPriorityAbsence = new Absence();
			var layers = new IVisualLayer[] { layer };

			var res = target.MergedCollection(layers, person);
			res.Length.Should().Be.EqualTo(2);
			foreach (VisualLayer visualLayer in res)
			{
				visualLayer.HighestPriorityAbsence
					.Should().Be.SameInstanceAs(layer.HighestPriorityAbsence);
			}
		}

        [Test]
        public void ShouldSplitMidnightUsingPassedInTimeZone()
        {
            var userDefinedTimeZone = (TimeZoneInfo.FindSystemTimeZoneById("Jordan Standard Time"));
            target = new ProjectionMidnightSplitterMerger(userDefinedTimeZone);
            var start = new DateTime(2000, 1, 1, 10, 0, 0, DateTimeKind.Utc);
            var end = new DateTime(2000, 1, 2, 10, 0, 0, DateTimeKind.Utc);
            var layers = new IVisualLayer[]
			             	{
			             		createLayer(new DateTimePeriod(start, end)),
			             	};

            var res = target.MergedCollection(layers, person);

            var expected = new DateTime(2000, 1, 2, 0, 0, 0, DateTimeKind.Utc).Add(-userDefinedTimeZone.BaseUtcOffset);
            res[0].Period.EndDateTime
                .Should().Be.EqualTo(expected);
        }

		private VisualLayer createLayer(DateTimePeriod period)
		{
			return (VisualLayer)visualLayerFactory.CreateShiftSetupLayer(activity, period, person);
		}

	}
}