using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
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

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	[TestFixture]
	[Toggle(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
	public class HistoricalAdherenceViewModelBuilderUserTimeZoneTest : ISetup
	{
		public HistoricalAdherenceViewModelBuilder Target;
		public FakeDatabase Database;
		public FakeHistoricalAdherenceReadModelPersister ReadModel;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;

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

			var historicalData = Target.Build(person);

			historicalData.Now.Should().Be("2016-10-10T17:00:00");
		}

		[Test]
		public void ShouldGetHistoricalDataForAgentWhenUserInStockholm()
		{
			TimeZone.IsSweden();
			Now.Is("2016-10-10 15:00");
			var person = Guid.NewGuid();
			ReadModel.Has(new HistoricalAdherenceReadModel
			{
				PersonId = person,
				OutOfAdherences = new[]
				{
					new HistoricalOutOfAdherenceReadModel
					{
						StartTime = "2016-10-10 06:05".Utc(),
						EndTime = "2016-10-10 06:15".Utc()
					}
				}
			});

			var historicalData = Target.Build(person);

			historicalData.OutOfAdherences.Single().StartTime.Should().Be("2016-10-10T08:05:00");
			historicalData.OutOfAdherences.Single().EndTime.Should().Be("2016-10-10T08:15:00");
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

			var historicalData = Target.Build(person);

			historicalData.Schedules.Single().Color.Should().Be("#80FF80");
			historicalData.Schedules.Single().StartTime.Should().Be("2016-10-11T09:00:00");
			historicalData.Schedules.Single().EndTime.Should().Be("2016-10-11T17:00:00");
		}

		[Test]
		public void ShouldGetOutOfAdherenceWhenUserInStockholm()
		{
			TimeZone.IsSweden();
			Now.Is("2016-10-12 12:00");
			var person = Guid.NewGuid();
			Database.WithAgent(person, "nicklas");
			ReadModel.Has(new HistoricalAdherenceReadModel
			{
				PersonId = person,
				OutOfAdherences = new[]
				{
					new HistoricalOutOfAdherenceReadModel
					{
						StartTime = "2016-10-12 14:00".Utc(),
						EndTime = "2016-10-12 15:00".Utc(),
					}
				}
			});

			var data = Target.Build(person);

			data.OutOfAdherences.Single().StartTime.Should().Be("2016-10-12T16:00:00");
		}

	}
}