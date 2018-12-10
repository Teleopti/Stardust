using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IWorkShiftFromEditableShift
	{
		IWorkShift Convert(IEditableShift mainShift, DateOnly currentDate, TimeZoneInfo timeZoneInfo);
	}

	public class WorkShiftFromEditableShift : IWorkShiftFromEditableShift
	{
		public IWorkShift Convert(IEditableShift mainShift, DateOnly currentDate, TimeZoneInfo timeZoneInfo)
		{
			var workShift = new WorkShift(mainShift.ShiftCategory);
			var baseDate = WorkShift.BaseDateOnly;
			var baseDateAdjustment = TimeSpan.MaxValue;

			foreach (var editableShiftLayer in mainShift.LayerCollection)
			{
				var periodStart = editableShiftLayer.Period;
				var currentDateDiff = periodStart.ToDateOnlyPeriod(timeZoneInfo).StartDate.Subtract(currentDate);
				if (currentDateDiff < baseDateAdjustment)baseDateAdjustment = currentDateDiff;
			}

			if (baseDateAdjustment == TimeSpan.MaxValue)baseDateAdjustment = TimeSpan.Zero;

			foreach (var editableShiftLayer in mainShift.LayerCollection)
			{
				var periodStart = editableShiftLayer.Period.StartDateTimeLocal(timeZoneInfo);
				var diff = periodStart.Subtract(editableShiftLayer.Period.StartDateTime);
				var dateTimePeriod = editableShiftLayer.Period.MovePeriod(TimeSpan.FromMinutes(diff.TotalMinutes));
				var dateDiff = baseDate.Date.Subtract(currentDate.Date.Add(baseDateAdjustment));
				dateTimePeriod = new DateTimePeriod(dateTimePeriod.StartDateTime, dateTimePeriod.EndDateTime).MovePeriod(dateDiff);
				var layer = new WorkShiftActivityLayer(editableShiftLayer.Payload, dateTimePeriod);
				workShift.LayerCollection.Add(layer);
			}

			return workShift;
		}
	}
}