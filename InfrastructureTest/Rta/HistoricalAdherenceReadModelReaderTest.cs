using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.InfrastructureTest.Rta
{
	[TestFixture]
	[DatabaseTest]
	public class HistoricalAdherenceReadModelReaderTest
	{
		public Database Database;
		public IHistoricalAdherenceReadModelPersister Persister;
		public IHistoricalAdherenceReadModelReader Reader;
		public IPersonRepository Persons;
		public WithUnitOfWork WithUnitOfWork;
		public WithReadModelUnitOfWork WithReadModelUnitOfWork;

		[Test]
		public void ShouldReadOutOfAdherences()
		{
			var person = Guid.NewGuid();
			WithReadModelUnitOfWork.Do(() =>
			{
				Persister.AddOut(person, "2016-10-18 08:00".Utc());
				Persister.AddIn(person, "2016-10-18 08:05".Utc());
				Persister.AddOut(person, "2016-10-18 09:00".Utc());
				Persister.AddNeutral(person, "2016-10-18 09:15".Utc());
			});

			var result = WithReadModelUnitOfWork.Get(() => Reader.Read(person, "2016-10-18 00:00".Utc(), "2016-10-19 00:00".Utc()));

			result.OutOfAdherences.First().StartTime.Should().Be("2016-10-18 08:00".Utc());
			result.OutOfAdherences.First().EndTime.Should().Be("2016-10-18 08:05".Utc());
			result.OutOfAdherences.Last().StartTime.Should().Be("2016-10-18 09:00".Utc());
			result.OutOfAdherences.Last().EndTime.Should().Be("2016-10-18 09:15".Utc());
		}

		[Test]
		public void ShouldGetOutOfAdherenceOcurrencesTwoHoursBeforeInterval()
		{
			Database.WithAgent("Mikkey");
			var personId = WithUnitOfWork.Get(() => Persons.FindAllSortByName().Single(x => x.Name.FirstName == "Mikkey").Id.Value);
			WithReadModelUnitOfWork.Do(() =>
			{
				Persister.AddOut(personId, "2016-10-11 22:00".Utc());
				Persister.AddIn(personId, "2016-10-11 23:00".Utc());
			});

			var data = WithReadModelUnitOfWork.Get(() => Reader.Read(personId, "2016-10-12 00:00".Utc(), "2016-10-13 00:00".Utc()));

			var result = data.OutOfAdherences.Single();
			result.StartTime.Should().Be("2016-10-11T22:00:00".Utc());
			result.EndTime.Should().Be("2016-10-11T23:00:00".Utc());
		}

		[Test]
		public void ShouldGetOutOfAdherenceOcurrencesOneDayAfterInterval()
		{
			Database.WithAgent("Mikkey");
			var personId = WithUnitOfWork.Get(() => Persons.FindAllSortByName().Single(x => x.Name.FirstName == "Mikkey").Id.Value);
			WithReadModelUnitOfWork.Do(() =>
			{
				Persister.AddOut(personId, "2016-10-13 23:00".Utc());
				Persister.AddIn(personId, "2016-10-14 00:00".Utc());
			});

			var data = WithReadModelUnitOfWork.Get(() => Reader.Read(personId, "2016-10-12 00:00".Utc(), "2016-10-13 00:00".Utc()));

			var result = data.OutOfAdherences.Single();
			result.StartTime.Should().Be("2016-10-13T23:00:00".Utc());
			result.EndTime.Should().Be("2016-10-14T00:00:00".Utc());
		}

		[Test]
		public void ShouldExcludeOutOfAdherenceOcurrencesLessThanTwoHoursBeforeInterval()
		{
			Database.WithAgent("Mikkey");
			var personId = WithUnitOfWork.Get(() => Persons.FindAllSortByName().Single(x => x.Name.FirstName == "Mikkey").Id.Value);
			WithReadModelUnitOfWork.Do(() =>
			{
				Persister.AddOut(personId, "2016-10-11 21:00".Utc());
				Persister.AddIn(personId, "2016-10-11 21:59".Utc());
				Persister.AddOut(personId, "2016-10-11 22:00".Utc());
				Persister.AddIn(personId, "2016-10-11 22:01".Utc());
			});

			var data = WithReadModelUnitOfWork.Get(() => Reader.Read(personId, "2016-10-12 00:00".Utc(), "2016-10-13 00:00".Utc()));

			data.OutOfAdherences.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldExcludeOutOfAdherenceOcurrencesMoreThanOneDayAfterInterval()
		{
			Database.WithAgent("Mikkey");
			var personId = WithUnitOfWork.Get(() => Persons.FindAllSortByName().Single(x => x.Name.FirstName == "Mikkey").Id.Value);
			WithReadModelUnitOfWork.Do(() =>
			{
				Persister.AddOut(personId, "2016-10-13 23:00".Utc());
				Persister.AddIn(personId, "2016-10-14 00:01".Utc());
			});

			var data = WithReadModelUnitOfWork.Get(() => Reader.Read(personId, "2016-10-12 00:00".Utc(), "2016-10-13 00:00".Utc()));

			var result = data.OutOfAdherences.Single();
			result.StartTime.Should().Be("2016-10-13T23:00:00".Utc());
			result.EndTime.Should().Be(null);
		}
	}
}