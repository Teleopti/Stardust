using System.Web.Mvc;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	public class ActivityChangeController : Controller
	{
		private readonly IRta _rta;

		public ActivityChangeController(IRta rta)
		{
			_rta = rta;
		}

		[HttpPost]
		// seems this works without the CheckForActivityChangeWebModel at this side...
		public void CheckFor(CheckForActivityChangeInputModel model)
		{
			_rta.CheckForActivityChange(model);
		}
	}
}
