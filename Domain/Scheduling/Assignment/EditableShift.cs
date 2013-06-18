using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class EditableShift : IEditableShift
	{
		private readonly IList<ILayer<IActivity>> _layerCollection = new List<ILayer<IActivity>>();

		public EditableShift(IShiftCategory shiftCategory)
		{
			ShiftCategory = shiftCategory;
		}

		public virtual ILayerCollection<IActivity> LayerCollection
		{
			get
			{
				return (new LayerCollection<IActivity>(this, _layerCollection));
			}
		}

		public void OnAdd(ILayer<IActivity> layer)
		{
			if (!(layer is IEditorActivityLayer))
				throw new ArgumentException("Only EditorActivityLayers can be added to a editableShift");
			
			((IEditorActivityLayer)layer).SetParent(this);
		}

		public IProjectionService ProjectionService()
		{
			var proj = new VisualLayerProjectionService(null);
			proj.Add(this);
			return proj;
		}

		public bool HasProjection { get; private set; }

		public IShiftCategory ShiftCategory { get; set; }

		public IVisualLayerFactory CreateVisualLayerFactory()
		{
			return new VisualLayerFactory();
		}

		public object Clone()
		{
			var ret = new EditableShift(ShiftCategory);
			foreach (var layer in LayerCollection)
			{
				ret.LayerCollection.Add(new EditorActivityLayer(layer.Payload, layer.Period));
			}

			return ret;
		}

		public IEditableShift NoneEntityClone()
		{
			//Should not be necessary
			return (IEditableShift) Clone();

		}

		public IEditableShift EntityClone()
		{
			//Should not be necessary
			return (IEditableShift)Clone();
		}
	}
}