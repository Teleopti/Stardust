using System.Configuration;
using System.ServiceProcess;
using Teleopti.Analytics.Etl.ServiceLogic;

namespace Teleopti.Analytics.Etl.ServiceHost
{
	public partial class Service1 : ServiceBase
	{
		private EtlJobStarter _serviceLogic;

		public Service1()
		{
			InitializeComponent();            
		}

		protected override void OnStart(string[] args)
		{
			_serviceLogic = new EtlJobStarter();
			_serviceLogic.NeedToStopService += _serviceLogic_NeedToStopService;
			_serviceLogic.Start();
		}

		void _serviceLogic_NeedToStopService(object sender, System.EventArgs e)
		{
			Stop();
		}

		protected override void OnStop()
		{
			_serviceLogic.Dispose();
		}
	}
}