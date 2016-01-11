using System.Configuration;
using System.ServiceProcess;
using Teleopti.Analytics.Etl.ServiceLogic;

namespace Teleopti.Analytics.Etl.ServiceHost
{
	public partial class Service1 : ServiceBase
	{
		private EtlService _service;

		public Service1()
		{
			InitializeComponent();            
		}

		protected override void OnStart(string[] args)
		{
			_service = new EtlService();
			_service.Start(Stop);
		}

		protected override void OnStop()
		{
			_service.Dispose();
		}
	}
}