using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("LongRunning")]
	public class AdherenceDetailsReadModelPersisterTest : IReadModelReadWriteTest<IAdherenceDetailsReadModelPersister>
	{
		public IAdherenceDetailsReadModelPersister Target { get; set; }

		[Test]
		public void ShouldSaveReadModelForPerson()
		{
			var personId = Guid.NewGuid();
			var dateOnly = new DateOnly(2014, 11, 19);
			var timeInAdherence = TimeSpan.FromMinutes(10);
			var timeOutOfAdherence = TimeSpan.FromMinutes(20);
			var model = createReadModel(dateOnly, personId, "Phone", new DateTime(2014, 11, 19, 8, 0, 0), timeInAdherence, timeOutOfAdherence);

			Target.Add(model);

			var savedModels = Target.Get(personId, dateOnly);
			savedModels.Single().PersonId.Should().Be(personId);
			savedModels.Single().Name.Should().Be(model.Name);
			savedModels.Single().BelongsToDate.Should().Be(model.BelongsToDate);
			savedModels.Single().StartTime.Should().Be(model.StartTime);
			savedModels.Single().ActualStartTime.Should().Be(model.ActualStartTime);
			savedModels.Single().LastStateChangedTime.Should().Be(model.LastStateChangedTime);
			savedModels.Single().IsInAdherence.Should().Be(model.IsInAdherence);
			savedModels.Single().TimeInAdherence.Should().Be(model.TimeInAdherence);
			savedModels.Single().TimeOutOfAdherence.Should().Be(model.TimeOutOfAdherence);
		}


		[Test]
		public void ShouldSaveWithNullables()
		{
			var personId = Guid.NewGuid();
			var dateOnly = new DateOnly(2012, 8, 29);
			var timeInAdherence = TimeSpan.FromMinutes(17);
			var timeOutOfAdherence = TimeSpan.FromMinutes(28);
			var model = createReadModel(dateOnly, personId, "Phone", new DateTime(2014, 11, 19, 8, 0, 0), timeInAdherence, timeOutOfAdherence);
			model.LastStateChangedTime = null;
			model.ActualStartTime = null;
			model.StartTime = null;

			Target.Add(model);

			var savedModels = Target.Get(personId, dateOnly);
			savedModels.Single().StartTime.Should().Be(null);
			savedModels.Single().ActualStartTime.Should().Be(null);
			savedModels.Single().LastStateChangedTime.Should().Be(null);
		}

		[Test]
		public void ShouldSaveReadModelOnDifferentActivitiesForSamePerson()
		{
			var personId = Guid.NewGuid();
			var dateOnly = new DateOnly(2014, 11, 19);
			var timeInAdherence = TimeSpan.FromMinutes(10);
			var timeOutOfAdherence = TimeSpan.FromMinutes(20);
			var model1 = createReadModel(dateOnly, personId, "Phone", new DateTime(2014, 11, 19, 8, 0, 0), timeInAdherence, timeOutOfAdherence);
			Target.Add(model1);
			var model2 = createReadModel(dateOnly, personId, "Lunch", new DateTime(2014, 11, 19, 9, 0, 0), timeInAdherence, timeOutOfAdherence);
			Target.Add(model2);

			var savedModels = Target.Get(personId, dateOnly);

			savedModels.First().PersonId.Should().Be(personId);
			savedModels.First().Name.Should().Be(model1.Name);
			savedModels.First().BelongsToDate.Should().Be(model1.BelongsToDate);
			savedModels.First().StartTime.Should().Be(model1.StartTime);
			savedModels.First().ActualStartTime.Should().Be(model1.ActualStartTime);
			savedModels.First().LastStateChangedTime.Should().Be(model1.LastStateChangedTime);
			savedModels.First().IsInAdherence.Should().Be(model1.IsInAdherence);
			savedModels.First().TimeInAdherence.Should().Be(model1.TimeInAdherence);
			savedModels.First().TimeOutOfAdherence.Should().Be(model1.TimeOutOfAdherence);

			savedModels.Last().PersonId.Should().Be(personId);
			savedModels.Last().Name.Should().Be(model2.Name);
			savedModels.Last().BelongsToDate.Should().Be(model2.BelongsToDate);
			savedModels.Last().StartTime.Should().Be(model2.StartTime);
			savedModels.Last().ActualStartTime.Should().Be(model2.ActualStartTime);
			savedModels.Last().LastStateChangedTime.Should().Be(model2.LastStateChangedTime);
			savedModels.Last().IsInAdherence.Should().Be(model2.IsInAdherence);
			savedModels.Last().TimeInAdherence.Should().Be(model2.TimeInAdherence);
			savedModels.Last().TimeOutOfAdherence.Should().Be(model2.TimeOutOfAdherence);
		}

		[Test]
		public void ShouldSaveReadModelOnDifferentDaysForSamePerson()
		{
			var personId = Guid.NewGuid();
			var dateOnly1 = new DateOnly(2014, 11, 19);
			var dateOnly2 = new DateOnly(2014, 11, 20);
			var timeInAdherence = TimeSpan.FromMinutes(10);
			var timeOutOfAdherence = TimeSpan.FromMinutes(20);
			var model1 = createReadModel(dateOnly1, personId, "Phone", new DateTime(2014, 11, 19, 8, 0, 0), timeInAdherence, timeOutOfAdherence);
			Target.Add(model1);
			var model2 = createReadModel(dateOnly2, personId, "Phone", new DateTime(2014, 11, 20, 9, 0, 0), timeInAdherence, timeOutOfAdherence);
			Target.Add(model2);

			var savedModels = Target.Get(personId, dateOnly1);

			savedModels.Single().PersonId.Should().Be(personId);
			savedModels.Single().Name.Should().Be(model1.Name);
			savedModels.Single().BelongsToDate.Should().Be(model1.BelongsToDate);
			savedModels.Single().StartTime.Should().Be(model1.StartTime);
			savedModels.Single().ActualStartTime.Should().Be(model1.ActualStartTime);
			savedModels.Single().LastStateChangedTime.Should().Be(model1.LastStateChangedTime);
			savedModels.Single().IsInAdherence.Should().Be(model1.IsInAdherence);
			savedModels.Single().TimeInAdherence.Should().Be(model1.TimeInAdherence);
			savedModels.Single().TimeOutOfAdherence.Should().Be(model1.TimeOutOfAdherence);
			
			savedModels = Target.Get(personId, dateOnly2);

			savedModels.Single().PersonId.Should().Be(personId);
			savedModels.Single().Name.Should().Be(model2.Name);
			savedModels.Single().BelongsToDate.Should().Be(model2.BelongsToDate);
			savedModels.Single().StartTime.Should().Be(model2.StartTime);
			savedModels.Single().ActualStartTime.Should().Be(model2.ActualStartTime);
			savedModels.Single().LastStateChangedTime.Should().Be(model2.LastStateChangedTime);
			savedModels.Single().IsInAdherence.Should().Be(model2.IsInAdherence);
			savedModels.Single().TimeInAdherence.Should().Be(model2.TimeInAdherence);
			savedModels.Single().TimeOutOfAdherence.Should().Be(model2.TimeOutOfAdherence);
		}

		[Test]
		public void ShouldUpdateExistingReadModel()
		{
			var personId = Guid.NewGuid();
			var dateOnly = new DateOnly(2014, 8, 29);
			var timeInAdherence = TimeSpan.FromMinutes(17);
			var timeOutOfAdherence = TimeSpan.FromMinutes(28);

			var model = createReadModel(dateOnly, personId, "Phone", new DateTime(2014, 11, 19, 8, 0, 0), timeInAdherence, timeOutOfAdherence);
			Target.Add(model);
			model.ActualStartTime = new DateTime(2014, 11, 19, 10, 0, 0);
			model.LastStateChangedTime = new DateTime(2014, 11, 19, 10, 0, 0);
			model.IsInAdherence = false;
			model.TimeInAdherence = TimeSpan.FromMinutes(20);
			model.TimeOutOfAdherence = TimeSpan.FromMinutes(30);
			Target.Update(model);

			var savedModels = Target.Get(personId, dateOnly);
			savedModels.Single().PersonId.Should().Be(personId);
			savedModels.Single().Name.Should().Be(model.Name);
			savedModels.Single().BelongsToDate.Should().Be(model.BelongsToDate);
			savedModels.Single().StartTime.Should().Be(model.StartTime);
			savedModels.Single().ActualStartTime.Should().Be(model.ActualStartTime);
			savedModels.Single().LastStateChangedTime.Should().Be(model.LastStateChangedTime);
			savedModels.Single().IsInAdherence.Should().Be(model.IsInAdherence);
			savedModels.Single().TimeInAdherence.Should().Be(model.TimeInAdherence);
			savedModels.Single().TimeOutOfAdherence.Should().Be(model.TimeOutOfAdherence);
		}
		
		[Test]
		public void ShouldUpdateWithNullables()
		{
			var personId = Guid.NewGuid();
			var dateOnly = new DateOnly(2012, 8, 29);
			var timeInAdherence = TimeSpan.FromMinutes(17);
			var timeOutOfAdherence = TimeSpan.FromMinutes(28);
			var model = createReadModel(dateOnly, personId, "Phone", new DateTime(2014, 11, 19, 8, 0, 0), timeInAdherence, timeOutOfAdherence);
			model.LastStateChangedTime = null;
			model.ActualStartTime = null;

			Target.Add(model);
			Target.Update(model);

			var savedModels = Target.Get(personId, dateOnly);
			savedModels.Single().ActualStartTime.Should().Be(null);
			savedModels.Single().LastStateChangedTime.Should().Be(null);
		}

		[Test]
		public void ShouldRemoveExistingReadModel()
		{
			var personId = Guid.NewGuid();
			var dateOnly = new DateOnly(2014, 8, 29);
			var timeInAdherence = TimeSpan.FromMinutes(17);
			var timeOutOfAdherence = TimeSpan.FromMinutes(28);

			var model = createReadModel(dateOnly, personId, "Phone", new DateTime(2014, 11, 19, 8, 0, 0), timeInAdherence, timeOutOfAdherence);
			Target.Add(model);

			Target.Remove(personId, dateOnly);

			var savedModels = Target.Get(personId, dateOnly);
			savedModels.Count().Should().Be(0);
		}


		private static AdherenceDetailsReadModel createReadModel(DateOnly dateOnly, Guid personId, string activityName, DateTime lastStateChangedTime, TimeSpan timeInAdherence, TimeSpan timeOutOfAdherence)
		{
			return new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = dateOnly,
				TimeInAdherence = timeInAdherence,
				TimeOutOfAdherence = timeOutOfAdherence,
				IsInAdherence = true,
				LastStateChangedTime = lastStateChangedTime,
				ActualStartTime = lastStateChangedTime,
				Name = activityName,
				StartTime = lastStateChangedTime
			};
		}
	}
}
