using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("LongRunning")]
	public class AdherencePercentageReadModelPersisterTest : DatabaseTest
	{

		private static AdherencePercentageReadModel createReadModel(DateOnly dateOnly, Guid personGuid, DateTime lastTimeStamp, int minOutOfAdherence= 8, int minInAdherence = 22)
		{
			//Add tests for timezone
			//we are missing events of when the shift start and shift end
			return new AdherencePercentageReadModel
			{
				Date = dateOnly,
				IsLastTimeInAdherence = true,
				LastTimestamp = lastTimeStamp,
				MinutesInAdherence = minInAdherence,
				MinutesOutOfAdherence = minOutOfAdherence,
				PersonId = personGuid
			};
		}


		[Test]
		public void ShouldBeAbleToSaveReadModelForPerson()
		{
			var target = new AdherencePercentageReadModelPersister(CurrentUnitOfWork.Make());
			var personGuid = Guid.NewGuid();
			var dateOnly = new DateOnly(2012, 8, 29);
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var model = createReadModel(dateOnly, personGuid, new DateTime(2012, 8, 29, 8, 0, 0));
				target.Persist(model);
				var savedModel = target.Get(dateOnly, personGuid);
				savedModel.BelongsToDate.Should().Be.EqualTo(model.BelongsToDate);
				savedModel.IsLastTimeInAdherence.Should().Be.EqualTo(model.IsLastTimeInAdherence);
				savedModel.LastTimestamp.Should().Be.EqualTo(model.LastTimestamp);
				savedModel.MinutesInAdherence.Should().Be.EqualTo(model.MinutesInAdherence);
				savedModel.MinutesOutOfAdherence.Should().Be.EqualTo(model.MinutesOutOfAdherence);
				savedModel.PersonId.Should().Be.EqualTo(model.PersonId);
			}
			
		}

		[Test]
		public void ShouldBeAbleToSaveReadModelOnDifferentDaysForSamePerson()
		{
			var target = new AdherencePercentageReadModelPersister(CurrentUnitOfWork.Make());
			var personGuid = Guid.NewGuid();
			var dateOnly1 = new DateOnly(2012, 8, 29);
			var dateOnly2 = new DateOnly(2012, 8, 30);
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var model1 = createReadModel(dateOnly1, personGuid, new DateTime(2012, 8, 29, 8, 0, 0));
				target.Persist(model1);

				var model2 = createReadModel(dateOnly2, personGuid, new DateTime(2012, 8, 30, 8, 0, 0));
				target.Persist(model2);
				var savedModel = target.Get(dateOnly1, personGuid);
				savedModel.BelongsToDate.Should().Be.EqualTo(model1.BelongsToDate);
				savedModel.IsLastTimeInAdherence.Should().Be.EqualTo(model1.IsLastTimeInAdherence);
				savedModel.LastTimestamp.Should().Be.EqualTo(model1.LastTimestamp);
				savedModel.MinutesInAdherence.Should().Be.EqualTo(model1.MinutesInAdherence);
				savedModel.MinutesOutOfAdherence.Should().Be.EqualTo(model1.MinutesOutOfAdherence);
				savedModel.PersonId.Should().Be.EqualTo(model1.PersonId);

				savedModel = target.Get(dateOnly2, personGuid);
				savedModel.BelongsToDate.Should().Be.EqualTo(model2.BelongsToDate);
				savedModel.IsLastTimeInAdherence.Should().Be.EqualTo(model2.IsLastTimeInAdherence);
				savedModel.LastTimestamp.Should().Be.EqualTo(model2.LastTimestamp);
				savedModel.MinutesInAdherence.Should().Be.EqualTo(model2.MinutesInAdherence);
				savedModel.MinutesOutOfAdherence.Should().Be.EqualTo(model2.MinutesOutOfAdherence);
				savedModel.PersonId.Should().Be.EqualTo(model2.PersonId);
			}

		}

		[Test]
		public void ShouldBeAbleToUpdateExistingReadModel()
		{
			var target = new AdherencePercentageReadModelPersister(CurrentUnitOfWork.Make());
			var personGuid = Guid.NewGuid();
			var dateOnly = new DateOnly(2014, 8, 29);
			using (UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
			{
				var modelOld = createReadModel(dateOnly, personGuid, new DateTime(2014, 8, 29, 8, 0, 0),9,21);
				target.Persist(modelOld);

				var modelUpdated = createReadModel(dateOnly, personGuid, new DateTime(2014, 8, 29, 8, 15, 0),8,20);
				target.Persist(modelUpdated);

				var savedModel = target.Get(dateOnly, personGuid);
				savedModel.BelongsToDate.Should().Be.EqualTo(modelUpdated.BelongsToDate);
				savedModel.IsLastTimeInAdherence.Should().Be.EqualTo(modelUpdated.IsLastTimeInAdherence);
				savedModel.LastTimestamp.Should().Be.EqualTo(modelUpdated.LastTimestamp);
				savedModel.MinutesInAdherence.Should().Be.EqualTo(modelUpdated.MinutesInAdherence);
				savedModel.MinutesOutOfAdherence.Should().Be.EqualTo(modelUpdated.MinutesOutOfAdherence);
				savedModel.PersonId.Should().Be.EqualTo(modelUpdated.PersonId);
			}

		}


	}
}

