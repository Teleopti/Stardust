using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class EditableShift : IEditableShift
	{
		public EditableShift(IShiftCategory shiftCategory)
		{
			ShiftCategory = shiftCategory;
			LayerCollection = new List<IEditableShiftLayer>();
		}

		public IProjectionService ProjectionService()
		{
			var proj = new VisualLayerProjectionService(null);
			proj.Add(LayerCollection, new VisualLayerFactory());
			return proj;
		}

		public IShiftCategory ShiftCategory { get; set; }
		public IList<IEditableShiftLayer> LayerCollection { get; private set; }
		public IEditableShift MakeCopy()
		{
			var ret = new EditableShift(ShiftCategory);
			foreach (var layer in LayerCollection)
			{
				ret.LayerCollection.Add(new EditableShiftLayer(layer.Payload, layer.Period));
			}
			return ret;
		}
	}
}