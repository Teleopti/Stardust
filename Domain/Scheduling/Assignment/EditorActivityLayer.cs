using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
	public interface IEditorActivityLayer
	{
		void SetParent(IEditableShift parent);
		IEditableShift Parent { get; }
	}

	public class EditorActivityLayer : ActivityLayer, IEditorActivityLayer
	{
		private IEditableShift _parent;

		public EditorActivityLayer(IActivity activity, DateTimePeriod period)
			: base(activity, period)
		{
			InParameter.EnsureNoSecondsInPeriod(period);
		}

		public new IEditableShift Parent
		{
			get { return _parent; }
		}

		public void SetParent(IEditableShift parent)
		{
			_parent = parent;
		}


	}
}