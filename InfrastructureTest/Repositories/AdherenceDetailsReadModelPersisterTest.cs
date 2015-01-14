using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	[TestFixture, Category("LongRunning")]
	[ReadModelTest]
	public class AdherenceDetailsReadModelPersisterTest
	{
		public IAdherenceDetailsReadModelPersister Target { get; set; }

		[Test]
		public void ShouldSaveReadModelForPerson()
		{
			var personId = Guid.NewGuid();
			var dateOnly = new DateOnly(2014, 11, 19);
			var timeInAdherence = TimeSpan.FromMinutes(10);
			var timeOutOfAdherence = TimeSpan.FromMinutes(20);
			var readModel = createReadModel(dateOnly, personId);
			var detailModel = createAdherenceDetail("Phone", new DateTime(2014, 11, 19, 8, 0, 0), timeInAdherence, timeOutOfAdherence);
			readModel.Model.Details.Add(detailModel);
			
			Target.Add(readModel);

			var savedModel = Target.Get(personId, dateOnly);
			var model = savedModel.Model.Details.First();
			savedModel.Model.IsInAdherence.Should().Be(readModel.Model.IsInAdherence);
			model.Name.Should().Be(detailModel.Name);
			model.StartTime.Should().Be(detailModel.StartTime);
			model.ActualStartTime.Should().Be(detailModel.ActualStartTime);
			model.TimeInAdherence.Should().Be(detailModel.TimeInAdherence);
			model.TimeOutOfAdherence.Should().Be(detailModel.TimeOutOfAdherence);
			model.LastStateChangedTime.Should().Be(detailModel.LastStateChangedTime);
		}


		[Test]
		public void ShouldSaveWithNullables()
		{
			var personId = Guid.NewGuid();
			var dateOnly = new DateOnly(2012, 8, 29);
			var timeInAdherence = TimeSpan.FromMinutes(17);
			var timeOutOfAdherence = TimeSpan.FromMinutes(28);
			var readModel = createReadModel(dateOnly, personId);
			var detailModel = createAdherenceDetail("Phone", new DateTime(2014, 11, 19, 8, 0, 0), timeInAdherence, timeOutOfAdherence);
			readModel.Model.Details.Add(detailModel);

			detailModel.LastStateChangedTime = null;
			detailModel.ActualStartTime = null;
			detailModel.StartTime = null;

			Target.Add(readModel);

			var savedModel = Target.Get(personId, dateOnly);

			var model = savedModel.Model.Details.First();
			model.StartTime.Should().Be(null);
			model.ActualStartTime.Should().Be(null);
			model.LastStateChangedTime.Should().Be(null);
		}

		[Test]
		public void ShouldSaveReadModelOnDifferentActivitiesForSamePerson()
		{
			var personId = Guid.NewGuid();
			var dateOnly = new DateOnly(2014, 11, 19);
			var timeInAdherence = TimeSpan.FromMinutes(10);
			var timeOutOfAdherence = TimeSpan.FromMinutes(20);
			var readModel = createReadModel(dateOnly, personId);
			var detailModel1 = createAdherenceDetail("Phone", new DateTime(2014, 11, 19, 8, 0, 0), timeInAdherence, timeOutOfAdherence);
			var detailModel2 = createAdherenceDetail("Lunch", new DateTime(2014, 11, 19, 9, 0, 0), timeInAdherence, timeOutOfAdherence);
			readModel.Model.Details.Add(detailModel1);
			readModel.Model.Details.Add(detailModel2);
			Target.Add(readModel);

			var savedModel = Target.Get(personId, dateOnly);
			savedModel.PersonId.Should().Be(personId);
			savedModel.BelongsToDate.Should().Be(readModel.BelongsToDate);
			savedModel.Model.IsInAdherence.Should().Be(readModel.Model.IsInAdherence);
			
			var firstModel = savedModel.Model.Details.First();
			firstModel.Name.Should().Be(detailModel1.Name);
			firstModel.StartTime.Should().Be(detailModel1.StartTime);
			firstModel.ActualStartTime.Should().Be(detailModel1.ActualStartTime);
			firstModel.TimeInAdherence.Should().Be(detailModel1.TimeInAdherence);
			firstModel.TimeOutOfAdherence.Should().Be(detailModel1.TimeOutOfAdherence);
			firstModel.LastStateChangedTime.Should().Be(detailModel1.LastStateChangedTime);

			var secondModel = savedModel.Model.Details.Last();
			secondModel.Name.Should().Be(detailModel2.Name);
			secondModel.StartTime.Should().Be(detailModel2.StartTime);
			secondModel.ActualStartTime.Should().Be(detailModel2.ActualStartTime);
			secondModel.TimeInAdherence.Should().Be(detailModel2.TimeInAdherence);
			secondModel.TimeOutOfAdherence.Should().Be(detailModel2.TimeOutOfAdherence);
			secondModel.LastStateChangedTime.Should().Be(detailModel2.LastStateChangedTime);
		}

		[Test]
		public void ShouldSaveReadModelOnDifferentDaysForSamePerson()
		{
			var personId = Guid.NewGuid();
			var dateOnly1 = new DateOnly(2014, 11, 19);
			var dateOnly2 = new DateOnly(2014, 11, 20);
			var timeInAdherence = TimeSpan.FromMinutes(10);
			var timeOutOfAdherence = TimeSpan.FromMinutes(20);
			var readModel1 = createReadModel(dateOnly1, personId);
			var readModel2 = createReadModel(dateOnly2, personId);
			var detailModel1 = createAdherenceDetail("Phone", new DateTime(2014, 11, 19, 8, 0, 0), timeInAdherence, timeOutOfAdherence);
			var detailModel2 = createAdherenceDetail("Phone", new DateTime(2014, 11, 20, 9, 0, 0), timeInAdherence, timeOutOfAdherence);
			readModel1.Model.Details.Add(detailModel1);
			readModel2.Model.Details.Add(detailModel2);

			Target.Add(readModel1);
			Target.Add(readModel2);

			var savedModel = Target.Get(personId, dateOnly1);
			savedModel.PersonId.Should().Be(readModel1.PersonId);
			savedModel.BelongsToDate.Should().Be(readModel1.BelongsToDate);
			savedModel.Model.IsInAdherence.Should().Be(readModel1.Model.IsInAdherence);

			var savedDetailModel1 = savedModel.Model.Details.First();
			savedDetailModel1.Name.Should().Be(detailModel1.Name);
			savedDetailModel1.StartTime.Should().Be(detailModel1.StartTime);
			savedDetailModel1.ActualStartTime.Should().Be(detailModel1.ActualStartTime);
			savedDetailModel1.TimeInAdherence.Should().Be(detailModel1.TimeInAdherence);
			savedDetailModel1.TimeOutOfAdherence.Should().Be(detailModel1.TimeOutOfAdherence);
			savedDetailModel1.LastStateChangedTime.Should().Be(detailModel1.LastStateChangedTime);
			
			var savedModel2 = Target.Get(personId, dateOnly2);

			savedModel2.PersonId.Should().Be(readModel2.PersonId);
			savedModel2.BelongsToDate.Should().Be(readModel2.BelongsToDate);
			savedModel2.Model.IsInAdherence.Should().Be(readModel2.Model.IsInAdherence);

			var savedDetailModel2 = savedModel2.Model.Details.First();
			savedDetailModel2.Name.Should().Be(detailModel2.Name);
			savedDetailModel2.StartTime.Should().Be(detailModel2.StartTime);
			savedDetailModel2.ActualStartTime.Should().Be(detailModel2.ActualStartTime);
			savedDetailModel2.TimeInAdherence.Should().Be(detailModel2.TimeInAdherence);
			savedDetailModel2.TimeOutOfAdherence.Should().Be(detailModel2.TimeOutOfAdherence);
			savedDetailModel2.LastStateChangedTime.Should().Be(detailModel2.LastStateChangedTime);
		}

		[Test]
		public void ShouldUpdateExistingReadModel()
		{
			var personId = Guid.NewGuid();
			var dateOnly = new DateOnly(2014, 8, 29);
			var timeInAdherence = TimeSpan.FromMinutes(17);
			var timeOutOfAdherence = TimeSpan.FromMinutes(28);

			var readModel = createReadModel(dateOnly, personId);
			var detailModel = createAdherenceDetail("Phone", new DateTime(2014, 11, 19, 8, 0, 0), timeInAdherence, timeOutOfAdherence);
			readModel.Model.Details.Add(detailModel);
			Target.Add(readModel);
			detailModel.ActualStartTime = new DateTime(2014, 11, 19, 10, 0, 0);
			detailModel.LastStateChangedTime = new DateTime(2014, 11, 19, 10, 0, 0);
			readModel.Model.IsInAdherence = false;
			detailModel.TimeInAdherence = TimeSpan.FromMinutes(20);
			detailModel.TimeOutOfAdherence = TimeSpan.FromMinutes(30);
			readModel.Model.HasShiftEnded = true;
			Target.Update(readModel);

			var savedModel = Target.Get(personId, dateOnly);
			savedModel.PersonId.Should().Be(personId);
			savedModel.BelongsToDate.Should().Be(dateOnly);
			savedModel.Model.HasShiftEnded.Should().Be(true);
			var model = savedModel.Model.Details.First();
			model.ActualStartTime.Should().Be(detailModel.ActualStartTime);
			savedModel.Model.IsInAdherence.Should().Be(readModel.Model.IsInAdherence);
			model.TimeInAdherence.Should().Be(detailModel.TimeInAdherence);
			model.TimeOutOfAdherence.Should().Be(detailModel.TimeOutOfAdherence);
			model.LastStateChangedTime.Should().Be(detailModel.LastStateChangedTime);
		}
		
		[Test]
		public void ShouldUpdateWithNullables()
		{
			var personId = Guid.NewGuid();
			var dateOnly = new DateOnly(2012, 8, 29);
			var timeInAdherence = TimeSpan.FromMinutes(17);
			var timeOutOfAdherence = TimeSpan.FromMinutes(28);
			var readModel = createReadModel(dateOnly, personId);
			var detailModel = createAdherenceDetail("Phone", new DateTime(2014, 11, 19, 8, 0, 0), timeInAdherence, timeOutOfAdherence);
			readModel.Model.Details.Add(detailModel);
			Target.Add(readModel);
			detailModel.LastStateChangedTime = null;
			detailModel.ActualStartTime = null;
			Target.Update(readModel);

			var savedModel = Target.Get(personId, dateOnly);
			savedModel.Model.Details.First().ActualStartTime.Should().Be(null);
			savedModel.Model.Details.First().LastStateChangedTime.Should().Be(null);
		}

		[Test]
		public void ShouldClearExistingReadModel()
		{
			var personId = Guid.NewGuid();
			var dateOnly = new DateOnly(2014, 8, 29);
			var timeInAdherence = TimeSpan.FromMinutes(17);
			var timeOutOfAdherence = TimeSpan.FromMinutes(28);

			var readModel = createReadModel(dateOnly, personId);
			var detailModel = createAdherenceDetail("Phone", new DateTime(2014, 11, 19, 8, 0, 0), timeInAdherence, timeOutOfAdherence);
			readModel.Model.Details.Add(detailModel);
			Target.Add(readModel);

			Target.ClearDetails(readModel);

			var savedModel = Target.Get(personId, dateOnly);
			savedModel.Model.Details.Count.Should().Be(0);
		}

		private static AdherenceDetailsReadModel createReadModel(DateOnly dateOnly, Guid personId)
		{
			var detailsModel = new AdherenceDetailsModel
			{
				Details = new List<AdherenceDetailModel>()
			};
			
			return new AdherenceDetailsReadModel
			{
				PersonId = personId,
				Date = dateOnly,
				Model = detailsModel
			};
		}

		private static AdherenceDetailModel createAdherenceDetail(string activityName, DateTime startTime, TimeSpan timeInAdherence, TimeSpan timeOutOfAdherence)
		{
			return new AdherenceDetailModel
			{
				TimeInAdherence = timeInAdherence,
				TimeOutOfAdherence = timeOutOfAdherence,
				ActualStartTime = startTime,
				Name = activityName,
				StartTime = startTime,
			};
		}
	}

}
