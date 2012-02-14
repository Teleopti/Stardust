using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Permissions
{

    #region Enumerations

    /// <summary>
    /// AuthorizationStepControl connection point enumeration.  
    /// </summary>
    public enum ConnectionSide
    {
        /// <summary>
        /// Connection point at the top.
        /// </summary>
        Top,
        /// <summary>
        /// Connection point at the right hand side.
        /// </summary>
        Right,
        /// <summary>
        /// Connection point at the bottom.
        /// </summary>
        Bottom,
        /// <summary>
        /// Connection point at the left hand side.
        /// </summary>
        Left
    }

    /// <summary>
    /// AuthorizationStepControl status enumeration.  
    /// </summary>
    public enum ControlStatus
    {
        /// <summary>
        /// AuthorizationStep works fine.
        /// </summary>
        Ok,
        /// <summary>
        /// AuthorizationStep is disabled.
        /// </summary>
        Disabled,
        /// <summary>
        /// Fatal exception in AuthorizationStep functioning.
        /// </summary>
        Error,
        /// <summary>
        /// Warning in AuthorizationStep functioning.
        /// </summary>
        Warning
    }

    #endregion

    public partial class AuthorizationStepPresenterControl : UserControl
    {
        private IAuthorizationStep _authorizationStep;
        private ControlStatus _authorizationStepStatus;
        private AuthorizationStepResultListScreen _resultScreen;
        private bool _resultScreenPermanent;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationStepPresenterControl"/> class.
        /// </summary>
        public AuthorizationStepPresenterControl()
        {
            InitializeComponent();
        }

        #region Interface

        /// <summary>
        /// Gets or sets the authorization step that the control represents.
        /// </summary>
        /// <value>The authorization step.</value>
        [Browsable(false)]
        public ControlStatus ControlStatus
        {
            get { return _authorizationStepStatus; }
            set
            {
                if (!DesignMode)
                {
                    _authorizationStepStatus = value;
                    switch (value)
                    {
                        case ControlStatus.Ok:
                            pictureBoxStatus.Image = SystemIcons.Information.ToBitmap();
                            break;
                        case ControlStatus.Disabled:
                        case ControlStatus.Warning:
                            pictureBoxStatus.Image = SystemIcons.Exclamation.ToBitmap();
                            break;
                        case ControlStatus.Error:
                            pictureBoxStatus.Image = SystemIcons.Error.ToBitmap();
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the authorization step that the control represents.
        /// </summary>
        /// <value>The authorization step.</value>
        [Browsable(false)]
        public IAuthorizationStep AuthorizationStep
        {
            get { return _authorizationStep; }
            set
            {
                SetAuthorizationStep(value);
            }
        }

        /// <summary>
        /// Sets the authorization step.
        /// </summary>
        /// <param name="value">The value.</param>
        private void SetAuthorizationStep(IAuthorizationStep value)
        {
            if (!DesignMode && value != null)
            {
                _authorizationStep = value;
                LabelText = value.PanelName;
                ToolTipDescriptionText = value.PanelDescription;
                if (!value.Enabled)
                {
                    ControlStatus = Permissions.ControlStatus.Disabled;
                    ToolTipStatusText = "xx Disabled by user!";
                    
                }
                else if (value.InnerException != null)
                {
                    ControlStatus = Permissions.ControlStatus.Error;
                    ToolTipStatusText = value.InnerException.Message;
                }
                else if (!string.IsNullOrEmpty(value.WarningMessage))
                {
                    ControlStatus = Permissions.ControlStatus.Warning;
                    ToolTipStatusText = value.WarningMessage;
                }
                else
                {
                    ControlStatus = Permissions.ControlStatus.Ok;
                    ToolTipStatusText = "xx Works fine!";
                }
            }
        }

        /// <summary>
        /// Sets the tool tip.
        /// </summary>
        public string ToolTipDescriptionText
        {
            get { return toolTip.GetToolTip(label1); }
            set
            {
                toolTip.SetToolTip(label1, value);
            }
        }
        /// <summary>
        /// Sets the tool tip.
        /// </summary>
        public string ToolTipStatusText
        {
            get { return toolTip.GetToolTip(pictureBoxStatus); }
            set
            {
                toolTip.SetToolTip(pictureBoxStatus, value);
                toolTip.SetToolTip(this, value);
            }
        }

        /// <summary>
        /// Sets the label text.
        /// </summary>
        public string LabelText
        {
            get { return label1.Text; }
            set { label1.Text = value; }
        }

        /// <summary>
        /// Gets the connection point.
        /// </summary>
        /// <param name="side">The side.</param>
        /// <returns></returns>
        public Point GetConnectionPoint(ConnectionSide side)
        {
            switch (side)
            { 
                case ConnectionSide.Right:
                    return new Point(Left + Width, Top + ((int)(Height / 2))); 
                case ConnectionSide.Top:
                    return new Point(Left + ((int)(Width / 2)), Top); 
                case ConnectionSide.Left:
                    return new Point(Left, Top + ((int)(Height / 2))); 
                case ConnectionSide.Bottom:
                    return new Point(Left + ((int)(Width / 2)), Top + ((int)(Height / 2))); 
                default:
                    throw new ArgumentOutOfRangeException("side");
            }
        }

        #endregion

        #region Event handling

        void _resultScreen_Closing(object sender, CancelEventArgs e)
        {
            _resultScreen = null;
            _resultScreenPermanent = false;
        }

        /// <summary>
        /// Handles the MouseEnter event of the label1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void label1_MouseEnter(object sender, EventArgs e)
        {
            //loadResultScreen();
        }

        /// <summary>
        /// Handles the MouseLeave event of the label1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void label1_MouseLeave(object sender, EventArgs e)
        {
            if (_resultScreen != null && !_resultScreenPermanent)
            {
                _resultScreen.Close();
            }
        }

        /// <summary>
        /// Handles the Click event of the label1 control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void label1_Click(object sender, EventArgs e)
        {
            if (_resultScreen != null)
            {
                _resultScreenPermanent = true;
                _resultScreen.CloseButton.Enabled = true;
            }
        }

        #endregion

        #region Local methods

        private void loadResultScreen()
        {
            if (AuthorizationStep != null && _resultScreen == null)
            {
                _resultScreen = new AuthorizationStepResultListScreen(AuthorizationStep);
                Point localZeroPoint = new Point(0, 0);
                Point absolutZeroPoint = PointToScreen(localZeroPoint);
                Point bottomLeftPoint = new Point(absolutZeroPoint.X, absolutZeroPoint.Y + Height);
                _resultScreen.Location = bottomLeftPoint;
                _resultScreen.Closing += _resultScreen_Closing;
                _resultScreen.Show(this);
            }
        }

        #endregion

        private void toolTip_Popup(object sender, PopupEventArgs e)
        {
            if (e.AssociatedControl == label1)
            {
                loadResultScreen();
            }

        }

    }
}
