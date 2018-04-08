using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Syncfusion.Diagnostics;
using Syncfusion.Styles;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells
{
	[Serializable]
	public class GridDropDownMultiSelectCellModel : GridDropDownCellModel
	{
		public GridDropDownMultiSelectCellModel(GridModel grid) : base(grid)
		{
		}

		protected GridDropDownMultiSelectCellModel(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override GridCellRendererBase CreateRenderer(GridControlBase control)
		{
			return new GridDropDownMultiSelectCellRenderer(control, this);
		}
	}

	public class GridDropDownMultiSelectCellRenderer : GridDropDownCellRenderer
	{
		private ListBox listBox;

		public GridDropDownMultiSelectCellRenderer(GridControlBase grid, GridCellModelBase cellModel) : base(grid, cellModel)
		{
			DropDownButton = new GridCellComboBoxButton(this);
			DropDownImp.InitFocusEditPart = true;
		}

		protected override void InitializeDropDownContainer()
		{
			base.InitializeDropDownContainer();

			listBox = new ListBox
			{
				SelectionMode = SelectionMode.MultiSimple,
				Dock = DockStyle.Fill,
				DisplayMember = CurrentStyle.DisplayMember
			};

			foreach (var item in (IEnumerable<object>) CurrentStyle.DataSource)
			{
				listBox.Items.Add(item);
			}

			setSelectedValues();

			DropDownContainer.Size = listBox.Size;
			DropDownContainer.Controls.Add(listBox);
		}

		private void listBoxSelectedIndexChanged(object sender, EventArgs e)
		{
			setCurrentCellText();
		}

		public override void DropDownContainerShowingDropDown(object sender, CancelEventArgs e)
		{
			var size = listBox.Size;
			var args = new GridCurrentCellShowingDropDownEventArgs(size);
			Grid.RaiseCurrentCellShowingDropDown(args);
			if (args.Cancel)
			{
				e.Cancel = true;
			}
			else
			{
				listBox.SelectedIndexChanged -= listBoxSelectedIndexChanged;
				setSelectedValues();
				listBox.SelectedIndexChanged += listBoxSelectedIndexChanged;
				DropDownContainer.Size = args.Size;
			}
		}

		private void setSelectedValues()
		{
			if (!string.IsNullOrEmpty(TextBox.Text))
			{
				var values = TextBox.Text.Split(',');
				for (var index = 0; index < listBox.Items.Count; index++)
				{
					listBox.SetSelected(index, values.Contains(listBox.Items[index].ToString()));
				}
			}
			else
			{
				listBox.SelectedItems.Clear();
			}
		}

		public override void DropDownContainerCloseDropDown(object sender, PopupClosedEventArgs e)
		{
			if (!(sender is GridDropDownContainer container)) return;

			if (container.ParentControl.Parent is GridControl gridControl)
			{
				gridControl.CurrentCell.EndEdit();
				// Workaround so edit other cells will work just after selecting
				gridControl.CurrentCell.MoveRight();
				gridControl.CurrentCell.MoveLeft();
			}
		}

		private void setCurrentCellText()
		{
			getCurrentGridModel().Text = getText();
			TextBox.Select(0, 0);
			TextBox.Modified = true;
			TextBox.Focus();
		}

		private GridStyleInfo getCurrentGridModel()
		{
			return Grid.Model[RowIndex, ColIndex];
		}

		private string getText()
		{
			var selectedValues = string.Empty;
			foreach (var listBoxSelectedItem in listBox.SelectedItems)
			{
				selectedValues += listBoxSelectedItem + ",";
			}
			return selectedValues.TrimEnd(',');
		}
	}
}
