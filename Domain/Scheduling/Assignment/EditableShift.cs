using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

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
			var proj = new VisualLayerProjectionService();
			proj.Add(LayerCollection, new VisualLayerFactory());
			return proj;
		}

		public IShiftCategory ShiftCategory { get; set; }
		public List<IEditableShiftLayer> LayerCollection { get; }
		public IEditableShift MakeCopy()
		{
			var ret = new EditableShift(ShiftCategory);
			ret.LayerCollection.AddRange(LayerCollection.Select(layer => new EditableShiftLayer(layer.Payload, layer.Period)).ToArray());

			return ret;
		}

		public IEditableShift MoveTo(DateOnly currentDate, DateOnly destinationDate)
		{
			var datediff = destinationDate.Date.Subtract(currentDate.Date);
			var ret = new EditableShift(ShiftCategory);

			ret.LayerCollection.AddRange(LayerCollection.Select(layer =>
			{
				var movedLayerPeriod = layer.Period.MovePeriod(datediff);
				return new EditableShiftLayer(layer.Payload, movedLayerPeriod);
			}).ToArray());

			return ret;
		}
	}
}