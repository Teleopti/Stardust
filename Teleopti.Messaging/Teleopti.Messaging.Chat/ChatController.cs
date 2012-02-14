using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Client;

namespace Teleopti.Messaging.Chat
{
    public class ChatController
    {
        private readonly ChatForm _view;
        private readonly ChatModel _model;
        private readonly IMessageBroker _messageBroker;

        public ChatController(ChatModel model, ChatForm view)
        {
            _model = model;
            _view = view;
            _view.ButtonSend.Click += buttonSend_Click;
            _view.TextBoxChat.KeyDown += OnKeyDown;
            IDictionary<Type, IList<Type>> typeFilter = new Dictionary<Type, IList<Type>>();
            typeFilter.Add(typeof(IChat),new List<Type> {typeof(IChat)});
            typeFilter.Add(typeof(IEventHeartbeat),new List<Type> {typeof(IEventHeartbeat)});
            _messageBroker = MessageBrokerImplementation.GetInstance(typeFilter);
            _messageBroker.ConnectionString = ConfigurationManager.AppSettings["MessageBroker"];
            _messageBroker.ExceptionHandler += OnExceptionHandler;
            _messageBroker.StartMessageBroker();
            _messageBroker.RegisterEventSubscription(OnEventMessage, typeof(IChat));
            _messageBroker.RegisterEventSubscription(OnEventMessage, typeof(IEventHeartbeat));

        }

        private static void OnExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            Debug.WriteLine(((Exception) e.ExceptionObject).Message + ((Exception) e.ExceptionObject).StackTrace);            
        }

        private void OnEventMessage(object sender, EventMessageArgs e)
        {
            if(e.Message.DomainObjectType == typeof(IChat).AssemblyQualifiedName)
                UpdateMainWindow(e.Message);            
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Return)
            {
                buttonSend_Click(sender, e);
                e.SuppressKeyPress = true;
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            string message = _view.TextBoxChat.Text;
            _model.SendMessage(_messageBroker, message);
            _view.TextBoxChat.Clear();
        }

        private delegate void UpdateMainWindowHandler(IEventMessage message);
        public void UpdateMainWindow(IEventMessage message)
        {
            if(_view.TextBoxMainWindow.InvokeRequired)
            {
                UpdateMainWindowHandler handler = UpdateMainWindow;
                _view.TextBoxMainWindow.Invoke(handler, new object[] { message });
            }
            else
            {
                string stringMessage = Encoding.ASCII.GetString(message.DomainObject);
                stringMessage = stringMessage.Replace("\0", String.Empty);
                _view.TextBoxMainWindow.Text = String.Format("{0} {1}{2}{3}{4}{5}", message.ChangedDateTime.ToShortTimeString(), message.ChangedBy, "> ", stringMessage, Environment.NewLine, _view.TextBoxMainWindow.Text);
            }
        }

    }

}
