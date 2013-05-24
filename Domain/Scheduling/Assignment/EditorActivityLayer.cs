﻿using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public interface IEditorActivityLayer
	{
		void SetParent(IEditorShift parent);
		IEditorShift Parent { get; }
	}

	public class EditorActivityLayer : ActivityLayer, IEditorActivityLayer
	{
		private IEditorShift _parent;

		public EditorActivityLayer(IActivity activity, DateTimePeriod period)
			: base(activity, period)
		{
			InParameter.EnsureNoSecondsInPeriod(period);
		}

		public override int OrderIndex
		{
			get
			{
				if (_parent == null)
					return -1;

				return Parent.LayerCollection.IndexOf(this);
			}
		}

		public new IEditorShift Parent
		{
			get { return _parent; }
		}

		public void SetParent(IEditorShift parent)
		{
			_parent = parent;
		}


	}
}