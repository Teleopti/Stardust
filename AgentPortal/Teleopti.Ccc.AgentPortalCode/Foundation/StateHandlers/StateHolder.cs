using System;
using System.Collections.Generic;
using Teleopti.Ccc.AgentPortalCode.Helper;
using Teleopti.Ccc.Sdk.Common.DataTransferObject;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Messaging.Exceptions;
using Teleopti.Messaging.SignalR;

namespace Teleopti.Ccc.AgentPortalCode.Foundation.StateHandlers
{

    /// <summary>
    /// A singleton holding an IState object
    /// </summary>
    /// <remarks>
    /// Created by: Dinesh Ranasinghe
    /// Created date: 2008-03-03
    /// </remarks>
    public class StateHolder : StateHolderReader
    {
        private string _connectionString;
        private static readonly object _locker = new object();
        private readonly IState _state;
        private IMessageBroker _messageBroker;

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>The instance.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-04
        /// </remarks>
        public new static StateHolder Instance
        {
            get { return (StateHolder)StateHolderReader.Instance; }
        }

        /// <summary>
        /// Gets the state reader.
        /// </summary>
        /// <value>The state reader.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-04
        /// </remarks>
        public override IStateReader StateReader
        {
            get { return _state; }
        }

        /// <summary>
        /// Gets the state.
        /// </summary>
        /// <value>The state.</value>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-04
        /// </remarks>
        public IState State
        {
            get { return _state; }
        }

        /// <summary>
        /// Gets the message broker.
        /// </summary>
        /// <value>The message broker.</value>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-22
        /// </remarks>
        public IMessageBroker MessageBroker
        {
            get { return _messageBroker; }

        }

        /// <summary>
        /// Initializes the specified client cache.
        /// </summary>
        /// <param name="clientCache">The client cache.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-04
        /// </remarks>
        public static void Initialize(IState clientCache)
        {
            if (clientCache != null)
            {
                if (!IsInitialized)
                {
                    //this lock is not tested
                    lock (_locker)
                    {
                        if (!IsInitialized)
                            InstanceInternal = new StateHolder(clientCache);
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StateHolder"/> class.
        /// </summary>
        /// <param name="state">The state.</param>
        /// <remarks>
        /// Created by: Dinesh Ranasinghe
        /// Created date: 2008-03-04
        /// </remarks>
        private StateHolder(IState state)
        {
            _state = state;
            try
            {
                InitializeMessageBroker();
            }
            catch (BrokerNotInstantiatedException)
            {
                return;
            }
        }

        /// <summary>
        /// Initializes the message broker.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-22
        /// </remarks>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
		private void InitializeMessageBroker()
        {
            SetConfigurationInfo();
        	Uri serverUrl;
        	if (Uri.TryCreate(_connectionString,UriKind.Absolute,out serverUrl))
        	{
				var broker = new SignalBroker(new DummyFilterManager()) {ConnectionString = _connectionString};
				_messageBroker = broker;
				_messageBroker.StartMessageBroker();
        	}
        }

        private class DummyFilterManager : IMessageFilterManager
        {
            public DummyFilterManager()
            {
                FilterDictionary = new Dictionary<Type, IList<Type>>();
            }

            public IDictionary<Type, IList<Type>> FilterDictionary { get; private set; }
            public string LookupType(Type domainObjectType)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Sets the configuration info.
        /// </summary>
        /// <remarks>
        /// Created by: Sumedah
        /// Created date: 2008-08-25
        /// </remarks>
        private void SetConfigurationInfo()
        {
            _connectionString = string.Empty;

            MessageBrokerDto messageBrokerDto = SdkServiceHelper.LogOnServiceClient.GetMessageBrokerConfiguration();
            if (messageBrokerDto!=null)
            {
                _connectionString = messageBrokerDto.ConnectionString;
            }
        }
    }
}