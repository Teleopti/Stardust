using System.Runtime.Serialization;
using System.Windows.Forms.VisualStyles;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
	public class DropDownCellModel : GridComboBoxCellModel
	{
		public DropDownCellModel(GridModel grid) : base(grid)
		{
		}

		protected DropDownCellModel(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public bool Sorted { get; set; }

		public override GridCellRendererBase CreateRenderer(GridControlBase control)
		{
			var renderer = new GridComboBoxCellRenderer(control, this);
			((GridComboBoxListBoxPart) renderer.ListBoxPart).DropDownRows = 30;
			renderer.ListBoxPart.Sorted = Sorted;
			return renderer;
		}
	}
}
