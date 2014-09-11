using System;
using Teleopti.Interfaces.Domain;

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
			var baseDate = new DateOnly(WorkShift.BaseDate);

			foreach (var editableShiftLayer in mainShift.LayerCollection)
			{
				var periodStart = editableShiftLayer.Period.StartDateTimeLocal(timeZoneInfo);
				var diff = periodStart.Subtract(editableShiftLayer.Period.StartDateTime);
				var dateTimePeriod = editableShiftLayer.Period.MovePeriod(TimeSpan.FromMinutes(diff.TotalMinutes));
				var dateDiff = baseDate.Date.Subtract(currentDate.Date);
				dateTimePeriod = new DateTimePeriod(dateTimePeriod.StartDateTime, dateTimePeriod.EndDateTime).MovePeriod(dateDiff);

				var layer = new WorkShiftActivityLayer(editableShiftLayer.Payload, dateTimePeriod);
				workShift.LayerCollection.Add(layer);		
			}

			return workShift;
		}
	}
}