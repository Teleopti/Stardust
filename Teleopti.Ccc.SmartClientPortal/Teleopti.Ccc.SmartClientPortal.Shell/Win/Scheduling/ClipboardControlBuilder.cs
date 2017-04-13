using System.Windows.Forms;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Win.Common.Controls;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public class ClipboardControlBuilder
	{
		private readonly ClipboardControl _clipboardControl;

		public ClipboardControlBuilder(ClipboardControl clipboardControl)
		{
			_clipboardControl = clipboardControl;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		public void Build()
		{
			_clipboardControl.CutSpecialItems.Add(new ToolStripButton { Text = Resources.CutShift, Tag = ClipboardItems.Shift });
			_clipboardControl.CutSpecialItems.Add(new ToolStripButton { Text = Resources.CutAbsence, Tag = ClipboardItems.Absence });
			_clipboardControl.CutSpecialItems.Add(new ToolStripButton { Text = Resources.CutDayOff, Tag = ClipboardItems.DayOff });
			_clipboardControl.CutSpecialItems.Add(new ToolStripButton
			{
				Text = Resources.CutPersonalShift,
				Tag = ClipboardItems.PersonalShift
			});
			_clipboardControl.CutSpecialItems.Add(new ToolStripButton { Text = Resources.CutSpecial, Tag = ClipboardItems.Special });


			_clipboardControl.PasteSpecialItems.Add(new ToolStripButton { Text = Resources.PasteShift, Tag = ClipboardItems.Shift });
			_clipboardControl.PasteSpecialItems.Add(new ToolStripButton { Text = Resources.PasteAbsence, Tag = ClipboardItems.Absence });
			_clipboardControl.PasteSpecialItems.Add(new ToolStripButton { Text = Resources.PasteDayOff, Tag = ClipboardItems.DayOff });
			_clipboardControl.PasteSpecialItems.Add(new ToolStripButton
			{
				Text = Resources.PastePersonalShift,
				Tag = ClipboardItems.PersonalShift
			});
			_clipboardControl.PasteSpecialItems.Add(new ToolStripButton { Text = Resources.PasteSpecial, Tag = ClipboardItems.Special });
			_clipboardControl.PasteSpecialItems.Add(new ToolStripButton
			{
				Text = Resources.PasteShiftFromShifts,
				Tag = ClipboardItems.ShiftFromShifts
			});
		}
	}
}
