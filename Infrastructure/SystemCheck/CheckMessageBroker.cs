﻿using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Events;

namespace Teleopti.Ccc.Infrastructure.SystemCheck
{
    public class CheckMessageBroker : ISystemCheck
    {
        private readonly IMessageBroker _messageBroker;

        public CheckMessageBroker(IMessageBroker messageBroker)
        {
            _messageBroker = messageBroker;
        }

        public bool IsRunningOk()
        {
            return string.IsNullOrEmpty(_messageBroker.ConnectionString) ||
                   _messageBroker.IsInitialized;
        }

        public string WarningText
        {
            get { return UserTexts.Resources.CheckMessageBrokerWarning; }
        }
    }
}
