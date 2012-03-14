using System.ComponentModel;
using Teleopti.Ccc.Domain.Common.Messaging;
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
            get { return _model.Title; }
            set
            {
                if(_model.Title != value)
                {
                    _model.Title = value;
                   
                    SendPropertyChanged("Title");
                }
            }
        }
        public string Message
        {
            get { return _model.Message; }
            set
            {
                if (_model.Message != value)
                {
                    _model.Message = value;
                    SendPropertyChanged("Message");
                }
            }
        }

        public bool CanSend
        {
            get
            {
                //designed by fx cop
                return !(string.IsNullOrEmpty(_model.Title) || string.IsNullOrEmpty(_model.Message));
            }
           
        }

        public PushMessageViewModel(IPushMessage pushMessage)
        {
            _model = pushMessage;
            Title = pushMessage.Title;
            Message = pushMessage.Message;
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