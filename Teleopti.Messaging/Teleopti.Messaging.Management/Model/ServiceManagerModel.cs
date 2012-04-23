using System;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Management;
using System.Runtime.Remoting.Messaging;
using System.Text;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.MessageBroker.Core;
using Teleopti.Interfaces.MessageBroker.Events;
using Teleopti.Logging;
using Teleopti.Messaging.Client;
using Teleopti.Messaging.Management.Controllers;

namespace Teleopti.Messaging.Management.Model
{
    public class ServiceManagerModel
    {
        private ServiceManagerController _controller;
        private IMessageBroker _messageBroker;
        private delegate string StringResponseAsyncHandler(string name);


        public void InstallService(string path, string name, string displayName, string install)
        {
            // Do not delete, works flawlessly with Windows XP but not with UAC and Windows VISTA
            //_controller.ConnectionString = ConfigurationManager.AppSettings["MessageBroker"];
            //// Start an executable that will install the service.
            //Process installer = new Process();
            //installer.StartInfo.FileName = String.Format(CultureInfo.InvariantCulture,"{0}\\Teleopti.Messaging.Installer.exe", path);
            //string fileName = String.Format(CultureInfo.InvariantCulture, "{0}\\Teleopti.Messaging.Svc.exe", path);
            //installer.StartInfo.Arguments = String.Format(CultureInfo.InvariantCulture, "\"{0}\" {1} \"{2}\" {3} \"{4}\"", fileName, name, displayName, install, _controller.ConnectionString);
            //installer.StartInfo.UseShellExecute = false;
            //installer.StartInfo.RedirectStandardOutput = true;
            //installer.StartInfo.RedirectStandardError = true;
            //installer.EnableRaisingEvents = true;
            //installer.StartInfo.CreateNoWindow = true;    
            //// see below for output handler    
            //installer.ErrorDataReceived += OnOutputDataReceived;
            //installer.OutputDataReceived += OnOutputDataReceived;
            //installer.Start();
            //installer.BeginErrorReadLine();
            //installer.BeginOutputReadLine();

            // Start an executable that will install the service.
            _controller.ConnectionString = ConfigurationManager.AppSettings["MessageBroker"];
            Process installer = new Process();
            installer.StartInfo.FileName = String.Format(CultureInfo.CurrentCulture, "{0}\\Teleopti.Messaging.Installer.exe", path);
            string fileName = String.Format(CultureInfo.CurrentCulture, "{0}\\Teleopti.Messaging.Svc.exe", path);
            installer.StartInfo.Arguments = String.Format(CultureInfo.CurrentCulture, "\"{0}\" {1} \"{2}\" {3} \"{4}\"", fileName, name, displayName, install, _controller.ConnectionString);
            installer.Start();
        }

        // Do not delete, works flawlessly with Windows XP but not with UAC and Windows VISTA
        //private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
        //{
        //    if (e.Data != null && !e.Data.StartsWith("Press any key to exit ...", StringComparison.Ordinal))
        //    {
        //        _controller.AppendStatus(e.Data);
        //    }
        //}

        public void LaunchMMC()
        {
            Process launcher = new Process();
            launcher.StartInfo.FileName = @"C:\WINDOWS\system32\services.msc";
            launcher.StartInfo.Arguments = "";
            launcher.Start();
        }

        public ServiceManagerController Controller
        {
            get { return _controller;  }
            set { _controller = value; }
        }

        public void StartService(string name)
        {
            AsyncCallback cb = new AsyncCallback(StartServiceAsyncResult);
            StringResponseAsyncHandler stringResponse = new StringResponseAsyncHandler(StartServiceAsync);
            stringResponse.BeginInvoke(name, cb, null);                
        }

        private string StartServiceAsync(string name)
        {
            return ServiceManagement(name, "StartService");
        }

        private void StartServiceAsyncResult(IAsyncResult ar)
        {
            try
            {
                AsyncResult asyncResult = (AsyncResult)ar;
                StringResponseAsyncHandler stringResponse = (StringResponseAsyncHandler)asyncResult.AsyncDelegate;
                string message = stringResponse.EndInvoke(ar);
                _controller.UpdateStatus(message);
            }
            catch (Exception exc)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exc.ToString());
                _controller.UpdateStatus("Failed : " + exc);
            }
        }

        private void OnException(object sender, UnhandledExceptionEventArgs e)
        {
            _controller.UpdateStatus(e.ExceptionObject.ToString());
        }

        private void OnEventMessage(object sender, EventMessageArgs e)
        {
            _controller.UpdateStatus(e.Message.ToString());
        }

        public void StopService(string name)
        {
            AsyncCallback cb = new AsyncCallback(StopServiceAsyncResult);
            StringResponseAsyncHandler stringResponse = new StringResponseAsyncHandler(StopServiceAsync);
            stringResponse.BeginInvoke(name, cb, null);    
        }

        private string StopServiceAsync(string name)
        {
            return ServiceManagement(name, "StopService");
        }

        private void StopServiceAsyncResult(IAsyncResult ar)
        {
            try
            {
                AsyncResult asyncResult = (AsyncResult)ar;
                StringResponseAsyncHandler stringResponse = (StringResponseAsyncHandler)asyncResult.AsyncDelegate;
                string message = stringResponse.EndInvoke(ar);
                _controller.UpdateStatus(message);
            }
            catch(Exception exc)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exc.ToString());
                _controller.UpdateStatus("Failed : " + exc);
            }
        }

        public void QueryStatus(string name)
        {
            AsyncCallback cb = new AsyncCallback(QueryStatusAsyncResult);
            StringResponseAsyncHandler stringResponse = new StringResponseAsyncHandler(QueryStatusAsync);
            stringResponse.BeginInvoke(name, cb, null);    
        }

        public string QueryStatusAsync(string name)
        {
            return GetServiceStatus(name);
        }

        private void QueryStatusAsyncResult(IAsyncResult ar)
        {
            try
            {
                AsyncResult asyncResult = (AsyncResult)ar;
                StringResponseAsyncHandler stringResponse = (StringResponseAsyncHandler)asyncResult.AsyncDelegate;
                string message = stringResponse.EndInvoke(ar);
                _controller.UpdateStatus(message);
            }
            catch(Exception exc)
            {
                BaseLogger.Instance.WriteLine(EventLogEntryType.Error, GetType(), exc.ToString());
                _controller.UpdateStatus("Failed");
            }
        }


        protected string ServiceManagement(string name, string method)
        {
            try
            {
                ManagementPath path = new ManagementPath();
                path.Server = Environment.MachineName;
                path.NamespacePath = @"root\CIMV2";
                path.RelativePath = String.Format(CultureInfo.InvariantCulture, "Win32_service.Name='{0}'", name);
                ManagementObject service = new ManagementObject(path);
                service.InvokeMethod(method, null, null);
                StringBuilder sb = new StringBuilder();
                sb.Append("Performing action ");
                sb.Append(method);
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
                sb.Append("Description :  ");
                sb.Append(service.GetPropertyValue("Description"));
                sb.Append(Environment.NewLine);
                sb.Append("PathName :   ");
                sb.Append(service.GetPropertyValue("PathName"));
                sb.Append(Environment.NewLine);
                sb.Append("ServiceType : ");
                sb.Append(service.GetPropertyValue("ServiceType"));
                sb.Append(Environment.NewLine);
                sb.Append("StartMode :  ");
                sb.Append(service.GetPropertyValue("StartMode"));
                sb.Append(Environment.NewLine);
                sb.Append("State :  ");
                sb.Append(service.GetPropertyValue("State"));
                sb.Append(Environment.NewLine);
                return sb.ToString();
            }
            catch (Exception exc)
            {
                return exc.ToString();
            }
        }

        protected string GetServiceStatus(string name)
        {
            try
            {
                ManagementPath path = new ManagementPath();
                path.Server = Environment.MachineName;
                path.NamespacePath = @"root\CIMV2";
                path.RelativePath = String.Format(CultureInfo.InvariantCulture, "Win32_service.Name='{0}'", name);
                ManagementObject service = new ManagementObject(path);
                StringBuilder sb = new StringBuilder();
                sb.Append("Performing action QueryStatus");
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
                sb.Append("Description :  ");
                sb.Append(service.GetPropertyValue("Description"));
                sb.Append(Environment.NewLine);
                sb.Append("PathName :   ");
                sb.Append(service.GetPropertyValue("PathName"));
                sb.Append(Environment.NewLine);
                sb.Append("ServiceType : ");
                sb.Append(service.GetPropertyValue("ServiceType"));
                sb.Append(Environment.NewLine);
                sb.Append("StartMode :  ");
                sb.Append(service.GetPropertyValue("StartMode"));
                sb.Append(Environment.NewLine);
                sb.Append("State :  ");
                sb.Append(service.GetPropertyValue("State"));
                sb.Append(Environment.NewLine);
                return sb.ToString();
            }
            catch (ManagementException exc)
            {
                if(exc.ErrorCode == ManagementStatus.NotFound)
                {
                    return "Service not found.";
                }
                return exc.ErrorCode.ToString();
            }
        }

        #pragma warning restore 1692

        public void MessageBrokerStart()
        {
            if (_messageBroker == null)
            {
                _messageBroker = MessageBrokerImplementation.GetInstance(_controller.ConnectionString);
                _messageBroker.StartMessageBroker();
                EventHandler<EventMessageArgs> eventHandler = new EventHandler<EventMessageArgs>(OnEventMessage);
                _messageBroker.RegisterEventSubscription(string.Empty,Guid.Empty, eventHandler, typeof(IChat));
				_messageBroker.RegisterEventSubscription(string.Empty, Guid.Empty, eventHandler, typeof(IEventHeartbeat));
				_messageBroker.RegisterEventSubscription(string.Empty, Guid.Empty, eventHandler, typeof(IExternalAgentState));
                _messageBroker.ExceptionHandler += new EventHandler<UnhandledExceptionEventArgs>(OnException);
            }
        }

        public void MessageBrokerStop()
        {
            if(_messageBroker != null)
            {
                _messageBroker.EventMessageHandler -= new EventHandler<EventMessageArgs>(OnEventMessage);
                _messageBroker.ExceptionHandler -= new EventHandler<UnhandledExceptionEventArgs>(OnException);
                _messageBroker.StopMessageBroker();
                ((MessageBrokerBase)_messageBroker).Dispose();
                _messageBroker = null;
            }
        }


    }
}
