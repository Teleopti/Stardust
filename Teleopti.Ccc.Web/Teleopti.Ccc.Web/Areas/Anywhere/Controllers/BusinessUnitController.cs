using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class BusinessUnitController : Controller
	{
		private readonly IBusinessUnitRepository _buRepository;
		private readonly ILoggedOnUser _loggedOnUser;

		public BusinessUnitController(IBusinessUnitRepository buRepository, ILoggedOnUser loggedOnUser)
		{
			_buRepository = buRepository;
			_loggedOnUser = loggedOnUser;
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult Index()
		{
			var currentUserPermissionInfo = _loggedOnUser.CurrentUser().PermissionInformation;
			if (currentUserPermissionInfo.HasAccessToAllBusinessUnits())
			{
				return Json(from b in _buRepository.LoadAllBusinessUnitSortedByName()
					select new BusinessUnitViewModel
					{
						Id = b.Id.Value,
						Name = b.Name
					}, JsonRequestBehavior.AllowGet);
			}
			return Json(from b in currentUserPermissionInfo.BusinessUnitAccessCollection()
				select new BusinessUnitViewModel
				{
					Id = b.Id.Value,
					Name = b.Name
				}, JsonRequestBehavior.AllowGet);
		}
	}
}