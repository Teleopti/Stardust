using System;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SyncfusionGridBinding
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
			_gridControl.Model.CommandStackChanged +=Model_CommandStackChanged;
			_gridControl.ClipboardCanPaste += OnClipboardCanPaste;
			_gridControl.ClipboardPaste += OnClipboardPaste;
		}

		private void OnClipboardPaste(object sender, GridCutPasteEventArgs e)
		{
			TriggerRangeChangedAndResetFlag();
		}

		private void OnClipboardCanPaste(object sender, GridCutPasteEventArgs e)
		{
			TriggerRangeChangingAndSetFlag();
		}

		private void Model_CommandStackChanged(object sender, EventArgs e)
		{
			if (_started)
			{
				TriggerRangeChangedAndResetFlag();
			}
			else
			{
				TriggerRangeChangingAndSetFlag();
			}
		}

		private void TriggerRangeChangingAndSetFlag()
		{
			var rangeChanging = RangeChanging;
			if (rangeChanging != null)
			{
				rangeChanging.Invoke(this, EventArgs.Empty);
			}
			_started = true;
		}

		private void TriggerRangeChangedAndResetFlag()
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