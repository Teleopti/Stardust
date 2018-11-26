using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.TestCommon;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Test.InfrastructureTesting;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Infrastructure.AgentState
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
			var date = "2018-09-04".Date();
			
			Persister.Upsert(new HistoricalOverviewReadModel
			{
				PersonId = personId,
				Date = date,
				WasLateForWork = true,
				MinutesLateForWork = 5,
				SecondsInAdherence = 1,
				SecondsOutOfAdherence = 2
			});

			var result = Reader.Read(new[] {personId});
			result.Single().PersonId.Should().Be(personId);
			result.Single().Date.Should().Be(date);
			result.Single().WasLateForWork.Should().Be(true);
			result.Single().MinutesLateForWork.Should().Be(5);
			result.Single().SecondsInAdherence.Should().Be(1);
			result.Single().SecondsOutOfAdherence.Should().Be(2);
		}

		[Test]
		public void ShouldUpdateWithProperties()
		{
			var personId = Guid.NewGuid();
			var date = "2018-09-04".Date();
			
			Persister.Upsert(new HistoricalOverviewReadModel
			{
				PersonId = personId,
				Date = date,
				WasLateForWork = false,
				MinutesLateForWork = 0,
				SecondsInAdherence = 1,
				SecondsOutOfAdherence = 2
			});
			Persister.Upsert(new HistoricalOverviewReadModel
			{
				PersonId = personId,
				Date = date,
				WasLateForWork = true,
				MinutesLateForWork = 5,
				SecondsInAdherence = 3,
				SecondsOutOfAdherence = 4
			});

			var result = Reader.Read(new[] {personId});
			result.Single().WasLateForWork.Should().Be(true);
			result.Single().MinutesLateForWork.Should().Be(5);
			result.Single().SecondsInAdherence.Should().Be(3);
			result.Single().SecondsOutOfAdherence.Should().Be(4);
		}

		[Test]
		public void ShouldUpdateForPerson()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			
			Persister.Upsert(new HistoricalOverviewReadModel
			{
				PersonId = personId1,
				SecondsInAdherence = 1,
				SecondsOutOfAdherence = 2
			});
			Persister.Upsert(new HistoricalOverviewReadModel
			{
				PersonId = personId2,
				SecondsInAdherence = 3,
				SecondsOutOfAdherence = 4
			});
			Persister.Upsert(new HistoricalOverviewReadModel
			{
				PersonId = personId1,
				SecondsInAdherence = 5,
				SecondsOutOfAdherence = 6
			});

			var result = Reader.Read(new[] {personId1, personId2});
			result.Single(x => x.PersonId == personId2).SecondsInAdherence.Should().Be(3);
			result.Single(x => x.PersonId == personId2).SecondsOutOfAdherence.Should().Be(4);
			result.Single(x => x.PersonId == personId1).SecondsInAdherence.Should().Be(5);
			result.Single(x => x.PersonId == personId1).SecondsOutOfAdherence.Should().Be(6);
		}

		[Test]
		public void ShouldUpdateForDate()
		{
			var personId = Guid.NewGuid();
			var date1 = "2018-09-04".Date();
			var date2 = "2018-09-05".Date();
			
			Persister.Upsert(new HistoricalOverviewReadModel
			{
				PersonId = personId,
				Date = date1,
				SecondsInAdherence = 1,
				SecondsOutOfAdherence = 2
			});
			Persister.Upsert(new HistoricalOverviewReadModel
			{
				PersonId = personId,
				Date = date2,
				SecondsInAdherence = 3,
				SecondsOutOfAdherence = 4
			});
			Persister.Upsert(new HistoricalOverviewReadModel
			{
				PersonId = personId,
				Date = date1,
				SecondsInAdherence = 5,
				SecondsOutOfAdherence = 6
			});

			var result = Reader.Read(new[] {personId});
			result.Single(x => x.Date == date2).SecondsInAdherence.Should().Be(3);
			result.Single(x => x.Date == date2).SecondsOutOfAdherence.Should().Be(4);
			result.Single(x => x.Date == date1).SecondsInAdherence.Should().Be(5);
			result.Single(x => x.Date == date1).SecondsOutOfAdherence.Should().Be(6);
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