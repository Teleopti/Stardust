﻿using System.Runtime.Serialization;
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

		public override GridCellRendererBase CreateRenderer(GridControlBase control)
		{
			var renderer = new GridComboBoxCellRenderer(control, this);
			((GridComboBoxListBoxPart) renderer.ListBoxPart).DropDownRows = 25;
			return renderer;
		}
	}
}
