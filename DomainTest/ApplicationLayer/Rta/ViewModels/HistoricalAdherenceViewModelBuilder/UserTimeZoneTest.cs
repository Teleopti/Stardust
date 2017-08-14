using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels.HistoricalAdherenceViewModelBuilder
{
	[DomainTest]
	[TestFixture]
	[Toggle(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
	public class UserTimeZoneTest : ISetup
	{
		public Domain.ApplicationLayer.Rta.ViewModels.HistoricalAdherenceViewModelBuilder Target;
		public FakeDatabase Database;
		public FakeHistoricalAdherenceReadModelPersister Adherences;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		public FakeHistoricalChangeReadModelPersister Changes;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeUserTimeZone>().For<IUserTimeZone>();
		}

		[Test]
		public void ShouldGetNowWhenUserInStockholm()
		{
			TimeZone.IsSweden();
			Now.Is("2016-10-10 15:00");
			var person = Guid.NewGuid();

			Database
				.WithAgent(person, "name");

			var data = Target.Build(person);

			data.Now.Should().Be("2016-10-10T17:00:00");
		}

		[Test]
		public void ShouldGetAdherenceDataForAgentWhenUserInStockholm()
		{
			TimeZone.IsSweden();
			Now.Is("2016-10-10 15:00");
			var person = Guid.NewGuid();
			Adherences.Has(person, new[]
				{
					new HistoricalOutOfAdherenceReadModel
					{
						StartTime = "2016-10-10 06:05".Utc(),
						EndTime = "2016-10-10 06:15".Utc()
					}
				});

			var data = Target.Build(person);

			data.OutOfAdherences.Single().StartTime.Should().Be("2016-10-10T08:05:00");
			data.OutOfAdherences.Single().EndTime.Should().Be("2016-10-10T08:15:00");
		}

		[Test]
		public void ShouldGetScheduleWhenUserInStockholm()
		{
			TimeZone.IsSweden();
			Now.Is("2016-10-11 09:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person, "name")
				.WithAssignment(person, "2016-10-11")
				.WithActivity(null, ColorTranslator.FromHtml("#80FF80"))
				.WithAssignedActivity("2016-10-11 07:00", "2016-10-11 15:00");

			var data = Target.Build(person);

			data.Schedules.Single().Color.Should().Be("#80FF80");
			data.Schedules.Single().StartTime.Should().Be("2016-10-11T09:00:00");
			data.Schedules.Single().EndTime.Should().Be("2016-10-11T17:00:00");
		}

		[Test]
		public void ShouldGetOutOfAdherenceWhenUserInStockholm()
		{
			TimeZone.IsSweden();
			Now.Is("2016-10-12 12:00");
			var person = Guid.NewGuid();
			Database.WithAgent(person, "nicklas");
			Adherences.Has(person, new[]
			{
				new HistoricalOutOfAdherenceReadModel
				{
					StartTime = "2016-10-12 14:00".Utc(),
					EndTime = "2016-10-12 15:00".Utc()
				}
			});

			var data = Target.Build(person);

			data.OutOfAdherences.Single().StartTime.Should().Be("2016-10-12T16:00:00");
		}

		[Test]
		public void ShouldGetChangeDataWhenUserInStockholm()
		{
			TimeZone.IsSweden();
			Now.Is("2017-04-18 08:20");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person, "nicklas")
				.WithAssignment(person, "2017-04-18")
				.WithActivity(null, ColorTranslator.FromHtml("#80FF80"))
				.WithAssignedActivity("2017-04-18 08:00", "2017-04-18 09:00"); ;
			Changes.Persist( new HistoricalChangeReadModel
			{
				PersonId = person,
				BelongsToDate = "2017-04-18".Date(),
				Timestamp = "2017-04-18 08:20".Utc()
			});

			var data = Target.Build(person);

			data.Changes.Single().Should().Not.Be.Null();
		}

		[Test]
		public void ShouldIncludeTimelineEndpointsWithoutScheduleWhenUserInStockholm()
		{
			TimeZone.IsSweden();
			Now.Is("2017-04-25 09:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person, "name");

			var viewModel = Target.Build(person);

			viewModel.Timeline.StartTime.Should().Be("2017-04-25T02:00:00");
			viewModel.Timeline.EndTime.Should().Be("2017-04-26T02:00:00");
		}

	}
}