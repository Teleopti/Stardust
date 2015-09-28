using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Remoting;
using System.Security;
using log4net;

namespace Teleopti.Ccc.Rta.TestApplication
{
    public class ClientHandler
    {
        private readonly IRtaDataHandlerClient _dataHandler;
        private readonly ILog _loggingSvc;
        private readonly string _serviceUrl;
        private readonly int _timeout;
        public const string LogName = "RtaClientLog";

        protected ClientHandler(ILog loggingSvc, IRtaDataHandlerClient dataHandler, IDictionary clientSettings)
        {
			if (dataHandler==null) throw new ArgumentNullException("dataHandler");

            _loggingSvc = loggingSvc;
            _dataHandler = dataHandler;
            _loggingSvc.Info("Getting settings for client");

            if (clientSettings == null)
            {
                clientSettings = new Hashtable();
            }

            if (!clientSettings.Contains("serviceUrl"))
            {
                _serviceUrl = "http://localhost/TeleoptiRtaService.svc";
                _loggingSvc.Warn(string.Concat("Setting for service url was not provided, default value ", _serviceUrl, " used."));
            }
            else
            {
                _serviceUrl = clientSettings["serviceUrl"].ToString();
            }

            if (!clientSettings.Contains("connectionTimeout"))
            {
                clientSettings.Add("connectionTimeout", _dataHandler.Timeout);
                _loggingSvc.Warn(string.Concat("Setting for connection timeout was not provided, default value ", _dataHandler.Timeout, " used."));
            }
            _timeout = (int) clientSettings["connectionTimeout"];

            if (!clientSettings.Contains("name"))
                clientSettings.Add("name",string.Empty);

            clientSettings["name"] = string.Format(CultureInfo.InvariantCulture, "RTAClientChannel {0}", Guid.NewGuid());
            _loggingSvc.InfoFormat("Server settings initialised. Address = {0}", _serviceUrl);
        }

        public ClientHandler(IDictionary clientSettings)
            : this(LogManager.GetLogger(LogName), new TeleoptiRtaServiceProxy(), clientSettings)
        {
        }

        public bool IsStarted
        {
            get { return _dataHandler!=null && _dataHandler.IsAlive; }
        }

        public void StartLogClient()
        {
            try
            {
                _dataHandler.Url = _serviceUrl;
                _dataHandler.Timeout = _timeout;
            }
            catch (RemotingException remotingException)
            {
                _loggingSvc.Error("Could create handle to service on server.", remotingException);
                StopLogClient();
                throw;
            }
            catch (SecurityException securityException)
            {
                _loggingSvc.Error("Could create handle to service on server due to security issues.", securityException);
                StopLogClient();
                throw;
            }

            _loggingSvc.InfoFormat("Client created handle to server at address {0}",_serviceUrl);
        }

        public void SendRtaDataToServer(string authenticationKey, string logOn, string stateCode, TimeSpan timeInState, DateTime timestamp, Guid platformTypeId, int logObjectId, DateTime batchId, bool isSnapshot)
        {
            _loggingSvc.InfoFormat("Sending message to server: LogOn {0}, StateCode {1}, Timestamp {2}", logOn, stateCode, timestamp);
			try
			{
				_dataHandler.ProcessRtaData(authenticationKey, logOn, stateCode, timeInState, timestamp, platformTypeId,
				                            logObjectId.ToString(CultureInfo.InvariantCulture), batchId,
				                            isSnapshot);
			}
			catch (InvalidOperationException exception)
			{
				const string errorMessage = "The data handler service has been disposed. Restart the client before trying to send new messages.";
				_loggingSvc.Error(errorMessage,exception);

				throw;
			}
            
            _loggingSvc.Info("Done sending message to server");
        }

		public void UpdateScheduleChange(string personId, string businessId, DateTime timestamp)
		{
			_loggingSvc.InfoFormat("Sending UpdateScheduleChange message to server: PersonId {0}, BU {1}, Timestamp {2}", personId, businessId, timestamp);
			try
			{
				_dataHandler.ProcessScheduleUpdate(Guid.Parse(personId), Guid.Parse(businessId), timestamp);
			}
			catch (Exception e)
			{
				_loggingSvc.Error(e);
				throw;
			}
		}

		public void SendRtaDataToServer(Guid platformTypeId, int logObjectId, ICollection<ITeleoptiRtaState> rtaStates)
		{
			try
			{
				_dataHandler.ProcessRtaData(platformTypeId, logObjectId.ToString(CultureInfo.InvariantCulture), rtaStates);
			}
			catch (InvalidOperationException exception)
			{
				const string errorMessage = "The data handler service has been disposed. Restart the client before trying to send new messages.";
				_loggingSvc.Error(errorMessage, exception);

				throw;
			}

			_loggingSvc.Info("Done sending message to server");
		}

        public void StopLogClient()
        {
            _loggingSvc.Info("Client channel unregistered");
        }
    }
}