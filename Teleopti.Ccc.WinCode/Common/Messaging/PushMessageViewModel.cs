using System.ComponentModel;
using Teleopti.Ccc.Domain.Common.Messaging;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Common.Messaging
{
    public class PushMessageViewModel:INotifyPropertyChanged
    {
        private IPushMessage _model;
       
        public IPushMessage PushMessage
        {
            get { return _model; }
            private set
            {
                if(_model!=value)
                {
                    _model = value;
                    SendPropertyChanged("PushMessage");
                }
            }
        }

        public string Title
        {
            set
            {
                if(_model.GetTitle(new NoFormatting()) != value)
                {
                    _model.Title = value;
                   
                    SendPropertyChanged("Title");
                }
            }
        }

		public string GetTitle(ITextFormatter formatter)
		{
			return _model.GetTitle(formatter);
		}

        public string Message
        {
            set
            {
                if (_model.GetMessage(new NoFormatting()) != value)
                {
                    _model.Message = value;
                    SendPropertyChanged("Message");
                }
            }
        }

		public string GetMessage(ITextFormatter formatter)
		{
			return _model.GetMessage(formatter);
		}

        public bool CanSend
        {
            get
            {
                //designed by fx cop
                return !(string.IsNullOrEmpty(_model.GetTitle(new NoFormatting())) || string.IsNullOrEmpty(_model.GetMessage(new NoFormatting())));
            }
           
        }

        public PushMessageViewModel(IPushMessage pushMessage)
        {
            _model = pushMessage;
            Title = pushMessage.GetTitle(new NoFormatting());
            Message = pushMessage.GetMessage(new NoFormatting());
        }

        private void SendPropertyChanged(string property)
        {
        	var handler = PropertyChanged;
            if (handler!=null)
            {
            	handler(this,new PropertyChangedEventArgs(property));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        //Create a new conversation when the old one is sent:
        public void CreateNewConversation()
        {
            PushMessage = new PushMessage();
            SendPropertyChanged("Message"); 
            SendPropertyChanged("Title");
        }
    }
}