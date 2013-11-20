using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

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
			 if (!personassignment.MainLayers().Any())
				 return null;

			 var retShift = new EditableShift(personassignment.ShiftCategory);
			 foreach (var mainShiftActivityLayer in personassignment.MainLayers())
			 {
				 retShift.LayerCollection.Add(new EditableShiftLayer(mainShiftActivityLayer.Payload, mainShiftActivityLayer.Period));
			 }

			 return retShift;
		 }

		//should be removed
		public void SetMainShiftLayers(IPersonAssignment personassignment, IEditableShift editableShift)
		{
			InParameter.NotNull("personassignment", personassignment);
			InParameter.NotNull("editableShift", editableShift);

			personassignment.ClearMainLayers();
			foreach (var layer in editableShift.LayerCollection)
			{
				personassignment.AssignActivity(layer.Payload, layer.Period);
			}
			personassignment.SetShiftCategory(editableShift.ShiftCategory);
		}
	}
}