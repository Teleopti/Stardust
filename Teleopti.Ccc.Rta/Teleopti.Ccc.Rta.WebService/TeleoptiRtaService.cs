﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.ServiceModel;
using Teleopti.Interfaces.Domain;
using log4net;
using log4net.Config;
using Teleopti.Ccc.Rta.Interfaces;
using Teleopti.Ccc.Rta.Server;

namespace Teleopti.Ccc.Rta.WebService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class TeleoptiRtaService : ITeleoptiRtaService, IDisposable
    {
        private IRtaDataHandler _rtaDataHandler;
        private readonly string _authenticationKey;
        private readonly object _lockObject = new object();
        private const string LogOutStateCode = "LOGGED-OFF";
        private static readonly ILog Log = LogManager.GetLogger(typeof (TeleoptiRtaService));

        public TeleoptiRtaService()
        {
            XmlConfigurator.Configure();
            

            Log.Info("The real time adherence service is now started");
            string authenticationKey = ConfigurationManager.AppSettings["AuthenticationKey"];
            if (string.IsNullOrEmpty(authenticationKey)) authenticationKey = "!#¤atAbgT%";
            _authenticationKey = authenticationKey;
        }

        private void InitializeClientHandler()
        {
            _rtaDataHandler = RtaFactory.DataHandler;
        }

        public int SaveExternalUserState(string authenticationKey, string userCode, string stateCode, string stateDescription, bool isLoggedOn, int secondsInState, DateTime timestamp, string platformTypeId, string sourceId, DateTime batchId, bool isSnapshot)
        {
            Guid messageId = Guid.NewGuid();

			verifyAuthenticationKey(authenticationKey, messageId);

            return processExternalUserState(messageId, userCode, stateCode, stateDescription, isLoggedOn, secondsInState, timestamp, platformTypeId, sourceId, batchId, isSnapshot);
        }

    	private int processExternalUserState(Guid messageId, string userCode, string stateCode, string stateDescription, bool isLoggedOn, int secondsInState, DateTime timestamp, string platformTypeId, string sourceId, DateTime batchId, bool isSnapshot)
    	{
			if (Log.IsInfoEnabled)
			{
				Log.InfoFormat(System.Globalization.CultureInfo.InvariantCulture,
				               "Incoming message: MessageId = {10}, UserCode = {0}, StateCode = {1}, StateDescription = {2}, IsLoggedOn = {3}, SecondsInState = {4}, TimeStamp = {5}, PlatformTypeId = {6}, SourceId = {7}, BatchId = {8}, IsSnapshot = {9}.",
				               userCode, stateCode, stateDescription, isLoggedOn, secondsInState, timestamp,
				               platformTypeId, sourceId, batchId, isSnapshot, messageId);
			}

    		if (string.IsNullOrEmpty(sourceId))
    		{
    			Log.ErrorFormat("The source id was not valid. Supplied value was {0}. (MessageId = {1})", sourceId, messageId);
    			return -300;
    		}

    		if (string.IsNullOrEmpty(platformTypeId))
    		{
    			Log.ErrorFormat("The platform type id cannot be empty or null. (MessageId = {0})", messageId);
    			return -200;
    		}

    		Guid parsedPlatformTypeId = new Guid(platformTypeId);

    		if (!isLoggedOn)
    		{
    			//If the user isn't logged on we'll substitute the stateCode to reflect this
    			Log.InfoFormat(
    				"This is a log out state. The original state code {0} is substituted with hardcoded state code {1}. (MessageId = {2})",
    				stateCode, LogOutStateCode, messageId);
    			stateCode = LogOutStateCode;
    			stateDescription = stateCode;
    		}

    		//The DateTimeKind.Utc is not set automatically when deserialising from soap message
    		timestamp = DateTime.SpecifyKind(timestamp, DateTimeKind.Utc);
    		if (timestamp>DateTime.UtcNow.AddMinutes(59))
    		{
    			Log.ErrorFormat(
    				"The supplied time stamp cannot be sent as UTC. Current UTC time is {0} and the supplied timestamp was {1}. (MessageId = {2})",
    				DateTime.UtcNow, timestamp, messageId);
    			return -430;
    		}

    		const int stateCodeMaxLength = 25;
    		stateCode = stateCode.Trim();
    		if (stateCode.Length > stateCodeMaxLength)
    		{
    			var newStateCode = stateCode.Substring(0, stateCodeMaxLength);
    			Log.WarnFormat("The original state code {0} is too long and substituted with state code {1}. (MessageId = {2})",
    			               stateCode, newStateCode, messageId);
    			stateCode = newStateCode;
    		}

			if (Log.IsInfoEnabled)
			{
				Log.InfoFormat("Message verified and validated from sender for userCode: {0}, stateCode: {1}. (MessageId = {2})", userCode, stateCode, messageId);
			}
    		lock (_lockObject)
    		{
    			if (_rtaDataHandler == null || !_rtaDataHandler.IsAlive) InitializeClientHandler();
    			if (_rtaDataHandler != null)
    			{
    				_rtaDataHandler.ProcessRtaData(userCode.Trim(), stateCode, TimeSpan.FromSeconds(secondsInState), timestamp,
    				                               parsedPlatformTypeId, sourceId, batchId, isSnapshot);
    			}
    		}
			if (Log.IsInfoEnabled)
			{
				Log.InfoFormat("Message handled from sender for userCode: {0}, stateCode: {1}. (MessageId = {2})", userCode, stateCode, messageId);
			}

    		return 1;
    	}

    	private void verifyAuthenticationKey(string authenticationKey, Guid messageId)
    	{
    		if (authenticationKey != _authenticationKey)
    		{
    			Log.ErrorFormat("An invalid authentication key was supplied. AuthenticationKey = {0}. (MessageId = {1})",authenticationKey,messageId);
    			throw new FaultException(
    				"You supplied an invalid authentication key. Please verify the key and try again.");
    		}
    	}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
		public int SaveBatchExternalUserState(string authenticationKey, string platformTypeId, string sourceId, ICollection<ExternalUserState> externalUserStateBatch)
    	{
			Guid messageId = Guid.NewGuid();

			verifyAuthenticationKey(authenticationKey, messageId);

    		verifyBatchNotTooLarge(externalUserStateBatch);

    		int result = 0;

			foreach (var externalUserState in externalUserStateBatch)
			{
				var processResult = processExternalUserState(messageId, externalUserState.UserCode, externalUserState.StateCode,
				                                             externalUserState.StateDescription, externalUserState.IsLoggedOn,
				                                             externalUserState.SecondsInState, externalUserState.Timestamp,
				                                             platformTypeId, sourceId, externalUserState.BatchId,
				                                             externalUserState.IsSnapshot);
				if (processResult < result || result == 0)
				{
					result = processResult;
				}
			}

    		return result;
    	}

        public void GetUpdatedScheduleChange(Guid personId, Guid bussinessUnitId, string dataSource, DateTime activityTimeStamp)
        {
            lock (_lockObject)
            {
                if (_rtaDataHandler == null || !_rtaDataHandler.IsAlive) InitializeClientHandler();
                if (_rtaDataHandler != null)
                {
                    _rtaDataHandler.CheckSchedule(personId);
                }
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "log4net.ILog.ErrorFormat(System.String,System.Object[])")]
		private static void verifyBatchNotTooLarge(ICollection<ExternalUserState> externalUserStateBatch)
    	{
    		if (externalUserStateBatch.Count>50)
    		{
				Log.ErrorFormat("The incoming batch contains more than 50 external user states. Reduce the number if states per batch to a number below 50.");
				throw new FaultException(
					"Incoming batch too large. Please lower the number of user states in a batch to below 50.");
    		}
    	}

    	public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_rtaDataHandler != null &&
                _rtaDataHandler.IsAlive)
                {
                    _rtaDataHandler = null;
                }
            }
        }
    }
}
