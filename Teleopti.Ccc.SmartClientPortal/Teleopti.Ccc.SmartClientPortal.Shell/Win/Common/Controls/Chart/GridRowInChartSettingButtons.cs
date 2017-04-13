using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Chart
{
	public partial class GridRowInChartSettingButtons : UserControl
	{
		public event EventHandler<GridlineInChartButtonEventArgs> LineInChartSettingsChanged;

		public event EventHandler<GridlineInChartButtonEventArgs> LineInChartEnabledChanged;

		public GridRowInChartSettingButtons()
		{
			InitializeComponent();
			pickColorControl1.ColorChanged += pickColorControl1ColorChanged;
		}


		private void pickColorControl1ColorChanged(object sender, ColorPickerUIAdv.ColorPickedEventArgs e)
		{
			if (LineInChartSettingsChanged == null) return;
			if (isEnabled == false) return;
			var args = new GridlineInChartButtonEventArgs
								{
									Enabled = true,
									GridToChartAxis = getselectedAxis(),
									ChartSeriesStyle = getselectedStyle(),
									LineColor = e.Color
								};
			LineInChartSettingsChanged.Invoke(this, args);
			Refresh();
		}

		private void buttonAdvRightAxisClick(object sender, EventArgs e)
		{
			if (LineInChartSettingsChanged == null) return;
			if (isEnabled == false) return;
			var args = new GridlineInChartButtonEventArgs
								{
									Enabled = true,
									ChartSeriesStyle = getselectedStyle(),
									GridToChartAxis = AxisLocation.Right,
									LineColor = getcolor()
								};
			buttonAdvRightAxis.State = ButtonAdvState.Pressed;
			buttonAdvRightAxis.BackColor = Color.FromArgb(0, 153, 255);
			buttonAdvLeftAxis.State = ButtonAdvState.Default;
			buttonAdvLeftAxis.BackColor = Color.White;
			LineInChartSettingsChanged.Invoke(this, args);
			Refresh();

		}

		private void buttonAdvLeftAxisClick(object sender, EventArgs e)
		{
			if (LineInChartSettingsChanged == null) return;
			if (isEnabled == false) return;
			var args = new GridlineInChartButtonEventArgs
								{
									Enabled = true,
									ChartSeriesStyle = getselectedStyle(),
									GridToChartAxis = AxisLocation.Left,
									LineColor = getcolor()
								};
			buttonAdvRightAxis.State = ButtonAdvState.Default;
			buttonAdvRightAxis.BackColor = Color.White;
			buttonAdvLeftAxis.State = ButtonAdvState.Pressed;
			buttonAdvLeftAxis.BackColor = Color.FromArgb(0, 153, 255);
			LineInChartSettingsChanged.Invoke(this, args);
			Refresh();
		}

		private void buttonAdvBarClick(object sender, EventArgs e)
		{
			if (LineInChartSettingsChanged == null) return;
			if (isEnabled == false) return;
			var args = new GridlineInChartButtonEventArgs
								{
									Enabled = true,
									ChartSeriesStyle = ChartSeriesDisplayType.Bar,
									GridToChartAxis = getselectedAxis(),
									LineColor = getcolor()
								};
			buttonAdvBar.State = ButtonAdvState.Pressed;
			buttonAdvBar.BackColor = Color.FromArgb(0, 153, 255);
			buttonAdvLine.State = ButtonAdvState.Default;
			buttonAdvLine.BackColor = Color.White;
			LineInChartSettingsChanged.Invoke(this, args);
			Refresh();
		}

		private void buttonAdvLineClick(object sender, EventArgs e)
		{
			if (LineInChartSettingsChanged == null) return;
			if (isEnabled == false) return;
			var args = new GridlineInChartButtonEventArgs
								{
									Enabled = true,
									ChartSeriesStyle = ChartSeriesDisplayType.Line,
									GridToChartAxis = getselectedAxis(),
									LineColor = getcolor()
								};
			buttonAdvBar.State = ButtonAdvState.Default;
			buttonAdvBar.BackColor = Color.White;
			buttonAdvLine.State = ButtonAdvState.Pressed;
			buttonAdvLine.BackColor = Color.FromArgb(0, 153, 255);
			LineInChartSettingsChanged.Invoke(this, args);
			Refresh();
		}

		private void checkBox1CheckedChanged(object sender, EventArgs e)
		{
			if (LineInChartEnabledChanged == null) return;
			var args = new GridlineInChartButtonEventArgs
								{
									Enabled = isEnabled
								};

			checkBox1.ImageIndex = isEnabled ? 1 : 0;//testa mera
			LineInChartEnabledChanged.Invoke(this, args);
			Refresh();

		}

		private bool isEnabled
		{
			get
			{
				return checkBox1.Checked;
			}
		}


		private Color getcolor()
		{
			return pickColorControl1.ThisColor;
		}

		private ChartSeriesDisplayType getselectedStyle()
		{
			if (buttonAdvBar.State == ButtonAdvState.Pressed || buttonAdvBar.State == (ButtonAdvState.Pressed | ButtonAdvState.Default))
				return ChartSeriesDisplayType.Bar;
			return ChartSeriesDisplayType.Line;
		}

		private AxisLocation getselectedAxis()
		{
			//In some cases the button seems both pressed and default?!? which made the chart to change axis of the series
			if (buttonAdvLeftAxis.State == ButtonAdvState.Pressed || buttonAdvLeftAxis.State == (ButtonAdvState.Pressed | ButtonAdvState.Default))
				return AxisLocation.Left;
			return AxisLocation.Right;
		}

		public void SetButtons(bool enabled, AxisLocation axis, ChartSeriesDisplayType style, Color color)
		{
			setShowRowButtonWithSuspendedEvents(enabled);

			if (enabled == false)
				disableButtons();
			else
			{
				enableButtons();
				setAxisButtons(axis);
				setStyleButtons(style);
				setpickColor(color);
			}
			Refresh();
		}

		private void enableButtons()
		{
			buttonAdvBar.Enabled = true;
			buttonAdvLine.Enabled = true;
			buttonAdvRightAxis.Enabled = true;
			buttonAdvLeftAxis.Enabled = true;
			pickColorControl1.SetEnabled(true);
		}

		private void disableButtons()
		{
			buttonAdvBar.Enabled = false;
			buttonAdvLine.Enabled = false;
			buttonAdvRightAxis.Enabled = false;
			buttonAdvLeftAxis.Enabled = false;
			pickColorControl1.SetEnabled(false);
		}


		private void setShowRowButtonWithSuspendedEvents(bool enabled)
		{
			checkBox1.CheckedChanged -= checkBox1CheckedChanged;
			checkBox1.Checked = enabled;
			checkBox1.ImageIndex = enabled ? 1 : 0;//testa mera
			checkBox1.CheckedChanged += checkBox1CheckedChanged;
		}

		private void setpickColor(Color color)
		{
			pickColorControl1.ThisColor = color;
		}

		private void setStyleButtons(ChartSeriesDisplayType style)
		{
			if (style == ChartSeriesDisplayType.Bar)
			{
				buttonAdvLine.State = ButtonAdvState.Default;
				buttonAdvLine.BackColor = Color.White;
				buttonAdvBar.State = ButtonAdvState.Pressed;
				buttonAdvBar.BackColor = Color.FromArgb(0, 153, 255);
			}
			else
			{
				buttonAdvLine.State = ButtonAdvState.Pressed;
				buttonAdvLine.BackColor =  Color.FromArgb(0, 153, 255);
				buttonAdvBar.State = ButtonAdvState.Default;
				buttonAdvBar.BackColor = Color.White;
			}
		}

		private void setAxisButtons(AxisLocation axis)
		{
			if (axis == AxisLocation.Left)
			{
				buttonAdvLeftAxis.State = ButtonAdvState.Pressed;
				buttonAdvLeftAxis.BackColor = Color.FromArgb(0, 153, 255);
				buttonAdvRightAxis.State = ButtonAdvState.Default;
				buttonAdvRightAxis.BackColor = Color.White;
			}
			else
			{
				buttonAdvLeftAxis.State = ButtonAdvState.Default;
				buttonAdvLeftAxis.BackColor = Color.White;
				buttonAdvRightAxis.State = ButtonAdvState.Pressed;
				buttonAdvRightAxis.BackColor = Color.FromArgb(0, 153, 255);
			}
		}

		public void SetButtons()
		{
			SetButtons(false, AxisLocation.Left, ChartSeriesDisplayType.Line, Color.WhiteSmoke);
		}
	}
}