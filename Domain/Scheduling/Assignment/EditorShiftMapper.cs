

using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public interface IEditorShiftMapper
	{
		IEditableShift CreateEditorShift(IPersonAssignment personassignment);
		void SetMainShiftLayers(IPersonAssignment personassignment, IEditableShift editableShift);
	}

	public class EditorShiftMapper : IEditorShiftMapper
	{
		 public IEditableShift CreateEditorShift(IPersonAssignment personassignment)
		 {
			 if (!personassignment.MainShiftActivityLayers.Any())
				 return null;

			 var retShift = new EditableShift(personassignment.ShiftCategory);
			 foreach (var mainShiftActivityLayer in personassignment.MainShiftActivityLayers)
			 {
				 retShift.LayerCollection.Add(new EditorActivityLayer(mainShiftActivityLayer.Payload, mainShiftActivityLayer.Period));
			 }

			 return retShift;
		 }

		public void SetMainShiftLayers(IPersonAssignment personassignment, IEditableShift editableShift)
		{
			InParameter.NotNull("personassignment", personassignment);
			InParameter.NotNull("editableShift", editableShift);

			var layerList = new List<IMainShiftActivityLayerNew>();
			foreach (var layer in editableShift.LayerCollection)
			{
				layerList.Add(new MainShiftActivityLayerNew(layer.Payload, layer.Period));
			}
			personassignment.SetMainShiftLayers(layerList, editableShift.ShiftCategory);
		}
	}
}