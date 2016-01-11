using System.Configuration;
using System.ServiceProcess;
using Teleopti.Analytics.Etl.ServiceLogic;

namespace Teleopti.Analytics.Etl.ServiceHost
{
	public partial class Service1 : ServiceBase
	{
		private EtlServiceHost _host;

		public Service1()
		{
			InitializeComponent();            
		}

		protected override void OnStart(string[] args)
		{
			_host = new EtlServiceHost();
			_host.Start(Stop);
		}

		protected override void OnStop()
		{
			_host.Dispose();
		}
	}
}