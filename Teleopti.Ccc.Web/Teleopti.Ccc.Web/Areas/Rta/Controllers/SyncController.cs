using System.Web.Mvc;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	public class SyncController : Controller
	{
		private readonly IStateStreamSynchronizer _synchronizer;

		public SyncController(IStateStreamSynchronizer synchronizer)
		{
			_synchronizer = synchronizer;
		}

		public string Index()
		{
			//todo: tenant how to solve this what Tenant??
			_synchronizer.Sync("Teleopti WFM");
			return "Synchronization done!";
		}
	}
}
