using System;
using System.IO;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Infrastructure.MachineLearning;

namespace Teleopti.Ccc.InfrastructureTest.MachineLearning
{
	public class ShiftCategoryPredictionTest
	{
		[Test]
		public void SimpleShiftCategorySelectionTest()
		{
			var shiftCategories = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
			var data = new[]
			{
				new ShiftCategoryPredictorModel {StartTime = 8.0, EndTime = 16.0, ShiftCategory = shiftCategories[0].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 8.5, EndTime = 16.5, ShiftCategory = shiftCategories[0].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 8.5, EndTime = 16.5, ShiftCategory = shiftCategories[0].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 14.0, EndTime = 22.0, ShiftCategory = shiftCategories[1].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 14.25, EndTime = 22.0, ShiftCategory = shiftCategories[1].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 14.25, EndTime = 22.0, ShiftCategory = shiftCategories[1].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 5.0, EndTime = 14.0, ShiftCategory = shiftCategories[2].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 5.5, EndTime = 13.5, ShiftCategory = shiftCategories[2].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 5.5, EndTime = 13.5, ShiftCategory = shiftCategories[2].ToString()},
			};

			var p = new PredictCategory();
			var m = p.Train(data);

			Guid.Parse(m.Predict(14.0, 22.0)).Should().Be.EqualTo(shiftCategories[1]);
		}

		[Test]
		public void SimpleShiftCategorySelectionOutsideTrainingDataTest()
		{
			var shiftCategories = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
			var data = new[]
			{
				new ShiftCategoryPredictorModel {StartTime = 8.0, EndTime = 16.0, ShiftCategory = shiftCategories[0].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 8.5, EndTime = 16.5, ShiftCategory = shiftCategories[0].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 8.5, EndTime = 16.5, ShiftCategory = shiftCategories[0].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 14.0, EndTime = 22.0, ShiftCategory = shiftCategories[1].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 14.25, EndTime = 22.0, ShiftCategory = shiftCategories[1].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 14.25, EndTime = 22.0, ShiftCategory = shiftCategories[1].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 5.0, EndTime = 14.0, ShiftCategory = shiftCategories[2].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 5.5, EndTime = 13.5, ShiftCategory = shiftCategories[2].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 5.5, EndTime = 13.5, ShiftCategory = shiftCategories[2].ToString()},
			};

			var p = new PredictCategory();
			var m = p.Train(data);

			Guid.Parse(m.Predict(7.0, 15.0)).Should().Be.EqualTo(shiftCategories[0]);
		}

		[Test]
		public void AnotherSimpleShiftCategorySelectionTest()
		{
			var shiftCategories = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
			var data = new[]
			{
				new ShiftCategoryPredictorModel {StartTime = 8.0, EndTime = 16.0, ShiftCategory = shiftCategories[0].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 8.5, EndTime = 16.5, ShiftCategory = shiftCategories[0].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 8.5, EndTime = 16.5, ShiftCategory = shiftCategories[0].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 14.0, EndTime = 22.0, ShiftCategory = shiftCategories[1].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 14.25, EndTime = 22.0, ShiftCategory = shiftCategories[1].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 14.25, EndTime = 22.0, ShiftCategory = shiftCategories[1].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 5.0, EndTime = 14.0, ShiftCategory = shiftCategories[2].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 5.5, EndTime = 13.5, ShiftCategory = shiftCategories[2].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 5.5, EndTime = 13.5, ShiftCategory = shiftCategories[2].ToString()},
			};

			var p = new PredictCategory();
			var m = p.Train(data);

			Guid.Parse(m.Predict(5.0, 14.0)).Should().Be.EqualTo(shiftCategories[2]);
		}

		[Test]
		public void UseStoredModelForPrediction()
		{
			var shiftCategories = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
			var data = new[]
			{
				new ShiftCategoryPredictorModel {StartTime = 8.0, EndTime = 16.0, ShiftCategory = shiftCategories[0].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 8.5, EndTime = 16.5, ShiftCategory = shiftCategories[0].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 8.5, EndTime = 16.5, ShiftCategory = shiftCategories[0].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 14.0, EndTime = 22.0, ShiftCategory = shiftCategories[1].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 14.25, EndTime = 22.0, ShiftCategory = shiftCategories[1].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 14.25, EndTime = 22.0, ShiftCategory = shiftCategories[1].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 5.0, EndTime = 14.0, ShiftCategory = shiftCategories[2].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 5.5, EndTime = 13.5, ShiftCategory = shiftCategories[2].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 5.5, EndTime = 13.5, ShiftCategory = shiftCategories[2].ToString()},
			};

			var p = new PredictCategory();
			var m = p.Train(data);

			using (var storage = new MemoryStream())
			{
				m.Store(storage);

				storage.Position = 0;
				var fromStoredModel = Model.Load(storage);
				
				Guid.Parse(fromStoredModel.Predict(5.0, 14.0)).Should().Be.EqualTo(shiftCategories[2]);
			}
		}

		[Test]
		public void DayOfWeekShiftCategorySelection()
		{
			var shiftCategories = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
			var data = new[]
			{
				new ShiftCategoryPredictorModel {StartTime = 8.0, EndTime = 16.0,   DayOfWeek = DayOfWeek.Monday, ShiftCategory = shiftCategories[0].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 8.5, EndTime = 16.5,   DayOfWeek = DayOfWeek.Monday, ShiftCategory = shiftCategories[0].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 8.5, EndTime = 16.5,   DayOfWeek = DayOfWeek.Monday, ShiftCategory = shiftCategories[0].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 14.0, EndTime = 22.0,  DayOfWeek = DayOfWeek.Monday, ShiftCategory = shiftCategories[1].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 14.25, EndTime = 22.0, DayOfWeek = DayOfWeek.Monday, ShiftCategory = shiftCategories[1].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 14.25, EndTime = 22.0, DayOfWeek = DayOfWeek.Monday, ShiftCategory = shiftCategories[1].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 5.0, EndTime = 14.0,   DayOfWeek = DayOfWeek.Monday, ShiftCategory = shiftCategories[2].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 5.5, EndTime = 13.5,   DayOfWeek = DayOfWeek.Monday, ShiftCategory = shiftCategories[2].ToString()},
				new ShiftCategoryPredictorModel {StartTime = 5.5, EndTime = 13.5,   DayOfWeek = DayOfWeek.Saturday, ShiftCategory = shiftCategories[3].ToString()},
			};

			var p = new PredictCategory();
			var m = p.Train(data);

			Guid.Parse(m.Predict(new ShiftCategoryPredictorModel{StartTime = 5.0,EndTime = 13.5,DayOfWeek = DayOfWeek.Saturday})).Should().Be.EqualTo(shiftCategories[3]);
			Guid.Parse(m.Predict(new ShiftCategoryPredictorModel{StartTime = 5.0,EndTime = 13.5,DayOfWeek = DayOfWeek.Monday})).Should().Be.EqualTo(shiftCategories[2]);
		}
	}
}
