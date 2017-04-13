using System;
using System.Drawing;
using System.Windows.Forms;
using Point=System.Drawing.Point;

namespace Teleopti.Ccc.SmartClientPortal.Shell.Win.Common
{
    public partial class TimedWarningDialog : BaseDialogForm
    {

        private Timer _timer;
        private int _timeoutCounter;
        private string _messageShown;
        private Control _showMeNearThisControl;

        public  TimedWarningDialog(int numberOfSecondsShown,string messageShown, object showOverThisControl)
        {
            Setup();

            _timeoutCounter = numberOfSecondsShown;
            _showMeNearThisControl = (Control) showOverThisControl;
            _messageShown = messageShown;
        }

        public TimedWarningDialog()
        {
            Setup();
        }

        private void Setup()
        {
            InitializeComponent();
            this.Load += new EventHandler(TimedWarningDialog_Load);
        }


        ///// <summary>
        ///// Draws the visual style element tool tip balloon.
        ///// shows a balloon with the text set by the consumer
        ///// </summary>
        ///// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
        ///// <remarks>
        ///// Created by: ostenpe
        ///// Created date: 2008-04-30
        ///// </remarks>
        //private  void DrawBalloon(PaintEventArgs e)
        //{
        //    if (VisualStyleRenderer.IsSupported && 
        //        VisualStyleRenderer.IsElementDefined(
        //        VisualStyleElement.ToolTip.Balloon.Link))
        //    {
        //        VisualStyleRenderer renderer =
        //             new VisualStyleRenderer(VisualStyleElement.ToolTip.Balloon.Link);
        //        Rectangle rectangle1 = new Rectangle(1,1,198, 98);
        //        renderer.DrawBackground(e.Graphics, rectangle1);
        //        RectangleF myTextRectangle = new RectangleF(10,10,180,70);
        //        //todo stringformat is needed I guess
        //        e.Graphics.DrawString(_messageShown ,
        //             this.Font, Brushes.Black,myTextRectangle );
        //    }
        //    else
        //        e.Graphics.DrawString(_messageShown + " /n no visual support for this feature - needs xp and above",
        //             this.Font, Brushes.Black, new Point(10, 10));
        //}




        /// <summary>
        /// Handles the Load event of the TimedWarningDialog control.
        /// sets and start the timer
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-04-30
        /// </remarks>
         void TimedWarningDialog_Load(object sender, EventArgs e)
        {
           SetLocation();
            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Enabled = true;
            _timer.Start();
            _timer.Tick += new System.EventHandler(this.timer_tick);
            ShowText();
        }

         /// <summary>
         /// Shows the text sent to the control - the ballon was not supported on xp as promised...
         /// </summary>
         /// <remarks>
         /// Created by: ostenpe
         /// Created date: 2008-06-17
         /// </remarks>
        private void ShowText()
        {
            if(_messageShown != null)this.labelWarning.Text = this._messageShown;
            else
            {
                this.labelWarning.Text = "warning text is missing";
            }
        }


        /// <summary>
            /// Sets the location of the warning. default is 400x400
            /// </summary>
            /// <remarks>
            /// Created by: ostenpe
            /// Created date: 2008-04-23
            /// </remarks>
            private void SetLocation()
            {
                Point _spot = new Point();
                _spot.X = 400;
                _spot.Y = 400;
                if(this.WarningShownNearThisControl != null)
                {
                    Rectangle o = WarningShownNearThisControl.RectangleToScreen(WarningShownNearThisControl.ClientRectangle);
                //todo resizable warning, smart centered
                _spot.Y = o.Top - this.Height; // it should be shown above the control
                _spot.X = o.Left;//- this.Width ;        //no longer centered 
                }
                  this.Location  = _spot;   
              }


        /// <summary>
        /// Gets or sets the number of seconds the warning will be shown.
        /// </summary>
        /// <value>The number of seconds shown.</value>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-04-23
        /// </remarks>
        public int WarningShownInSeconds
        {
            get { return _timeoutCounter; }
            set { _timeoutCounter = value; }
        }
        /// <summary>
        /// Gets or sets the message shown.
        /// </summary>
        /// <value>The message shown.</value>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-04-23
        /// </remarks>
        public string WarningMessageShown
        {
            get { return _messageShown; }
            set { _messageShown = value; }
        }

        /// <summary>
        /// the warning to appear above the control sent.
        /// </summary>
        /// <value>The show me near this control.</value>
        /// <remarks>
        /// Created by: ostenpe
        /// Created date: 2008-04-23
        /// </remarks>
        public Control WarningShownNearThisControl
        {
            get { return _showMeNearThisControl; }
            set { _showMeNearThisControl = value; }
        }


        private void timer_tick(object sender, EventArgs e)
        {
            _timeoutCounter--;

            if (_timeoutCounter <= 0)
            {
                this._timer.Stop();
                this._timer.Dispose();
                Close();
            }
        }

        //private void TimedWarningDialog_Paint(object sender, PaintEventArgs e)
        //{
        //    this.DrawBalloon(e);
        //}


    }

}
