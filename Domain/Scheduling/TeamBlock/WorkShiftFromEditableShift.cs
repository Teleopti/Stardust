using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IWorkShiftFromEditableShift
	{
		IWorkShift Convert(IEditableShift mainShift, DateOnly currentDate);
	}

	public class WorkShiftFromEditableShift : IWorkShiftFromEditableShift
	{
		public IWorkShift Convert(IEditableShift mainShift, DateOnly currentDate)
		{
			var workShift = new WorkShift(mainShift.ShiftCategory);
			var rebasedShift = mainShift.MoveTo(currentDate, new DateOnly(WorkShift.BaseDate));
			foreach (var editableShiftLayer in rebasedShift.LayerCollection)
			{
				var layer = new WorkShiftActivityLayer(editableShiftLayer.Payload, editableShiftLayer.Period);
				workShift.LayerCollection.Add(layer);
			}

			return workShift;
		}
	}
}