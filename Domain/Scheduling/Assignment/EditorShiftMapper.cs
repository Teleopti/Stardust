using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public interface IEditableShiftMapper
	{
		IEditableShift CreateEditorShift(IPersonAssignment personassignment);
		void SetMainShiftLayers(IPersonAssignment personassignment, IEditableShift editableShift);
	}

	public class EditableShiftMapper : IEditableShiftMapper
	{
		 public IEditableShift CreateEditorShift(IPersonAssignment personassignment)
		 {
			 if (!personassignment.MainActivities().Any())
				 return null;

			 var retShift = new EditableShift(personassignment.ShiftCategory);
			 retShift.LayerCollection.AddRange(
				 personassignment.MainActivities()
					 .Select(
						 mainShiftActivityLayer => new EditableShiftLayer(mainShiftActivityLayer.Payload, mainShiftActivityLayer.Period)));

			 return retShift;
		 }

		//should be removed
		public void SetMainShiftLayers(IPersonAssignment personassignment, IEditableShift editableShift)
		{
			InParameter.NotNull(nameof(personassignment), personassignment);
			InParameter.NotNull(nameof(editableShift), editableShift);

			personassignment.ClearMainActivities();
			foreach (var layer in editableShift.LayerCollection)
			{
				personassignment.AddActivity(layer.Payload, layer.Period);
			}
			personassignment.SetShiftCategory(editableShift.ShiftCategory);
		}
	}
}