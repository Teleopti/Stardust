using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using log4net.Config;
using Rhino.ServiceBus.Hosting;

namespace Teleopti.Ccc.Sdk.ServiceBus.Host
{
    public partial class ServiceBusHost : ServiceBase
    {
		private ConfigFileDefaultHost _requestBus;
		private ConfigFileDefaultHost _generalBus;
		private ConfigFileDefaultHost _denormalizeBus;

        public ServiceBusHost()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                HostServiceStart();
            }
            catch (InvalidOperationException exception)
            {
                //Handles problems with old schema in subscription storage by removing old stuff
                EventLog.WriteEntry(
                    string.Format(System.Globalization.CultureInfo.CurrentCulture,
                                  "An exception was encountered upon starting the Teleopti Service Bus. \nThe exception message: {0}. \nThe stack trace: {1}.",
                                  exception.Message, exception.StackTrace), EventLogEntryType.Warning);
                throw;
            }
        }

        private void HostServiceStart()
        {
            RequestAdditionalTime(60000);
            XmlConfigurator.Configure(new FileInfo("log4net.config"));

            _requestBus = new ConfigFileDefaultHost();
            _requestBus.UseFileBasedBusConfiguration("RequestQueue.config");
            _requestBus.Start<BusBootStrapper>();

			_generalBus = new ConfigFileDefaultHost();
            _generalBus.UseFileBasedBusConfiguration("GeneralQueue.config");
            _generalBus.Start<BusBootStrapper>();

			_denormalizeBus = new ConfigFileDefaultHost();
			_denormalizeBus.UseFileBasedBusConfiguration("DenormalizeQueue.config");
			_denormalizeBus.Start<DenormalizeBusBootStrapper>();
        }

        protected override void OnStop()
        {
            HostServiceStop();
        }

        private void HostServiceStop()
        {
            RequestAdditionalTime(60000);
            DisposeBusHosts();
        }

        private void DisposeBusHosts()
        {
            if (_requestBus != null)
            {
                try
                {
                    _requestBus.Dispose();
                }
                catch (Exception)
                {
                }
            }
            if (_generalBus!=null)
            {
                try
                {
                    _generalBus.Dispose();
                }
                catch (Exception)
                {
                }
            }
			if (_denormalizeBus != null)
			{
				try
				{
					_denormalizeBus.Dispose();
				}
				catch (Exception)
				{
				}
			}
        }
    }
}
