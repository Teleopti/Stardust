using System;
using System.Globalization;
using System.Runtime.Serialization;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Drawing;
using System.Windows.Forms;
using System.Drawing;

namespace Teleopti.Ccc.Win.Common.Controls.Cells
{
    [Serializable]
	[System.ComponentModel.DesignerCategory("Code")]
	public class DateTimeCellModel : GridStaticCellModel
	{
		public DateTimeCellModel (GridModel grid)
			: base(grid)
		{	 
		}

        protected DateTimeCellModel(SerializationInfo info, StreamingContext context): base(info, context)
        {
        }

		public override GridCellRendererBase CreateRenderer(GridControlBase control)
		{
			return new DateTimeCellRenderer(control, this);
		}
	}

	public class DateTimeCellRenderer : GridStaticCellRenderer
	{
        private readonly CustomDateTimePicker dateTimePicker;
        private Timer t;

		public DateTimeCellRenderer(GridControlBase grid, GridCellModelBase cellModel)
			: base(grid, cellModel)
		{
			dateTimePicker = new CustomDateTimePicker();
          
		    dateTimePicker.Format = DateTimePickerFormat.Custom;
			dateTimePicker.ShowUpDown = false;
			dateTimePicker.ShowCheckBox = false;
			dateTimePicker.ShowDropButton = true;
			dateTimePicker.Border3DStyle = Border3DStyle.Flat;

            dateTimePicker.Culture = CultureInfo.CurrentCulture;
            dateTimePicker.Style = VisualStyle.Office2007Outlook;
            dateTimePicker.Office2007Theme = Office2007Theme.Blue;
		
        	grid.Controls.Add(dateTimePicker);

			dateTimePicker.Show();
			dateTimePicker.Hide();
		}

		protected override void OnDraw(Graphics g, Rectangle clientRectangle, int rowIndex, int colIndex, GridStyleInfo style)
		{
			if (Grid.CurrentCell.HasCurrentCellAt(rowIndex, colIndex) && CurrentCell.IsEditing)
			{
			    if (dateTimePicker.RightToLeft==RightToLeft.Yes)
			    {
                    dateTimePicker.DropDownAlign = LeftRightAlignment.Right;
			    }

                dateTimePicker.Size = clientRectangle.Size;
				dateTimePicker.CustomFormat = style.Format;
				dateTimePicker.Font = style.GdipFont;
				dateTimePicker.Location = clientRectangle.Location;
                dateTimePicker.Style = VisualStyle.Office2007;
                dateTimePicker.ContextMenu = new ContextMenu();
				dateTimePicker.Show();
				if (!dateTimePicker.ContainsFocus)
					dateTimePicker.Focus();
			}
			else
			{
				style.TextMargins.Left = 3;
				base.OnDraw(g, clientRectangle, rowIndex, colIndex, style);
			}
		}

		protected override void OnInitialize(int rowIndex, int colIndex)
		{
			GridStyleInfo style = Grid.Model[rowIndex, colIndex];
			dateTimePicker.Value = (DateTime) style.CellValue;
			
			base.OnInitialize(rowIndex, colIndex);
			dateTimePicker.ValueChanged += datePicker_ValueChanged;
            dateTimePicker.ShowDropButton = style.ShowButtons != GridShowButtons.Hide;
			dateTimePicker.Update();
		}

		protected override bool OnSaveChanges()
		{
			if (CurrentCell.IsModified)
			{
				Grid.Focus();
				GridStyleInfo style = Grid.Model[RowIndex, ColIndex];
				style.CellValue = dateTimePicker.Value;;
				return true;
			}
			return false;
		}
		
		protected override void OnDeactived(int rowIndex, int colIndex)
		{
			if(dateTimePicker.Visible)
			{
				dateTimePicker.Hide();
			}
			dateTimePicker.ValueChanged -= datePicker_ValueChanged;
			
		}

		private void datePicker_ValueChanged(object sender, EventArgs e)
		{
			CurrentCell.IsModified = true;
		}

        protected override void OnDoubleClick(int rowIndex, int colIndex, MouseEventArgs e)
		{
            CurrentCell.BeginEdit();
			base.OnClick(rowIndex, colIndex, e);
            if (e.Button == MouseButtons.Left)
			{
				ClickControl();
			}
		}

		private void ClickControl()
		{
			t = new Timer();
			t.Interval = 20;
			t.Tick += click;
			t.Start();
		}

		private void click(object sender, EventArgs e)
		{
			t.Stop();
			t.Tick -= click;
			Point p = dateTimePicker.PointToClient(Control.MousePosition);
			ActiveXSnapshot.FakeLeftMouseClick(dateTimePicker, p);
			t.Dispose();
			t = null;
		}
		
		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			if(!dateTimePicker.Focused)
			{
				dateTimePicker.Focus();
			}
			base.OnKeyPress(e);
		}
	}

	public class CustomDateTimePicker : Syncfusion.Windows.Forms.Tools.DateTimePickerAdv
	{
		protected override bool ProcessCmdKey(ref Message msg,Keys keyData)
		{
			if(keyData == Keys.Enter)
				return false;

			if(msg.Msg == 0x100 && keyData != Keys.Tab)
				OnValueChanged(EventArgs.Empty);

			return base.ProcessCmdKey(ref msg, keyData);
		}
	}
}