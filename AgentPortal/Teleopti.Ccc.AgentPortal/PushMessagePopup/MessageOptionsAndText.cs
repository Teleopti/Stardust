using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Teleopti.Ccc.AgentPortalCode.Helper;

namespace Teleopti.Ccc.AgentPortal.PushMessagePopup
{
    public partial class MessageOptionsAndText : UserControl
    {
        private bool _showOptions;
        private bool _showTextBox;

        public MessageOptionsAndText()
        {
            InitializeComponent();
            optionList1.RadioButtonIsChecked += optionList1_RadioButtonIsChecked;
            textBox1.TextChanged += textBox1_TextChanged;
            buttonAdvClose.Text = UserTexts.Resources.Close;
        }


        public bool ShowOptions
        {
            get { return _showOptions; }
            set
            {
                optionList1.Visible = value;
                _showOptions = value;
                ButtonCheck();
            }
        }

        public bool ShowTextBox
        {
            get { return _showTextBox; }
            set
            {
                _showTextBox = value;
                textBox1.Visible = value;
                ButtonCheck();
            }
        }

        public void SetButtonText(string text)
        {
            buttonAdvSend.Text = text;
        }

        private void ButtonCheck()
        {
            if (ShowOptions && ShowTextBox) buttonAdvSend.Enabled = false;
            else buttonAdvSend.Enabled = true;
        }

        public void SetTitle(string title)
        {
            labelHeader.Text = title;
        }

        public void SetTextBody(string textBody)
        {
            label1.Text = textBody;
        }

        public void SetOptionItems(ICollection<string> options)
        {
            optionList1.SetOptionItems(options);
        }

        private void optionList1_RadioButtonIsChecked(object sender, EventArgs e)
        {
            buttonAdvSend.Enabled = true;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            buttonAdvSend.Enabled = true;
        }

        public event EventHandler<ReplyMessageEventArgs> MessageReplyClicked;


        private void buttonAdv1_Click(object sender, EventArgs e)
        {
            if (MessageReplyClicked != null)
            {
                string replyText = textBox1.Text;
                string selectedOption = optionList1.Selected;
                var currentPresenterObject = (MessagePresenterObject) buttonAdvSend.Tag;

                MessageReplyClicked.Invoke(this, new ReplyMessageEventArgs(replyText, selectedOption, currentPresenterObject));
            }
        }

        public void SetId(MessagePresenterObject messagePresenterObject)
        {
            textBox1.Text = messagePresenterObject.LatestReply;
            buttonAdvSend.Tag = messagePresenterObject;
        }

        public event EventHandler CloseButtonClicked;
        private void buttonAdvClose_Click(object sender, EventArgs e)
        {
            if(CloseButtonClicked != null)
            {
                CloseButtonClicked.Invoke(this,e);
            }
        }
    }
}