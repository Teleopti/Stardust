using System;
using System.IO;
using System.Net;
using Autofac;
using log4net.Config;
using Teleopti.Ccc.Domain.Config;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.Sdk.ServiceBus.Container;
using Teleopti.Ccc.Sdk.ServiceBus.Payroll.FormatLoader;

namespace Teleopti.Ccc.Sdk.ServiceBus
{
	[Serializable]
	public class ServiceBusRunner
	{
		private readonly Action<int> _requestAdditionalTime;

		[NonSerialized]
		private ConfigFileDefaultHost _requestBus;
		[NonSerialized]
		private ConfigFileDefaultHost _generalBus;
		[NonSerialized]
		private ConfigFileDefaultHost _denormalizeBus;
		[NonSerialized]
		private ConfigFileDefaultHost _payrollBus;
		[NonSerialized]
		private ConfigFileDefaultHost _rtaBus;

		public ServiceBusRunner(Action<int> requestAdditionalTime)
		{
			_requestAdditionalTime = requestAdditionalTime;
		}

		public void Start()
		{
			hostServiceStart();
}

		private void hostServiceStart()
		{
			_requestAdditionalTime(60000);

			XmlConfigurator.Configure(new FileInfo("log4net.config"));

			ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true; //ignoreInvalidCertificate
			ServicePointManager.DefaultConnectionLimit = 50;

			var toggleManager = CommonModule.ToggleManagerForIoc(new IocArgs(new ConfigReader()));
			var sharedContainer = new ContainerBuilder().Build();
			new ContainerConfiguration(sharedContainer, toggleManager).Configure(null);

			_requestBus = new ConfigFileDefaultHost("RequestQueue.config", new BusBootStrapper(makeContainer(toggleManager, sharedContainer)));
			_requestBus.Start();

			_generalBus = new ConfigFileDefaultHost("GeneralQueue.config", new GeneralBusBootStrapper(makeContainer(toggleManager, sharedContainer)));
			_generalBus.Start();

			_denormalizeBus = new ConfigFileDefaultHost("DenormalizeQueue.config", new DenormalizeBusBootStrapper(makeContainer(toggleManager, sharedContainer)));
			_denormalizeBus.Start();

			_rtaBus = new ConfigFileDefaultHost("RtaQueue.config", new RtaBusBootStrapper(makeContainer(toggleManager, sharedContainer)));
			_rtaBus.Start();

			new PayrollDllCopy(new SearchPath()).CopyPayrollDll();

			_payrollBus = new ConfigFileDefaultHost("PayrollQueue.config", new PayrollBusBootStrapper(makeContainer(toggleManager, sharedContainer)));
			_payrollBus.Start();

			AppDomain.MonitoringIsEnabled = true;
		}

		private static IContainer makeContainer(IToggleManager toggleManager, IContainer sharedContainer)
		{
			var container = new ContainerBuilder().Build();
			new ContainerConfiguration(container, toggleManager).Configure(sharedContainer);
			return container;
		}

		public void Stop()
		{
			hostServiceStop();
		}

		private void hostServiceStop()
		{
			_requestAdditionalTime(60000);
			DisposeBusHosts();
		}

		public void DisposeBusHosts()
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
			if (_generalBus != null)
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
			if (_payrollBus != null)
			{
				try
				{
					_payrollBus.Dispose();
				}
				catch (Exception)
				{
				}
			}
			if (_rtaBus != null)
			{
				try
				{
					_rtaBus.Dispose();
				}
				catch (Exception)
				{
				}
			}
		}
	}
}