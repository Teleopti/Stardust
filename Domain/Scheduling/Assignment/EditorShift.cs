using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public class EditorShift : IEditorShift
	{

		private IShiftCategory _shiftCategory;
		private IList<ILayer<IActivity>> _layerCollection = new List<ILayer<IActivity>>();

		public EditorShift(IShiftCategory shiftCategory)
		{
			_shiftCategory = shiftCategory;

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
				throw new ArgumentException("Only ActivityLayers can be added to a EditorShift");
			
			((IEditorActivityLayer)layer).SetParent(this);
		}

		public IProjectionService ProjectionService()
		{
			throw new System.NotImplementedException();
		}

		public bool HasProjection { get; private set; }

		public IShiftCategory ShiftCategory
		{
			get { return _shiftCategory; }
		}

		public IVisualLayerFactory CreateVisualLayerFactory()
		{
			throw new System.NotImplementedException();
		}

		public object Clone()
		{
			throw new System.NotImplementedException();
		}

		public IShift NoneEntityClone()
		{
			throw new System.NotImplementedException();
		}

		public IShift EntityClone()
		{
			throw new System.NotImplementedException();
		}
	}
}