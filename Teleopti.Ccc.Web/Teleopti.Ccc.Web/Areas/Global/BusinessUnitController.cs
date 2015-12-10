using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class BusinessUnitController : ApiController
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

		[UnitOfWork, HttpGet, Route("api/BusinessUnit")]
		public virtual IHttpActionResult Index()
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
				return Ok(businessUnits);
			}
			return Ok(from b in currentUserPermissionInfo.BusinessUnitAccessCollection()
				select new BusinessUnitViewModel
				{
					Id = b.Id.Value,
					Name = b.Name
				});
		}

		[UnitOfWork, HttpGet, Route("api/BusinessUnit/Current")]
		public virtual IHttpActionResult Current()
		{
			var currentBusinessUnit = _buRepository.Get(_currentBusinessUnit.Current().Id.GetValueOrDefault());
				
			return Ok(new BusinessUnitViewModel
				{
					Id = currentBusinessUnit.Id.GetValueOrDefault(),
					Name = currentBusinessUnit.Name
				});
		}
	}
}