using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;


namespace Teleopti.Ccc.TestCommon.FakeData
{
	public class EditableShiftFactory
	{
		public static IEditableShift CreateEditorShift(IActivity activity, DateTimePeriod period, IShiftCategory category)
		{
			var shift = new EditableShift(category);
			var layer = new EditableShiftLayer(activity, period);
			shift.LayerCollection.Add(layer);
			return shift;
		}


		/// <summary>
		/// Creates the main shift.
		/// </summary>
		/// <param name="cat">The cat.</param>
		/// <returns></returns>
		public static IEditableShift CreateEditorShift(IShiftCategory cat)
		{
			var shift = new EditableShift(cat);
			return shift;
		}

		public static IEditableShift CreateEditorShift(TimeSpan start, TimeSpan end, IActivity activity, IShiftCategory category)
		{
			var shift = new EditableShift(category);
			var layer1 = new EditableShiftLayer(activity, new DateTimePeriod(WorkShift.BaseDate.Add(start),
									  WorkShift.BaseDate.Add(end)));
			shift.LayerCollection.Add(layer1);
			return shift;
		}

		/// <summary>
		/// Creates mainshift with three activity layers.
		/// </summary>
		/// <returns></returns>
		public static IEditableShift CreateEditorShiftWithThreeActivityLayers()
		{
			var telephone = ActivityFactory.CreateActivity("Tel");
			DateTimePeriod period1 =
				new DateTimePeriod(new DateTime(2007, 1, 1, 8, 0, 0, DateTimeKind.Utc),
								   new DateTime(2007, 1, 1, 12, 0, 0, DateTimeKind.Utc));

			var longDuty = ActivityFactory.CreateActivity("Long duty");
			DateTimePeriod period2 =
				new DateTimePeriod(new DateTime(2007, 1, 1, 8, 0, 0, DateTimeKind.Utc),
								   new DateTime(2007, 1, 2, 1, 0, 0, DateTimeKind.Utc));

			DateTimePeriod period3 =
				new DateTimePeriod(new DateTime(2007, 1, 2, 1, 0, 0, DateTimeKind.Utc),
								   new DateTime(2007, 1, 2, 2, 0, 0, DateTimeKind.Utc));


			EditableShiftLayer layer1 = new EditableShiftLayer(telephone, period1);
			EditableShiftLayer layer2 = new EditableShiftLayer(longDuty, period2);
			EditableShiftLayer layer3 = new EditableShiftLayer(longDuty, period3);

			var resultShift = CreateEditorShift(ShiftCategoryFactory.CreateShiftCategory("TEL"));
			resultShift.LayerCollection.Add(layer1);
			resultShift.LayerCollection.Add(layer2);
			resultShift.LayerCollection.Add(layer3);

			return resultShift;
		}

		public static IEditableShift CreateEditorShiftWithLayers(IActivity baseAct, IActivity lunchAct, IActivity shortAct)
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


			EditableShiftLayer layer1 = new EditableShiftLayer(baseAct, period1);
			EditableShiftLayer layer2 = new EditableShiftLayer(lunchAct, period2);
			EditableShiftLayer layer3 = new EditableShiftLayer(shortAct, period3);
			EditableShiftLayer layer4 = new EditableShiftLayer(shortAct, period4);

			var resultShift = CreateEditorShift(ShiftCategoryFactory.CreateShiftCategory("TEL"));
			resultShift.LayerCollection.Add(layer1);
			resultShift.LayerCollection.Add(layer2);
			resultShift.LayerCollection.Add(layer3);
			resultShift.LayerCollection.Add(layer4);

			return resultShift;
		}

	}
}