using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.AgentAdherenceDay;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels.HistoricalAdherenceViewModelBuilder
{
	[DomainTest]
	[DefaultData]
	[TestFixture]
	public class AdherencesTest
	{
		public Domain.ApplicationLayer.Rta.ViewModels.HistoricalAdherenceViewModelBuilder Target;
		public FakeDatabase Database;
		public FakeHistoricalAdherenceReadModelPersister ReadModel;
		public MutableNow Now;

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

			var data = Target.Build(person);

			data.OutOfAdherences.Single().StartTime.Should().Be("2016-10-10T08:05:00");
			data.OutOfAdherences.Single().EndTime.Should().Be("2016-10-10T08:15:00");
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

			var viewModel = Target.Build(person);

			viewModel.OutOfAdherences.Single().StartTime.Should().Be("2016-10-12T08:00:00");
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

			var viewModel = Target.Build(person);

			viewModel.OutOfAdherences.Single().StartTime.Should().Be("2016-10-11T09:00:00");
		}

		[Test]
		public void ShouldReadOutOfAdherenceStartedOneDayAgo2()
		{
			Now.Is("2016-10-12 12:00");
			var person = Guid.NewGuid();

			Database.WithAgent(person, "nicklas");
			ReadModel.Has(new[]
			{
				new HistoricalAdherence
				{
					PersonId = person,
					Adherence = HistoricalAdherenceAdherence.Out,
					Timestamp = "2016-10-11 09:00".Utc()
				},
				new HistoricalAdherence
				{
					PersonId = person,
					Adherence = HistoricalAdherenceAdherence.In,
					Timestamp = "2016-10-12 11:00".Utc()
				}
			});

			var viewModel = Target.Build(person);

			viewModel.OutOfAdherences.Single().StartTime.Should().Be("2016-10-11T09:00:00");
			viewModel.OutOfAdherences.Single().EndTime.Should().Be("2016-10-12T11:00:00");
		}
	}
}