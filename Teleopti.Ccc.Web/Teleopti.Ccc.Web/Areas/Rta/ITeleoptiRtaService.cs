﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;

// ReSharper disable CheckNamespace
// 2015-03-16 -> DO NOT CHANGE! Changing namespace will break backwards compatibility /Team RealTime
namespace Teleopti.Ccc.Rta.WebService
// ReSharper restore CheckNamespace
{
	[ServiceContract]
	public interface ITeleoptiRtaService
	{
		[OperationContract]
		int SaveExternalUserState(string authenticationKey, string userCode, string stateCode, string stateDescription, bool isLoggedOn, int secondsInState, DateTime timestamp,
										string platformTypeId, string sourceId, DateTime batchId, bool isSnapshot);

		[OperationContract]
		int SaveBatchExternalUserState(string authenticationKey, string platformTypeId, string sourceId, ICollection<ExternalUserState> externalUserStateBatch);

		[OperationContract]
		void GetUpdatedScheduleChange(Guid personId, Guid businessUnitId, DateTime timestamp, string tenant);
	}

	[DataContract, Serializable]
	public class ExternalUserState : IExtensibleDataObject
	{
		[NonSerialized]
		private ExtensionDataObject _extensionData;

		[DataMember]
		public string UserCode { get; set; }

		[DataMember]
		public string StateCode { get; set; }

		[DataMember]
		public string StateDescription { get; set; }

		[DataMember]
		public bool IsLoggedOn { get; set; }

		[DataMember]
		public int SecondsInState { get; set; }

		[DataMember]
		public DateTime Timestamp { get; set; }

		[DataMember]
		public DateTime BatchId { get; set; }

		[DataMember]
		public bool IsSnapshot { get; set; }

		public ExtensionDataObject ExtensionData
		{
			get { return _extensionData; }
			set { _extensionData = value; }
		}
	}
}
