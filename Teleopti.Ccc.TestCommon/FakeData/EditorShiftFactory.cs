using System;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class EditorShiftFactory
	{
		public static IEditorShift CreateEditorShift(IActivity activity, DateTimePeriod period, IShiftCategory category)
		{
			var shift = new EditorShift(category);
			var layer = new EditorActivityLayer(activity, period);
			shift.LayerCollection.Add(layer);
			return shift;
		}

		/// <summary>
		/// Creates a shift with default category
		/// </summary>
		/// <returns></returns>
		public static IEditorShift CreateEditorShiftWithDefaultCategory()
		{
			ShiftCategory category = new ShiftCategory("Morning");
			var shift = new EditorShift(category);
			return shift;
		}

		/// <summary>
		/// Creates the main shift.
		/// </summary>
		/// <param name="cat">The cat.</param>
		/// <returns></returns>
		public static IEditorShift CreateEditorShift(IShiftCategory cat)
		{
			var shift = new EditorShift(cat);
			return shift;
		}

		public static IEditorShift CreateEditorShift(TimeSpan start, TimeSpan end, IActivity activity, IShiftCategory category)
		{
			var shift = new EditorShift(category);
			var layer1 = new EditorActivityLayer(activity, new DateTimePeriod(WorkShift.BaseDate.Add(start),
									  WorkShift.BaseDate.Add(end)));
			shift.LayerCollection.Add(layer1);
			return shift;
		}

		/// <summary>
		/// Creates mainshift with three activity layers.
		/// </summary>
		/// <returns></returns>
		public static IEditorShift CreateEditorShiftWithThreeActivityLayers()
		{
			Activity telephone = ActivityFactory.CreateActivity("Tel");
			DateTimePeriod period1 =
				new DateTimePeriod(new DateTime(2007, 1, 1, 8, 0, 0, DateTimeKind.Utc),
								   new DateTime(2007, 1, 1, 12, 0, 0, DateTimeKind.Utc));

			Activity longDuty = ActivityFactory.CreateActivity("Long duty");
			DateTimePeriod period2 =
				new DateTimePeriod(new DateTime(2007, 1, 1, 8, 0, 0, DateTimeKind.Utc),
								   new DateTime(2007, 1, 2, 1, 0, 0, DateTimeKind.Utc));

			DateTimePeriod period3 =
				new DateTimePeriod(new DateTime(2007, 1, 2, 1, 0, 0, DateTimeKind.Utc),
								   new DateTime(2007, 1, 2, 2, 0, 0, DateTimeKind.Utc));


			EditorActivityLayer layer1 = new EditorActivityLayer(telephone, period1);
			EditorActivityLayer layer2 = new EditorActivityLayer(longDuty, period2);
			EditorActivityLayer layer3 = new EditorActivityLayer(longDuty, period3);

			var resultShift = CreateEditorShift(ShiftCategoryFactory.CreateShiftCategory("TEL"));
			resultShift.LayerCollection.Add(layer1);
			resultShift.LayerCollection.Add(layer2);
			resultShift.LayerCollection.Add(layer3);

			return resultShift;
		}

		public static IEditorShift CreateEditorShiftWithLayers(IActivity baseAct, IActivity lunchAct, IActivity shortAct)
		{

			DateTimePeriod period1 =
				new DateTimePeriod(new DateTime(2007, 1, 1, 8, 0, 0, DateTimeKind.Utc),
								   new DateTime(2007, 1, 1, 18, 0, 0, DateTimeKind.Utc));

			DateTimePeriod period2 =
				new DateTimePeriod(new DateTime(2007, 1, 1, 11, 0, 0, DateTimeKind.Utc),
								   new DateTime(2007, 1, 1, 12, 0, 0, DateTimeKind.Utc));

			DateTimePeriod period3 =
				new DateTimePeriod(new DateTime(2007, 1, 1, 15, 0, 0, DateTimeKind.Utc),
								   new DateTime(2007, 1, 1, 15, 15, 0, DateTimeKind.Utc));

			DateTimePeriod period4 =
				new DateTimePeriod(new DateTime(2007, 1, 1, 16, 0, 0, DateTimeKind.Utc),
								   new DateTime(2007, 1, 1, 16, 15, 0, DateTimeKind.Utc));


			EditorActivityLayer layer1 = new EditorActivityLayer(baseAct, period1);
			EditorActivityLayer layer2 = new EditorActivityLayer(lunchAct, period2);
			EditorActivityLayer layer3 = new EditorActivityLayer(shortAct, period3);
			EditorActivityLayer layer4 = new EditorActivityLayer(shortAct, period4);

			var resultShift = CreateEditorShift(ShiftCategoryFactory.CreateShiftCategory("TEL"));
			resultShift.LayerCollection.Add(layer1);
			resultShift.LayerCollection.Add(layer2);
			resultShift.LayerCollection.Add(layer3);
			resultShift.LayerCollection.Add(layer4);

			return resultShift;
		}

	}
}