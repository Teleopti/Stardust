using System;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.GridBinding
{
	public class GridRangeChange
	{
		private readonly GridControl _gridControl;
		private bool _started;

		public event EventHandler RangeChanged;
		public event EventHandler RangeChanging;

		public GridRangeChange(GridControl gridControl)
		{
			_gridControl = gridControl;
			_gridControl.Model.CommandStackChanged += modelCommandStackChanged;
			_gridControl.ClipboardCanPaste += onClipboardCanPaste;
			_gridControl.ClipboardPaste += onClipboardPaste;
		}

		private void onClipboardPaste(object sender, GridCutPasteEventArgs e)
		{
			triggerRangeChangedAndResetFlag();
		}

		private void onClipboardCanPaste(object sender, GridCutPasteEventArgs e)
		{
			triggerRangeChangingAndSetFlag();
		}

		private void modelCommandStackChanged(object sender, EventArgs e)
		{
			if (_started)
			{
				triggerRangeChangedAndResetFlag();
			}
			else
			{
				triggerRangeChangingAndSetFlag();
			}
		}

		private void triggerRangeChangingAndSetFlag()
		{
			var rangeChanging = RangeChanging;
			if (rangeChanging != null)
			{
				rangeChanging.Invoke(this, EventArgs.Empty);
			}
			_started = true;
		}

		private void triggerRangeChangedAndResetFlag()
		{
			var rangeChanged = RangeChanged;
			if (rangeChanged != null)
			{
				rangeChanged.Invoke(this, EventArgs.Empty);
			}
			_started = false;
		}
	}
}