﻿using System;
using System.Diagnostics;
using System.Globalization;
using System.ServiceProcess;

namespace Teleopti.Ccc.Sdk.ServiceBus.Host
{
    public partial class ServiceBusHost : ServiceBase
    {
    	private ServiceBusRunner serviceBusRunner;

    	public ServiceBusHost()
        {
            InitializeComponent();

			serviceBusRunner = new ServiceBusRunner(logUnhandledException, logStartupException, RequestAdditionalTime);
        }

    	private void logUnhandledException(Exception exception)
    	{
    		EventLog.WriteEntry(
    			string.Format(CultureInfo.InvariantCulture,
    			              "An unhandled exception occurred in the Teleopti Service Bus. \nThe exception message: {0}. \nThe stack trace: {1}.",
    			              exception.Message, exception.StackTrace), EventLogEntryType.Warning);
    	}

    	protected override void OnStart(string[] args)
        {
            serviceBusRunner.Start();
        }

    	private void logStartupException(Exception exception)
    	{
    		EventLog.WriteEntry(
    			string.Format(CultureInfo.CurrentCulture,
    			              "An exception was encountered upon starting the Teleopti Service Bus. \nThe exception message: {0}. \nThe stack trace: {1}.",
    			              exception.Message, exception.StackTrace), EventLogEntryType.Warning);
    	}

    	protected override void OnStop()
        {
            serviceBusRunner.Stop();
        }
    }
}
