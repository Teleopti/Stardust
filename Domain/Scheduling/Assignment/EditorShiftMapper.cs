

using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public interface IEditorShiftMapper
	{
		IEditorShift CreateEditorShift(IPersonAssignment personassignment);
		void SetMainShiftLayers(IPersonAssignment personassignment, IEditorShift editorShift);
	}

	public class EditorShiftMapper : IEditorShiftMapper
	{
		 public IEditorShift CreateEditorShift(IPersonAssignment personassignment)
		 {
			 if (!personassignment.MainShiftActivityLayers.Any())
				 return null;

			 var retShift = new EditorShift(personassignment.ShiftCategory);
			 foreach (var mainShiftActivityLayer in personassignment.MainShiftActivityLayers)
			 {
				 retShift.LayerCollection.Add(new EditorActivityLayer(mainShiftActivityLayer.Payload, mainShiftActivityLayer.Period));
			 }

			 return retShift;
		 }

		public void SetMainShiftLayers(IPersonAssignment personassignment, IEditorShift editorShift)
		{
			InParameter.NotNull("personassignment", personassignment);
			InParameter.NotNull("editorShift", editorShift);

			var layerList = new List<IMainShiftActivityLayerNew>();
			foreach (var layer in editorShift.LayerCollection)
			{
				layerList.Add(new MainShiftActivityLayerNew(layer.Payload, layer.Period));
			}
			personassignment.SetMainShiftLayers(layerList, editorShift.ShiftCategory);
		}
	}
}