using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells
{
	[Serializable]
	public class GridDropDownAdvComboBoxCellModel : GridDropDownCellModel
	{
		public GridDropDownAdvComboBoxCellModel(GridModel grid) : base(grid)
		{
		}

		protected GridDropDownAdvComboBoxCellModel(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override GridCellRendererBase CreateRenderer(GridControlBase control)
		{
			return new GridDropDownAdvComboBoxCellRenderer(control, this);
		}
	}

	public class GridDropDownAdvComboBoxCellRenderer : GridDropDownCellRenderer
	{
		private ListBox listBox;

		public GridDropDownAdvComboBoxCellRenderer(GridControlBase grid, GridCellModelBase cellModel) : base(grid, cellModel)
		{
			DropDownButton = new GridCellComboBoxButton(this);
			DropDownImp.InitFocusEditPart = true;
		}

		protected override void InitializeDropDownContainer()
		{
			base.InitializeDropDownContainer();

			listBox = new CheckedListBox
			{
				CheckOnClick = true,
				Dock = DockStyle.Fill,
				DisplayMember = CurrentStyle.DisplayMember
			};

			foreach (var item in (IEnumerable<object>) CurrentStyle.DataSource)
			{
				listBox.Items.Add(item);
			}

			listBox.Size = new Size(listBox.Size.Width, (listBox.Items.Count + 1) * listBox.ItemHeight);

			setSelectedValues();

			DropDownContainer.Size = new Size(Grid.Model.ColWidths[ColIndex], listBox.Size.Height);
			DropDownContainer.Controls.Add(listBox);
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
				listBox.SelectedIndexChanged -= selectedIndexChanged;
				setSelectedValues();
				listBox.SelectedIndexChanged += selectedIndexChanged;
				DropDownContainer.Size = new Size(Grid.Model.ColWidths[ColIndex], listBox.Size.Height);
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

		private void selectedIndexChanged(object sender, EventArgs e)
		{
			setCurrentCellText();
		}

		private void setSelectedValues()
		{
			if (!string.IsNullOrEmpty(TextBox.Text))
			{
				var values = TextBox.Text.Split(',');
				for (var index = 0; index < listBox.Items.Count; index++)
				{
					((CheckedListBox) listBox).SetItemChecked(index, values.Contains(listBox.Items[index].ToString()));
				}
			}
			else
			{
				listBox.SelectedItems.Clear();
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

			foreach (var listBoxSelectedItem in ((CheckedListBox) listBox).CheckedItems)
			{
				selectedValues += listBoxSelectedItem + ",";
			}

			return selectedValues.TrimEnd(',');
		}
	}
}
