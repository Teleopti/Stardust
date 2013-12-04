using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.ViewModelFactory;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class TeamController : Controller
	{
		private readonly ITeamViewModelFactory _teamViewModelFactory;

		public TeamController(ITeamViewModelFactory teamViewModelFactory)
		{
			_teamViewModelFactory = teamViewModelFactory;
		}

		[UnitOfWorkAction]
		public JsonResult Teams(DateOnly? date)
		{
			if (!date.HasValue)
				date = DateOnly.Today;
			return Json(_teamViewModelFactory.CreateTeamOrGroupOptionsViewModel(date.Value), JsonRequestBehavior.AllowGet);
		}
	}
}