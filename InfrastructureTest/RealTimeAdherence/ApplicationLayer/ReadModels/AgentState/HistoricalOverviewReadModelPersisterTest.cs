using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;

namespace Teleopti.Ccc.InfrastructureTest.RealTimeAdherence.ApplicationLayer.ReadModels.AgentState
{
	[TestFixture]
	[UnitOfWorkTest]
	public class HistoricalOverviewReadModelPersisterTest
	{
		public IHistoricalOverviewReadModelReader Reader;
		public IHistoricalOverviewReadModelPersister Persister;

		[Test]
		public void ShouldPersist()
		{
			var personId = Guid.NewGuid();
			
			Persister.Upsert(new HistoricalOverviewReadModel {PersonId = personId});

			var result = Reader.Read(new[] {personId});
			result.Single().PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldUpdate()
		{
			var personId = Guid.NewGuid();
			
			Persister.Upsert(new HistoricalOverviewReadModel {PersonId = personId});
			Persister.Upsert(new HistoricalOverviewReadModel {PersonId = personId});

			var result = Reader.Read(new[] {personId});
			result.Count().Should().Be(1);
		}

		[Test]
		public void ShouldPersistWithProperties()
		{
			var personId = Guid.NewGuid();
			var date = "2018-09-04".Utc().ToDateOnly();
			
			Persister.Upsert(new HistoricalOverviewReadModel
			{
				PersonId = personId,
				Date = date,
				Adherence = 90,
				WasLateForWork = true,
				MinutesLateForWork = 5
			});

			var result = Reader.Read(new[] {personId});
			result.Single().PersonId.Should().Be(personId);
			result.Single().Date.Should().Be(date);
			result.Single().Adherence.Should().Be(90);
			result.Single().WasLateForWork.Should().Be(true);
			result.Single().MinutesLateForWork.Should().Be(5);
		}

		[Test]
		public void ShouldUpdateWithProperties()
		{
			var personId = Guid.NewGuid();
			var date = "2018-09-04".Utc().ToDateOnly();
			
			Persister.Upsert(new HistoricalOverviewReadModel
			{
				PersonId = personId,
				Date = date,
				Adherence = 90,
				WasLateForWork = false,
				MinutesLateForWork = 0,
			});
			Persister.Upsert(new HistoricalOverviewReadModel
			{
				PersonId = personId,
				Date = date,
				Adherence = 98,
				WasLateForWork = true,
				MinutesLateForWork = 5
			});

			var result = Reader.Read(new[] {personId});
			result.Single().Adherence.Should().Be(98);
			result.Single().WasLateForWork.Should().Be(true);
			result.Single().MinutesLateForWork.Should().Be(5);
		}

		[Test]
		public void ShouldUpdateForPerson()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			
			Persister.Upsert(new HistoricalOverviewReadModel
			{
				PersonId = personId1,
				Adherence = 1
			});
			Persister.Upsert(new HistoricalOverviewReadModel
			{
				PersonId = personId2,
				Adherence = 2
			});
			Persister.Upsert(new HistoricalOverviewReadModel
			{
				PersonId = personId1,
				Adherence = 3
			});

			var result = Reader.Read(new[] {personId1, personId2});
			result.Single(x => x.PersonId == personId2).Adherence.Should().Be(2);
			result.Single(x => x.PersonId == personId1).Adherence.Should().Be(3);
		}

		[Test]
		public void ShouldUpdateForDate()
		{
			var personId = Guid.NewGuid();
			var date1 = "2018-09-04".Utc().ToDateOnly();
			var date2 = "2018-09-05".Utc().ToDateOnly();
			
			Persister.Upsert(new HistoricalOverviewReadModel
			{
				PersonId = personId,
				Date = date1,
				Adherence = 1
			});
			Persister.Upsert(new HistoricalOverviewReadModel
			{
				PersonId = personId,
				Date = date2,
				Adherence = 2
			});
			Persister.Upsert(new HistoricalOverviewReadModel
			{
				PersonId = personId,
				Date = date1,
				Adherence = 3
			});

			var result = Reader.Read(new[] {personId});
			result.Single(x => x.Date == date2).Adherence.Should().Be(2);
			result.Single(x => x.Date == date1).Adherence.Should().Be(3);
		}
		
		[Test]
		public void ShouldRead()
		{
			var personId = Guid.NewGuid();
			
			Persister.Upsert(new HistoricalOverviewReadModel {PersonId = personId});
			Persister.Upsert(new HistoricalOverviewReadModel {PersonId = Guid.NewGuid()});

			var result = Reader.Read(new[] {personId});
			result.Single().PersonId.Should().Be(personId);
		}
	}
}