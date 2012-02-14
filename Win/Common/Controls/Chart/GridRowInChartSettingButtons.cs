using System;
using System.Drawing;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Common.Controls.Chart
{
    /// <summary>
    /// pretty specific buttons for charthandling
    /// </summary>
    /// <remarks>
    /// Created by: ostenpe
    /// Created date: 2008-06-27
    /// </remarks>
    public partial class GridRowInChartSettingButtons : UserControl
    {
        /// <summary>
        /// Occurs when [any button in this control is clicked - the args contains all settings but only the one changed will be set].
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-06-27
        /// </remarks>
        public event EventHandler<GridlineInChartButtonEventArgs> LineInChartSettingsChanged;

        public event EventHandler<GridlineInChartButtonEventArgs> LineInChartEnabledChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="GridRowInChartSettingButtons"/> class.
        /// </summary>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-06-27
        /// </remarks>
        public GridRowInChartSettingButtons()
        {
            InitializeComponent();
            pickColorControl1.ColorChanged += pickColorControl1_ColorChanged;
        }

     
        #region events [contains all events from the different buttons - pretty hardwired no thrills]
        /// <summary>
        /// Handles the ColorChanged event of the pickColorControl1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Syncfusion.Windows.Forms.Tools.ColorPickerUIAdv.ColorPickedEventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-06-27
        /// </remarks>
        private void pickColorControl1_ColorChanged(object sender, ColorPickerUIAdv.ColorPickedEventArgs e)
        {
            if (LineInChartSettingsChanged == null) return;
            if (isEnabled == false) return;
            var args = new GridlineInChartButtonEventArgs
                           {
                               Enabled = true,
                               GridToChartAxis = getselectedAxis() ,
                               ChartSeriesStyle = getselectedStyle(),
                               LineColor = e.Color
                           };          
            LineInChartSettingsChanged.Invoke(this, args);
            this.Refresh();
        }      

        private void buttonAdvRightAxis_Click(object sender, EventArgs e)
        {
            if (LineInChartSettingsChanged == null) return;
            if (isEnabled == false) return;
            var args = new GridlineInChartButtonEventArgs 
                           {
                               Enabled = true,
                               ChartSeriesStyle = getselectedStyle(),
                               GridToChartAxis = AxisLocation.Right ,
                               LineColor = getcolor()
                           };
            this.buttonAdvRightAxis.State = ButtonAdvState.Pressed;
            this.buttonAdvLeftAxis.State = ButtonAdvState.Default;
            LineInChartSettingsChanged.Invoke(this, args);
            this.Refresh();

        }

        private void buttonAdvLeftAxis_Click(object sender, EventArgs e)
        {
            if (LineInChartSettingsChanged == null) return;
            if (isEnabled == false) return;
            var args = new GridlineInChartButtonEventArgs
                           {
                               Enabled = true,
                               ChartSeriesStyle = getselectedStyle(),
                               GridToChartAxis = AxisLocation.Left ,                            
                               LineColor = getcolor()
                           };
            this.buttonAdvRightAxis.State = ButtonAdvState.Default ;
            this.buttonAdvLeftAxis.State = ButtonAdvState.Pressed  ;
            LineInChartSettingsChanged.Invoke(this, args);
            this.Refresh();
        }

        //private void checkBoxAdvShowRow_Click(object sender, EventArgs e)
        //{
        //    if (LineInChartEnabledChanged == null) return;

        //    var args = new GridlineInChartButtonEventArgs
        //                   {
        //                       enabled =isEnabled
        //    };

        //    LineInChartEnabledChanged.Invoke(this, args);
        //    this.Refresh();
        //}

        private void buttonAdvBar_Click(object sender, EventArgs e)
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
            this.buttonAdvBar.State = ButtonAdvState.Pressed;
            this.buttonAdvLine.State = ButtonAdvState.Default;
            LineInChartSettingsChanged.Invoke(this, args);
            this.Refresh();
        }

        private void buttonAdvLine_Click(object sender, EventArgs e)
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
            this.buttonAdvBar.State = ButtonAdvState.Default;
            this.buttonAdvLine.State = ButtonAdvState.Pressed;
            LineInChartSettingsChanged.Invoke(this,args);
            this.Refresh();
        }

        //-------------------
        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (LineInChartEnabledChanged == null) return;
            var args = new GridlineInChartButtonEventArgs
                           {
                               Enabled =isEnabled
                           };

            checkBox1.ImageIndex = isEnabled ? 1 : 0;//testa mera
            LineInChartEnabledChanged.Invoke(this, args);
            this.Refresh();
        
        }

        private bool isEnabled
        {


            get
            {
                return this.checkBox1.Checked;
            }
            //get
            //{
            //    return this.checkBoxAdvShowRow.CheckState == CheckState.Checked;
            //}
        }


        private Color getcolor()
        {
            return this.pickColorControl1.ThisColor;
        }

        private ChartSeriesDisplayType getselectedStyle()
        {
            if (buttonAdvBar.State == ButtonAdvState.Pressed || buttonAdvBar.State == (ButtonAdvState.Pressed | ButtonAdvState.Default)) return ChartSeriesDisplayType.Bar;
            else return ChartSeriesDisplayType.Line;
        }

        private AxisLocation getselectedAxis()
        {
            //In some cases the button seems both pressed and default?!? which made the chart to change axis of the series
            if (buttonAdvLeftAxis.State == ButtonAdvState.Pressed || buttonAdvLeftAxis.State == (ButtonAdvState.Pressed | ButtonAdvState.Default)) return AxisLocation.Left;
            else 
                return AxisLocation.Right;
        }

        #endregion

        #region public function - must be called or the state shown will be out of sync
        /// <summary>
        /// Sets the buttons of the control 
        /// </summary>
        /// <param name="enabled">if set to <c>false</c> [all other buttons are disabled and the eye is closed].</param>
        /// <param name="axis">The selected axis.(left/right)</param>
        /// <param name="style">The selected style.(bar/line)</param>
        /// <param name="color">The color of the selected gridrow line.</param>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-06-27
        /// </remarks>
        public void SetButtons(bool enabled, AxisLocation axis, ChartSeriesDisplayType style, Color color)
        {
            SetShowRowButtonWithSuspendedEvents(enabled);
 
            if (enabled == false)
                DisableButtons();
            else
            {
                EnableButtons();
                SetAxisButtons(axis);
                SetStyleButtons(style);
                SetpickColor(color);
            }
            this.Refresh();
        }



        #endregion

        #region private setters [contains the setters called by SetButtons]

        private void EnableButtons()
        {
            this.buttonAdvBar.Enabled = true;
            this.buttonAdvLine.Enabled = true;
            this.buttonAdvRightAxis.Enabled = true;
            this.buttonAdvLeftAxis.Enabled = true;
            this.pickColorControl1.SetEnabled(true);
        }

        private void DisableButtons()
        {
            this.buttonAdvBar.Enabled = false;
            this.buttonAdvLine.Enabled = false;
            this.buttonAdvRightAxis.Enabled = false;
            this.buttonAdvLeftAxis.Enabled = false;
            this.pickColorControl1.SetEnabled(false);
        }


        private void SetShowRowButtonWithSuspendedEvents(bool enabled)
        {
            checkBox1.CheckedChanged -= checkBox1_CheckedChanged;
            checkBox1.Checked = enabled;
            checkBox1.ImageIndex = enabled ? 1 : 0;//testa mera
            checkBox1.CheckedChanged += checkBox1_CheckedChanged;
        }

        private void SetpickColor(Color color)
        {
            this.pickColorControl1.ThisColor = color;
        }

        private void SetStyleButtons(ChartSeriesDisplayType style)
        {
            if (style == ChartSeriesDisplayType.Bar)
            {
                buttonAdvLine.State = ButtonAdvState.Default;
                buttonAdvBar.State = ButtonAdvState.Pressed;

            }
            else
            {
                buttonAdvLine.State = ButtonAdvState.Pressed;
                buttonAdvBar.State = ButtonAdvState.Default ;
            }
        }

        private void SetAxisButtons(AxisLocation axis)
        {
            if (axis == AxisLocation.Left)
            {
                buttonAdvLeftAxis.State = ButtonAdvState.Pressed;
                buttonAdvRightAxis.State = ButtonAdvState.Default;
            }
            else
            {
                buttonAdvLeftAxis.State = ButtonAdvState.Default;
                buttonAdvRightAxis.State = ButtonAdvState.Pressed;
            }
        }
        #endregion

        private void GridRowInChartSettingButtons_Load(object sender, EventArgs e)
        {

        }

        /// <summary>
        /// Sets the buttons with initial values.
        /// </summary>
        /// <remarks>
        /// Created by: robink
        /// Created date: 2008-09-24
        /// </remarks>
        public void SetButtons()
        {
            SetButtons(false, AxisLocation.Left, ChartSeriesDisplayType.Line, Color.WhiteSmoke); 
        }
    }
}