using System;
using System.Runtime.Serialization;

namespace Teleopti.Ccc.Sdk.Common.DataTransferObject
{
    [Serializable]
    [DataContract(Namespace = "http://schemas.ccc.teleopti.com/sdk/2010/04/")]
    public class AgentQueueStatDetailsDto : IExtensibleDataObject
    {
        private string _queueName;
        private int _answeredContacts;
        private long _averageTalkTime;
        private long _afterContactWorkTime;
		private long _averageHandlingTime;

		[NonSerialized]
		private ExtensionDataObject _extensionData;

        [DataMember]
        public string QueueName
        {
            get { return _queueName; }
            set { _queueName = value; }
        }
        [DataMember]
        public int AnsweredContacts
        {
            get { return _answeredContacts; }
            set { _answeredContacts = value; }
        }
        [DataMember]
        public long AverageTalkTime
        {
            get { return _averageTalkTime; }
            set { _averageTalkTime = value; }
        }
        [DataMember]
        public long AfterContactWorkTime
        {
            get { return _afterContactWorkTime; }
            set { _afterContactWorkTime = value; }
        }
        [DataMember]
        public long AverageHandlingTime
        {
            get { return _averageHandlingTime; }
            set { _averageHandlingTime = value; }
        }

    	public ExtensionDataObject ExtensionData
    	{
    		get { return _extensionData; }
    		set { _extensionData = value; }
    	}
    }
}
