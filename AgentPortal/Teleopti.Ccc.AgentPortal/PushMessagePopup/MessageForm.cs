using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using Syncfusion.Windows.Forms;
using Teleopti.Ccc.AgentPortal.Helper;
using Teleopti.Ccc.AgentPortalCode.Common;
using Teleopti.Ccc.AgentPortalCode.Helper;


namespace Teleopti.Ccc.AgentPortal.PushMessagePopup
{
	public partial class MessageForm : Office2007Form, ILocalized
    {
        private MessagePresenterObject _currentMessage;
        private Collection<MessagePresenterObject> _presenterObjects;
        private readonly PushMessageController _pushMessageController;

        public MessageForm()
        {
            InitializeComponent();
            SetColors();
        }

        public MessageForm(PushMessageController pushMessageController)
        {
            InitializeComponent();
            _pushMessageController = pushMessageController;
            _presenterObjects = _pushMessageController.GetMessagePresenterObjects();
            _pushMessageController.NumberOfUnreadMessagesChanged += PushMessageControllerNumberOfUnreadMessagesChanged;
            if (_presenterObjects.Count > 0) SetMessage(_presenterObjects[0]);
            SetColors();
			SetTexts();
            messageControl1.MessageReplyClicked += MessageControl1MessageReplyClicked;
            messageControl1.CloseButtonClicked += messageControl1_CloseButtonClicked;
        }


        delegate void Func();
        private void PushMessageControllerNumberOfUnreadMessagesChanged(object sender, PushMessageHelperEventArgs e)
        {
            if (e.OriginalSource!=this)
            {
                if (InvokeRequired)
                {
                    Func del = RefreshPresenterObjects;
                    Invoke(del);
                }
                else RefreshPresenterObjects();
            }

        }
        
        private void RefreshPresenterObjects()
        {
                var currentDtoId = _currentMessage.Dto.Id;
                MessagePresenterObject selected = null;

                _presenterObjects = _pushMessageController.GetMessagePresenterObjects();

                foreach (MessagePresenterObject presenterObject in _presenterObjects)
                {
                    if (presenterObject.Dto.Id.Equals(currentDtoId))
                    {
                        selected = presenterObject;
                    }
                }

                if (selected == null)
                {
                    MessageBoxHelper.ShowWarningMessage(UserTexts.Resources.SomeoneChangedTheSameDataBeforeYouDot,UserTexts.Resources.WarningMessageTitle);
                    if (_presenterObjects.Count > 0) selected = _presenterObjects[0];
                    else Close(); //No messages left....
                }
                SetMessage(selected);
        }

        void messageControl1_CloseButtonClicked(object sender, EventArgs e)
        {
            Close();
        }

        private void SetColors()
        {
            BackColor = Color.WhiteSmoke;
        }

        private void SetMessage(MessagePresenterObject messagePresenterObject)
        {
            if (messagePresenterObject != null)
            {
                _currentMessage = messagePresenterObject;
                HandleGuiParts(messagePresenterObject);
                messageControl1.Enabled = !messagePresenterObject.Dto.IsReplied;
                messageControl1.SetOptionItems(messagePresenterObject.ReplyOptions);
                messageControl1.SetTextBody(messagePresenterObject.OriginalMessage + messagePresenterObject.DialogMessages);
                messageControl1.SetTitle(messagePresenterObject.Title);
                messageControl1.SetId(messagePresenterObject);
                labelSender.Text = messagePresenterObject.Sender;
                labelDate.Text = messagePresenterObject.OriginalDate;
                HandleArrows();
            }
        }

        private void HandleGuiParts(MessagePresenterObject o)
        {

            messageControl1.ShowOptions = o.ReplyOptions.Count > 1;
            messageControl1.ShowTextBox = o.AllowDialogueReply;
            Buttontext();
        }


        private void HandleArrows()
        {
            int i = _presenterObjects.IndexOf(_currentMessage);
            if (i == _presenterObjects.Count - 1) pictureBoxRight.Visible = false;
            if (i < _presenterObjects.Count - 1) pictureBoxRight.Visible = true;
            if (i == 0) pictureBoxLeft.Visible = false;
            if (i > 0) pictureBoxLeft.Visible = true;
            if (CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft)
            {                                                        
                pictureBoxLeft.Image = Properties.Resources.ccc_Right;
                pictureBoxRight.Image = Properties.Resources.ccc_Left;
            }
        }


        private void MessageControl1MessageReplyClicked(object sender, ReplyMessageEventArgs e)
        {
           _pushMessageController.ReplyAndUpdate(e,this);
           _presenterObjects = _pushMessageController.GetMessagePresenterObjects();
           if (_presenterObjects.Count > 0)
               SetMessage(_presenterObjects[0]);
           else Close();
        }

        private void Buttontext()
        {
            if (_currentMessage.ReplyOptions.Count == 1)
            {
                List<string> options = new List<string>(_currentMessage.ReplyOptions);
                messageControl1.SetButtonText(options[0]);
            }
            else
            {
                messageControl1.SetButtonText(UserTexts.Resources.Ok);
            }
        }

        private void pictureBoxRight_Click(object sender, EventArgs e)
        {
            int i = _presenterObjects.IndexOf(_currentMessage);
            if (i < _presenterObjects.Count - 1)
            {
                SetMessage(_presenterObjects[i + 1]);
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            int i = _presenterObjects.IndexOf(_currentMessage);
            if (i > 0)
            {
                SetMessage(_presenterObjects[i - 1]);
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            _pushMessageController.NumberOfUnreadMessagesChanged -= PushMessageControllerNumberOfUnreadMessagesChanged;
            base.OnClosed(e);
        }

		public void SetTexts()
		{
			new LanguageResourceHelper().SetTexts(this);
		}
    }
}