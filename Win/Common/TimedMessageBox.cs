using System;
using System.Windows.Forms;
using System.Globalization;

namespace Teleopti.Ccc.Win.Common
{
    /// <summary>
    /// Timed message box.
    /// </summary>
    public partial class TimedMessageBox : Form
    {

        #region Static methods

        private static TimedMessageBox _messageBox;

        /// <summary>
        /// Shows the message box.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <returns></returns>
        public static DialogResult Show(string text)
        {
            return Show(text, 30);
        }

        /// <summary>
        /// Shows the message box.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="timeoutSecond">The timeout second.</param>
        /// <returns></returns>
        public static DialogResult Show(string text, int timeoutSecond)
        {
            DialogResult res;
            _messageBox = new TimedMessageBox(timeoutSecond);
            _messageBox.lblMessage.Text = text;
            _messageBox.ShowDialog();
            res = _messageBox.Result;
            _messageBox.Dispose();
            return res;
        }

        /// <summary>
        /// Shows the message box.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="caption">The caption.</param>
        /// <returns></returns>
        public static DialogResult Show(string text, string caption)
        {
            return Show(text, caption, 30);
        }

        /// <summary>
        /// Shows the message box.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="caption">The caption.</param>
        /// <param name="timeoutSecond">The timeout second.</param>
        /// <returns></returns>
        public static DialogResult Show(string text, string caption, int timeoutSecond)
        {
            DialogResult res;
            _messageBox = new TimedMessageBox(timeoutSecond);
            _messageBox.Text = caption;
            _messageBox.lblMessage.Text = text;
            _messageBox.ShowDialog();
            res = _messageBox.Result;
            _messageBox.Dispose();
            return res;
        }

        //public static DialogResult Show(string text, uint timeoutSecond)
        //{
        //    Setup("", timeoutSecond);
        //    return MessageBox.Show(text);
        //}

        //public static DialogResult Show(string text, string caption, uint timeoutSecond)
        //{
        //    Setup(caption, timeoutSecond);
        //    return MessageBox.Show(text, caption);
        //}

        //public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, uint timeoutSecond)
        //{
        //    Setup(caption, timeoutSecond);
        //    return MessageBox.Show(text, caption, buttons);
        //}

        //public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, uint timeoutSecond)
        //{
        //    Setup(caption, timeoutSecond);
        //    return MessageBox.Show(text, caption, buttons, icon);
        //}

        //public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defButton, uint timeoutSecond)
        //{
        //    Setup(caption, timeoutSecond);
        //    return MessageBox.Show(text, caption, buttons, icon, defButton);
        //}

        //public static DialogResult Show(string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defButton, MessageBoxOptions options, uint timeoutSecond)
        //{
        //    Setup(caption, timeoutSecond);
        //    return MessageBox.Show(text, caption, buttons, icon, defButton, options);
        //}

        //public static DialogResult Show(IWin32Window owner, string text, uint timeoutSecond)
        //{
        //    Setup("", timeoutSecond);
        //    return MessageBox.Show(owner, text);
        //}

        //public static DialogResult Show(IWin32Window owner, string text, string caption, uint timeoutSecond)
        //{
        //    Setup(caption, timeoutSecond);
        //    return MessageBox.Show(owner, text, caption);
        //}

        //public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, uint timeoutSecond)
        //{
        //    Setup(caption, timeoutSecond);
        //    return MessageBox.Show(owner, text, caption, buttons);
        //}

        //public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, uint timeoutSecond)
        //{
        //    Setup(caption, timeoutSecond);
        //    return MessageBox.Show(owner, text, caption, buttons, icon);
        //}

        //public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defButton, uint timeoutSecond)
        //{
        //    Setup(caption, timeoutSecond);
        //    return MessageBox.Show(owner, text, caption, buttons, icon, defButton);
        //}

        //public static DialogResult Show(IWin32Window owner, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, MessageBoxDefaultButton defButton, MessageBoxOptions options, uint timeoutSecond)
        //{
        //    Setup(caption, timeoutSecond);
        //    return MessageBox.Show(owner, text, caption, buttons, icon, defButton, options);
        //}



        #endregion

        #region Variables

        private Timer _timer;
        private int _timeoutCounter;
        private DialogResult _res;

        #endregion

        #region Interface

        /// <summary>
        /// Gets the result.
        /// </summary>
        /// <value>The result.</value>
        public DialogResult Result
        {
            get { return _res; }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="TimedMessageBox"/> class.
        /// </summary>
        private TimedMessageBox()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TimedMessageBox"/> class.
        /// </summary>
        /// <param name="timeoutSecond">The timeout second.</param>
        private TimedMessageBox(int timeoutSecond) : this()
        {
            _timeoutCounter = timeoutSecond;
        }

        #endregion

        #region Component events

        /// <summary>
        /// Handles the Load event of the Form control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void Form_Load(object sender, EventArgs e)
        {
            _res = DialogResult.Yes;
            picIcon.Image = System.Drawing.SystemIcons.Question.ToBitmap();
            _messageBox.lblTimer.Text = "Close in " + _timeoutCounter.ToString(CultureInfo.CurrentCulture) + " seconds with the result of: " + _res;
            _timer = new Timer();
            _timer.Interval = 1000;
            _timer.Enabled = true;
            _timer.Start();
            _timer.Tick += new System.EventHandler(this.timer_tick);
            
        }

        /// <summary>
        /// Handles the Click event of the btnOK control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnOK_Click(object sender, EventArgs e)
        {
            _messageBox._timer.Stop();
            _messageBox._timer.Dispose();
            _res = DialogResult.Yes;
            Close();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            _messageBox._timer.Stop();
            _messageBox._timer.Dispose();
            _res = DialogResult.No;
            Close();
        }

        /// <summary>
        /// Handles the tick event of the timer control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void timer_tick(object sender, EventArgs e)
        {
            _timeoutCounter--;

            if (_timeoutCounter >= 0)
            {
                _messageBox.lblTimer.Text = "Close in " + _timeoutCounter.ToString(CultureInfo.CurrentCulture) + " seconds with the result of: " + _res;
            }
            else
            {
                _messageBox._timer.Stop();
                _messageBox._timer.Dispose();
                Close();
            }
        }

        #endregion

    }
}