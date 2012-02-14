using System;
using Teleopti.Ccc.Sdk.ServiceBus.Messages;

namespace Teleopti.Ccc.ServiceBus.ErrorViewer.ViewModel
{
    public class CustomErrorViewModel
    {
        public CustomErrorMessage Message { get; private set; }

        public CustomErrorViewModel(CustomErrorMessage message)
        {
            Message = message;
        }

        public Uri Source { get { return Message.Source; } }

        public Uri Destination { get { return Message.Destination; } }

        public string MessageId { get { return Message.MessageId; } }

        public string MessageData { get { return Message.Message.ToString(); } }

        public object ObjectData { get { return Message.Message; } }

        //public string ExceptionText { get { return Message.Exception==null?string.Empty: Message.Exception.Message; } }

        //public string InnerExceptionText { get { return Message.Exception == null || Message.Exception.InnerException == null ? string.Empty : Message.Exception.InnerException.Message; } }
    }
}