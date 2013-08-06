using System;

namespace Teleopti.Ccc.AgentPortalCode.Helper
{
    public class ReplyMessageEventArgs : EventArgs
    {
        public ReplyMessageEventArgs(string textMessage, string optionChecked, MessagePresenterObject messagePresenterObject)
        {
            TextMessage = textMessage;
            OptionChecked = optionChecked;
            PresenterObject = messagePresenterObject;
        }

        public string TextMessage { get; private set; }

        public string OptionChecked { get; private set; }
        public MessagePresenterObject PresenterObject { get; private set; }
    }
}