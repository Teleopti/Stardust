using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
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
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	[TestFixture]
	[Toggle(Toggles.RTA_SeeAllOutOfAdherencesToday_39146)]
	public class HistoricalAdherenceViewModelBuilderTest : ISetup
	{
		public HistoricalAdherenceViewModelBuilder Target;
		public FakeDatabase Database;
		public FakeHistoricalAdherenceReadModelPersister ReadModel;
		public MutableNow Now;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
		}

		[Test]
		public void ShouldGetPerson()
		{
			Now.Is("2016-10-10 15:00");
			var person = Guid.NewGuid();
			var name = RandomName.Make();
			Database.WithAgent(person, name);

			var historicalData = Target.Build(person);

			historicalData.PersonId.Should().Be(person);
			historicalData.AgentName.Should().Be(name);
		}

		[Test]
		public void ShouldGetHistoricalDataForAgent()
		{
			Now.Is("2016-10-10 15:00");
			var person = Guid.NewGuid();
			ReadModel.Has(person, new[]
			{
				new HistoricalOutOfAdherenceReadModel
				{
					StartTime = "2016-10-10 08:05".Utc(),
					EndTime = "2016-10-10 08:15".Utc()
				}
			});

			var historicalData = Target.Build(person);

			historicalData.OutOfAdherences.Single().StartTime.Should().Be("2016-10-10T08:05:00");
			historicalData.OutOfAdherences.Single().EndTime.Should().Be("2016-10-10T08:15:00");
		}

		[Test]
		public void ShouldGetForCorrectDate()
		{
			Now.Is("2016-10-10 15:00");
			var person = Guid.NewGuid();

			Database
				.WithAgent(person, "name");

			ReadModel
				.Has(person, new[] { new HistoricalOutOfAdherenceReadModel { StartTime = "2016-10-08 00:00".Utc() } })
				.Has(person, new[] { new HistoricalOutOfAdherenceReadModel { StartTime = "2016-10-10 00:00".Utc() } });

			var historicalData = Target.Build(person);

			historicalData.PersonId.Should().Be(person);
			historicalData.Now.Should().Be("2016-10-10T15:00:00");
			historicalData.OutOfAdherences.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldGetSchedule()
		{
			Now.Is("2016-10-11 09:00");
			var person = Guid.NewGuid();
			Database
				.WithAgent(person, "name")
				.WithAssignment(person, "2016-10-11")
				.WithActivity(null, "phone", ColorTranslator.FromHtml("#80FF80"))
				.WithAssignedActivity("2016-10-11 09:00", "2016-10-11 17:00");

			var historicalData = Target.Build(person);

			historicalData.Schedules.Single().Name.Should().Be("phone");
			historicalData.Schedules.Single().Color.Should().Be("#80FF80");
			historicalData.Schedules.Single().StartTime.Should().Be("2016-10-11T09:00:00");
			historicalData.Schedules.Single().EndTime.Should().Be("2016-10-11T17:00:00");
		}

		[Test]
		public void ShouldGetOutOfAdherencesForAgentInChina()
		{
			Now.Is("2016-10-12 12:00");
			var person = Guid.NewGuid();
			Database.WithAgent(person, "nicklas", TimeZoneInfoFactory.ChinaTimeZoneInfo());
			ReadModel.Has(person, new[]
				{
					new HistoricalOutOfAdherenceReadModel
					{
						StartTime = "2016-10-11 16:00".Utc(),
						EndTime = "2016-10-11 17:00".Utc(),
					}
				});

			var data = Target.Build(person);

			data.OutOfAdherences.Single().StartTime.Should().Be("2016-10-11T16:00:00");

			// "2016-10-12 12:00" utc
			// "2016-10-12 20:00" +8
			// "2016-10-12" agents date
			// "2016-10-12 00:00 - 2016-10-13 00:00" +8
			// "2016-10-11 16:00 - 2016-10-12 16:00" utc
		}

		[Test]
		public void ShouldGetOutOfAdherencesForAgentInHawaii()
		{
			Now.Is("2016-10-12 09:00");
			var person = Guid.NewGuid();
			Database.WithAgent(person, "nicklas", TimeZoneInfoFactory.HawaiiTimeZoneInfo());
			ReadModel.Has(person, new[]
				{
					new HistoricalOutOfAdherenceReadModel
					{
						StartTime = "2016-10-11 10:00".Utc(),
						EndTime = "2016-10-11 11:00".Utc()
					}
				});

			var data = Target.Build(person);

			data.OutOfAdherences.Single().StartTime.Should().Be("2016-10-11T10:00:00");

			// "2016-10-12 09:00" utc
			// "2016-10-11 23:00" -10
			// "2016-10-11" agents date
			// "2016-10-11 00:00 - 2016-10-12 00:00" -10
			// "2016-10-11 10:00 - 2016-10-12 10:00" utc
		}

		[Test]
		public void ShouldGetScheduleForAgentInChina()
		{
			Now.Is("2016-10-12 12:00");
			var person = Guid.NewGuid();

			Database
				.WithAgent(person, "nicklas", TimeZoneInfoFactory.ChinaTimeZoneInfo())
				.WithAssignment(person, "2016-10-11")
				.WithActivity()
				.WithAssignedActivity("2016-10-11 00:00", "2016-10-11 09:00")
				.WithAssignment(person, "2016-10-12")
				.WithActivity()
				.WithAssignedActivity("2016-10-12 00:00", "2016-10-12 09:00")
				.WithAssignment(person, "2016-10-13")
				.WithActivity()
				.WithAssignedActivity("2016-10-13 00:00", "2016-10-13 09:00");

			var data = Target.Build(person);

			data.Schedules.Single().StartTime.Should().Be("2016-10-12T00:00:00");
		}

		[Test]
		public void ShouldNotCreateMultipleOutOfAdherencesWithoutEndTime()
		{
			Now.Is("2016-10-12 12:00");
			var person = Guid.NewGuid();

			Database.WithAgent(person, "nicklas");
			ReadModel.Has(person, new[]
				{
					new HistoricalOutOfAdherenceReadModel
					{
						StartTime = "2016-10-12 08:00".Utc()
					},
					new HistoricalOutOfAdherenceReadModel
					{
						StartTime = "2016-10-12 09:00".Utc()
					}
				});

			var data = Target.Build(person);

			data.OutOfAdherences.Single().StartTime.Should().Be("2016-10-12T08:00:00");
		}

		[Test]
		public void ShouldReadOutOfAdherenceStartedOneDayAgo()
		{
			Now.Is("2016-10-12 12:00");
			var person = Guid.NewGuid();

			Database.WithAgent(person, "nicklas");
			ReadModel.Has(person, new[]
				{
					new HistoricalOutOfAdherenceReadModel
					{
						StartTime = "2016-10-11 09:00".Utc()
					}
				});

			var data = Target.Build(person);

			data.OutOfAdherences.Single().StartTime.Should().Be("2016-10-11T09:00:00");
		}

		[Test]
		public void ShouldReadOutOfAdherenceStartedOneDayAgo2()
		{
			Now.Is("2016-10-12 12:00");
			var person = Guid.NewGuid();

			Database.WithAgent(person, "nicklas");
			ReadModel.Has(new[]
			{
				new HistoricalAdherenceReadModel
				{
					PersonId = person,
					Adherence = HistoricalAdherenceReadModelAdherence.Out,
					Timestamp = "2016-10-11 09:00".Utc()
				},
				new HistoricalAdherenceReadModel
				{
					PersonId = person,
					Adherence = HistoricalAdherenceReadModelAdherence.In,
					Timestamp = "2016-10-12 11:00".Utc()
				}
			});

			var data = Target.Build(person);

			data.OutOfAdherences.Single().StartTime.Should().Be("2016-10-11T09:00:00");
			data.OutOfAdherences.Single().EndTime.Should().Be("2016-10-12T11:00:00");
		}
	}
}