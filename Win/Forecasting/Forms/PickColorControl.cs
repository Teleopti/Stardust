using System;
using System.Drawing;
using Syncfusion.Windows.Forms.Tools;
using Teleopti.Ccc.Win.Common;

namespace Teleopti.Ccc.Win.Forecasting.Forms
{
    public partial class PickColorControl : BaseUserControl
    {
        public PickColorControl()
        {
            InitializeComponent();
            if (!DesignMode) SetTexts();
        }
        
        void dropDownBarItem2_BeforePopupItemPaint(object sender, Syncfusion.Windows.Forms.Tools.XPMenus.PopupItemPaintEventArgs drawItemInfo)
        {
            popupControlContainer1.Visible = true;
        }

        public event EventHandler<ColorPickerUIAdv.ColorPickedEventArgs> ColorChanged;
        private void colorPickerUIAdv1_Picked(object sender, ColorPickerUIAdv.ColorPickedEventArgs args)
        {
            //Set the GradientLabel color
            gradientLabel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(
                Syncfusion.Drawing.GradientStyle.Vertical, new System.Drawing.Color[]
                                                               {
                                                                   Color.WhiteSmoke, args.Color, Color.WhiteSmoke
                                                               });

            // Ensures that the PopupControlContainer is closed after the selection of a color.          
            ColorPickerUIAdv cc = sender as ColorPickerUIAdv;
            Syncfusion.Windows.Forms.PopupControlContainer pcc =
                cc.Parent as Syncfusion.Windows.Forms.PopupControlContainer;
            pcc.HidePopup(Syncfusion.Windows.Forms.PopupCloseType.Done);
            popupControlContainer1.Size = colorPickerUIAdv1.Size;

        	var handler = ColorChanged;
            if (handler != null)
            {
                handler.Invoke(this, args);
            }
        }

        public void SetEnabled(bool enable)
        {
            Enabled = enable;
        }
        //public void SetColor(Color color)
        //{
        //    this.gradientLabel1.BackgroundColor = new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical, new System.Drawing.Color[] {
        //   Color.WhiteSmoke, color, Color.WhiteSmoke});
        //}

        public Color ThisColor
        {
            get
            {
                
                 return gradientLabel1.BackgroundColor.GradientColors[1];
            }
            set
            {
              
                gradientLabel1.BackgroundColor =
                    new Syncfusion.Drawing.BrushInfo(Syncfusion.Drawing.GradientStyle.Vertical,
                                                     new System.Drawing.Color[]
                                                         {
                                                             Color.WhiteSmoke,    value, Color.WhiteSmoke
                                                         });
            }
        }
    }
}
