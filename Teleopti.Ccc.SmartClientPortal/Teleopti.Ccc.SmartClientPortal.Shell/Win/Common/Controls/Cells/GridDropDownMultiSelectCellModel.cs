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
		private CheckedListBox checkedListBox;

		public GridDropDownMultiSelectCellRenderer(GridControlBase grid, GridCellModelBase cellModel) : base(grid, cellModel)
		{
			DropDownButton = new GridCellComboBoxButton(this);
			DropDownImp.InitFocusEditPart = true;
		}

		protected override void InitializeDropDownContainer()
		{
			base.InitializeDropDownContainer();

			checkedListBox = new CheckedListBox
			{
				CheckOnClick = true,
				Dock = DockStyle.Fill,
				DisplayMember = CurrentStyle.DisplayMember
			};

			foreach (var item in (IEnumerable<object>) CurrentStyle.DataSource)
			{
				checkedListBox.Items.Add(item);
			}

			checkedListBox.Size = new Size(checkedListBox.Size.Width, (checkedListBox.Items.Count + 1) * checkedListBox.ItemHeight);

			setSelectedValues();

			DropDownContainer.Size = new Size(Grid.Model.ColWidths[ColIndex], checkedListBox.Size.Height);
			DropDownContainer.Controls.Add(checkedListBox);
		}

		private void listBoxItemCheck(object sender, EventArgs e)
		{
			setCurrentCellText();
		}

		public override void DropDownContainerShowingDropDown(object sender, CancelEventArgs e)
		{
			var size = checkedListBox.Size;
			var args = new GridCurrentCellShowingDropDownEventArgs(size);
			Grid.RaiseCurrentCellShowingDropDown(args);
			if (args.Cancel)
			{
				e.Cancel = true;
			}
			else
			{
				checkedListBox.SelectedIndexChanged -= listBoxItemCheck;
				setSelectedValues();
				checkedListBox.SelectedIndexChanged += listBoxItemCheck;
				DropDownContainer.Size = new Size(Grid.Model.ColWidths[ColIndex], checkedListBox.Size.Height);
			}
		}

		private void setSelectedValues()
		{
			if (!string.IsNullOrEmpty(TextBox.Text))
			{
				var values = TextBox.Text.Split(',');
				for (var index = 0; index < checkedListBox.Items.Count; index++)
				{
					checkedListBox.SetItemChecked(index, values.Contains(checkedListBox.Items[index].ToString()));
				}
			}
			else
			{
				checkedListBox.SelectedItems.Clear();
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
			foreach (var listBoxSelectedItem in checkedListBox.CheckedItems)
			{
				selectedValues += listBoxSelectedItem + ",";
			}
			return selectedValues.TrimEnd(',');
		}
	}
}
