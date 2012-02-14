using System;
using Teleopti.Ccc.Domain.Common.EntityBaseTypes;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Common.Messaging
{
    public class DialogueMessage : AggregateEntity, IDialogueMessage
    {
        private readonly string _text;
        private readonly DateTime _created;
        private readonly IPerson _sender;

        public virtual IPerson Sender
        {
            get { return _sender; }
        }
        public virtual string Text
        {
            get { return _text; }
        }
        public virtual DateTime Created
        {
            get { return _created; }
        }

        public DialogueMessage(string text,IPerson sender) : this()
        {
            _text = text;
            _sender = sender;
            _created = DateTime.UtcNow;
        }

        protected DialogueMessage()
        {
            
        }
    }
}
