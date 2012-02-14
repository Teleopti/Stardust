using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortal.PushMessagePopup;
using Teleopti.Ccc.AgentPortalCode.Helper;

namespace Teleopti.Ccc.AgentPortal.Common.Controls
{
    public partial class MailButton : UserControl
    {
        private int _noOfMails = 3;
        private PushMessageController _controller;
        private bool _controllerIsInitialized;
        private MessageForm _messageForm;

        public MailButton()
        {
            InitializeComponent();
            Enabled = false;
        }

        public int NoOfMails
        {
            get { return _noOfMails; }
            private set
            {
                 _noOfMails = value;
                toolTip1.SetToolTip(buttonAdv1 ,value.ToString(CultureInfo.CurrentCulture ));     
                Enabled = (_noOfMails > 0);
                Invalidate();
                Refresh();
            }
        }


        public void CloseMessageForm()
        {
            if (_messageForm != null)
            {
                _messageForm.Close();
            }
        }

        private void buttonAdv1_Click(object sender, EventArgs e)
        {
            CloseMessageForm();
            _messageForm = new MessageForm(_controller);
            _messageForm.Show();   
        }

        private void buttonAdv1_Paint(object sender, PaintEventArgs e)
        {
            if (NoOfMails < 1)
                return;
             Font font = new Font("Arial", 9, FontStyle.Bold);
             e.Graphics.SmoothingMode = SmoothingMode.HighQuality; 

       
            Rectangle rect = new Rectangle(buttonAdv1.Width - 22, 1, 20, 20);

            Rectangle dropshadow = rect;
            dropshadow.Offset(1,4);
            using (Brush brush = new LinearGradientBrush(new Rectangle(1, 1, 20, 20), Color.Black, Color.DarkGray, 90))
            {
                e.Graphics.FillEllipse(brush, dropshadow);
            }

            using (Brush brush = new LinearGradientBrush(new Rectangle(1, 1, 20, 20), Color.WhiteSmoke, Color.Red, 90))
            {
                e.Graphics.FillEllipse(brush, rect);
            }

            Rectangle backborder = rect;
            backborder.Inflate(-1, -1);

            using (Pen pen = new Pen(Color.WhiteSmoke, 2f))
            {
                e.Graphics.DrawEllipse(pen, backborder);
            }

            StringFormat format = new StringFormat();
            format.LineAlignment = StringAlignment.Center;
            format.Alignment = StringAlignment.Center;
            string mailNumber;
            if (NoOfMails > 9) mailNumber = 9.ToString(CultureInfo.CurrentCulture) + "+";
            else mailNumber = NoOfMails.ToString(CultureInfo.CurrentCulture);
            e.Graphics.DrawString(mailNumber, font, Brushes.White, rect, format);
        }

        #region MessageEvents
        private void InitializeListenerForMessageEvents(object sender, EventArgs e)
        {
            if (_controller != null && !_controllerIsInitialized)
            {
                 NoOfMails = _controller.GetMessagePresenterObjects().Count;
                _controller.NumberOfUnreadMessagesChanged += HelperNumberOfUnreadMessagesChanged;
                _controllerIsInitialized = true;
            }
        }
        
        public void SubscribeToMessageEvent(PushMessageController controller)
        {
            _controller = controller;
        }

        delegate void Func();
        private void HelperNumberOfUnreadMessagesChanged(object sender, PushMessageHelperEventArgs e)
        {
           
                Func del = delegate
                {
                    NoOfMails = e.NoOfMails;
                };
                Invoke(del);
            
        }
        #endregion
    }
}
