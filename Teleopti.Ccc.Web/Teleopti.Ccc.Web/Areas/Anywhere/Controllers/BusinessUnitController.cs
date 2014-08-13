using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Common;
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
		private readonly ICurrentBusinessUnit _currentBusinessUnit;

		public BusinessUnitController(IBusinessUnitRepository buRepository, ILoggedOnUser loggedOnUser, ICurrentBusinessUnit currentBusinessUnit)
		{
			_buRepository = buRepository;
			_loggedOnUser = loggedOnUser;
			_currentBusinessUnit = currentBusinessUnit;
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult Index()
		{
			var currentUserPermissionInfo = _loggedOnUser.CurrentUser().PermissionInformation;
			if (currentUserPermissionInfo.HasAccessToAllBusinessUnits())
			{
				var currentBusinessUnit = _buRepository.Get(_currentBusinessUnit.Current().Id.GetValueOrDefault());
				var businessUnits = new List<BusinessUnitViewModel>
				{
					new BusinessUnitViewModel {Id = currentBusinessUnit.Id.GetValueOrDefault(), Name = currentBusinessUnit.Name}
				};
				businessUnits.AddRange(from b in _buRepository.LoadAllBusinessUnitSortedByName()
					where b != currentBusinessUnit
					select new BusinessUnitViewModel
					{
						Id = b.Id.Value,
						Name = b.Name
					});
				return Json(businessUnits, JsonRequestBehavior.AllowGet);
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