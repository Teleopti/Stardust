using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Syncfusion.Diagnostics;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Grid;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;


namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common.Controls.Cells
{
	[Serializable]
	public class GridDropDownMonthCalendarAdvCellModel : GridDropDownCellModel
	{
		private string noneButtonText;
		private string todayButtonText;
		private bool showNoneButton = true;
		private bool showTodayButton = true;

		public GridDropDownMonthCalendarAdvCellModel(GridModel grid)
			: base(grid)
		{
		}

		protected GridDropDownMonthCalendarAdvCellModel(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		public string NoneButtonText
		{
			get { return noneButtonText; }
		}

		public string TodayButtonText
		{
			get { return todayButtonText; }
		}

		public bool ShowNoneButton
		{
			get { return showNoneButton; }
		}

		public bool ShowTodayButton
		{
			get { return showTodayButton; }
		}

		public void SetNoneButtonText(string theText)
		{
			noneButtonText = theText;
		}
		public void SetTodayButtonText(string theText)
		{
			todayButtonText = theText;
		}
		public void HideNoneButton()
		{
			showNoneButton = false;
		}
		public void HideTodayButton()
		{
			showTodayButton = false;
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			info.AddValue("Text", GetActiveText(Grid.CurrentCellInfo.RowIndex, Grid.CurrentCellInfo.ColIndex));
			base.GetObjectData(info, context);
		}
		public override bool ApplyFormattedText(GridStyleInfo style, string text, int textInfo)
		{
			var e = new GridCellTextEventArgs(text, style, null, textInfo);
			Grid.RaiseSaveCellFormattedText(e);
			if (!e.Handled)
			{
				Grid.RaiseParseCommonFormats(e);
			}
			if (!e.Handled)
			{
				try
				{
					style.BeginUpdate();
					style.CellValue = getValueFromStyle(style, text);
					style.ResetError();
				}
				catch (Exception exception)
				{
					style.Error = exception.Message;
					if (style.StrictValueType)
					{
						//Ignore applicable functionality if StrictValueType, not promting to the user as an exception. If there is good way then need to proceed.
					}
					if (!(exception is FormatException) && !(exception.InnerException is FormatException))
					{
						//Ignore applicable functionality if FormatException, not promting to the user as an exception. If there is good way then need to proceed.
					}
					style.CellValue = text;
				}
				finally
				{
					style.EndUpdate();
				}
			}
			return true;
		}

		public override bool ApplyText(GridStyleInfo style, string text)
		{
			var e = new GridCellTextEventArgs(text, style, null, -1);
			Grid.RaiseSaveCellText(e);
			if (!e.Handled)
			{
				try
				{
					style.BeginUpdate();
					style.CellValue = !string.IsNullOrEmpty(text) ? getValueFromStyle(style, text) : text;
					style.ResetError();
				}
				catch (Exception exception)
				{
					style.Error = exception.Message;
					if (style.StrictValueType)
					{
						throw;
					}
					if (!(exception is FormatException) && !(exception.InnerException is FormatException))
					{
						throw;
					}
					style.CellValue = text;
				}
				finally
				{
					style.EndUpdate();
				}
			}
			return true;
		}

		public override GridCellRendererBase CreateRenderer(GridControlBase control)
		{
			var renderer = new GridDropDownMonthCalendarCellRenderer(control, this);
			renderer.SetNoneButtonText(NoneButtonText);
			renderer.SetTodayButtonText(TodayButtonText);
			if (!ShowTodayButton)
				renderer.HideTodayButton();
			if (!ShowNoneButton)
				renderer.HideNoneButton();
			return renderer;
		}

		public override string GetFormattedText(GridStyleInfo style, object value, int textInfo)
		{
			CultureInfo info = style.CultureInfo ?? CultureInfo.CurrentCulture;
			object valueFromStyle = getValueFromStyle(style, value);
			if (valueFromStyle is DateTime)
			{
				var time = (DateTime)valueFromStyle;
				return time.ToString(style.Format, info.DateTimeFormat);
			}
			return base.GetFormattedText(style, value, textInfo);
		}

		private static object getValueFromStyle(GridStyleInfo style, object value)
		{
			if (!(value is DateTime))
			{
				DateTime time;
				if (!(value is string) || (string.IsNullOrEmpty(value.ToString())))
				{
					return value;
				}
				string str = value.ToString();
				CultureInfo info = style.CultureInfo ?? CultureInfo.CurrentCulture;
				if (!string.IsNullOrEmpty(style.Format))
				{
					return DateTime.ParseExact(str, new[] { style.Format, "G", "g", "f", "F" }, info.DateTimeFormat, 0).LimitMin();
				}
				if (DateTime.TryParse(str, CultureInfo.CurrentCulture.DateTimeFormat, 0, out time))
				{
					return time.LimitMin();
				}
				if (DateTime.TryParse(str, info.DateTimeFormat, 0, out time))
				{
					return time.LimitMin();
				}
			}
			return ((DateTime)value).LimitMin();
		}
	}

	public class GridDropDownMonthCalendarCellRenderer : GridDropDownCellRenderer
	{
		// Fields
		//private MonthCalendar calendar;
		private MonthCalendarAdv calendar;
		private Size calendarControlSize = Size.Empty;
		private string noneButtonText;
		private string todayButtonText;
		private bool showNoneButton = true;
		private bool showTodayButton = true;

		public void SetNoneButtonText(string theText)
		{
			noneButtonText = theText;
			setCalenderStyle();
		}

		public void SetTodayButtonText(string theText)
		{
			todayButtonText = theText;
			setCalenderStyle();
		}

		public void HideNoneButton()
		{
			showNoneButton = false;
			setCalenderStyle();
		}
		public void HideTodayButton()
		{
			showTodayButton = false;
			setCalenderStyle();
		}

		private void setCalenderStyle()
		{
			if (Calendar == null)
				return;

			if (!string.IsNullOrEmpty(noneButtonText))
				Calendar.NoneButton.Text = noneButtonText;

			if (!string.IsNullOrEmpty(todayButtonText))
				Calendar.TodayButton.Text = todayButtonText;

			Calendar.NoneButton.Visible = showNoneButton;
			Calendar.TodayButton.Visible = showTodayButton;
		}

		public GridDropDownMonthCalendarCellRenderer(GridControlBase grid, GridCellModelBase cellModel)
			: base(grid, cellModel)
		{
			DropDownButton = new GridCellComboBoxButton(this);
			DropDownImp.InitFocusEditPart = true;
		}

		private void calendarDateSelected(object sender, EventArgs e)
		{
			CurrentCell.CloseDropDown(PopupCloseType.Done);
		}

		public override void ChildClosing(IPopupChild childUi, PopupCloseType popupCloseType)
		{

			if ((popupCloseType == PopupCloseType.Done) && !IsReadOnly())
			{
				if (!NotifyCurrentCellChanging())
				{
					return;
				}
				DateTime selectionStart = Calendar.Value; // SelectionStart;
				if (StyleInfo.CellValueType == typeof(DateTime))
				{
					ControlValue = selectionStart;
				}
				else
				{
					ControlValue = selectionStart.ToShortDateString();
				}
				TextBox.Select(0, 0);
				TextBox.Modified = true;
				TextBox.Focus();
			}

			base.ChildClosing(childUi, popupCloseType);
		}

		public override void DropDownContainerShowingDropDown(object sender, CancelEventArgs e)
		{
			var size = new Size(CalendarControlSize.Width + 2, CalendarControlSize.Height + 2);
			var args = new GridCurrentCellShowingDropDownEventArgs(size);
			Grid.RaiseCurrentCellShowingDropDown(args);
			if (args.Cancel)
			{
				e.Cancel = true;
			}
			else
			{
				DropDownContainer.Size = args.Size;
				try
				{
					UpdateControlValue();
					object controlValue = ControlValue;
					if (controlValue is DateTime)
					{
						Calendar.Value = (DateTime)controlValue;
					}
					else if (TextBox.Text.Length > 0)
					{
						Calendar.Value = DateTime.Parse(TextBox.Text, CultureInfo.CurrentCulture);
					}
					else
					{
						Calendar.Value = DateTime.Now;
					}
				}
				catch (Exception exception)
				{
					TraceUtil.TraceExceptionCatched(exception);
					if (!ExceptionManager.RaiseExceptionCatched(this, exception))
					{
						throw;
					}
				}
			}
		}

		public override void DropDownContainerCloseDropDown(object sender, PopupClosedEventArgs e)
		{
			var container = sender as GridDropDownContainer;
			if (container == null) return;

			var gridControl = ((GridControl)container.ParentControl.Parent);
			gridControl.CurrentCell.EndEdit();
			// Workaround so edit other cells will work just after selecting a date
			gridControl.CurrentCell.MoveRight();
			gridControl.CurrentCell.MoveLeft();
		}

		protected override void InitializeDropDownContainer()
		{
			base.InitializeDropDownContainer();
			calendar = new MonthCalendarAdv
			{
				Culture = CultureInfo.CurrentCulture,
				Dock = DockStyle.Fill,
				Style = VisualStyle.Metro,
				MinValue = DateHelper.MinSmallDateTime,
				Visible = true,
				BackColor =Color.White
			};

			setCalenderStyle();

			calendar.DateSelected += calendarDateSelected;
			calendar.NoneButtonClick += calendarNoneButtonClick;
			calendar.VisibleChanged += calendarVisibleChanged;
			calendar.Width = 250;
			DropDownContainer.Size = calendar.Size;
			DropDownContainer.Controls.Add(Calendar);
		}

		void calendarVisibleChanged(object sender, EventArgs e)
		{
			Calendar.Refresh();
		}

		void calendarNoneButtonClick(object sender, EventArgs e)
		{
			TextBox.Text = "";
			CurrentCell.CloseDropDown(PopupCloseType.Canceled);
		}

		public Size CalendarControlSize
		{
			get
			{
				if (calendarControlSize.IsEmpty)
				{
					var window = new TopLevelWindow {Location = new Point(0x2710, 0x2710), Size = new Size(500, 500)};
					window.ShowWindowTopMost();
					var monthCalendarAdv = new MonthCalendarAdv {Style = VisualStyle.Metro, Width = 250};
					window.Controls.Add(monthCalendarAdv);
					window.Visible = true;
					calendarControlSize = monthCalendarAdv.Size;
					window.Dispose();
				}
				return calendarControlSize;
			}
		}

		public MonthCalendarAdv Calendar
		{
			get { return calendar; }
		}
	}
}
