using System;
using System.Web.Http;
using System.Web.Http.ModelBinding;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Global.Core;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Global
{
	public class GroupPageController : ApiController
	{
		private readonly GroupPageViewModelFactory _groupPageViewModelFactory;

		public GroupPageController(GroupPageViewModelFactory groupPageViewModelFactory)
		{
			_groupPageViewModelFactory = groupPageViewModelFactory;
		}

		[UnitOfWork, HttpGet, Route("api/GroupPage/AvailableStructuredGroupPages")]
		public virtual IHttpActionResult AvailableStructuredGroupPages([ModelBinder(typeof(DateOnlyModelBinder))]DateOnly startDate, [ModelBinder(typeof(DateOnlyModelBinder))]DateOnly endDate)
		{
			return Ok(_groupPageViewModelFactory.CreateViewModel(new DateOnlyPeriod(startDate, endDate), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules));
		}

		[UnitOfWork, HttpGet, Route("api/GroupPage/AvailableGroupPages")]
		public virtual IHttpActionResult AvailableGroupPages(DateTime date)
		{
			return Ok(_groupPageViewModelFactory.CreateViewModel(new DateOnly(date)));
		}

		[UnitOfWork, HttpGet, Route("api/GroupPage/AvailableStructuredGroupPagesForRequests")]
		public virtual IHttpActionResult AvailableStructuredGroupPagesForRequests([ModelBinder(typeof(DateOnlyModelBinder))]DateOnly startDate, [ModelBinder(typeof(DateOnlyModelBinder))]DateOnly endDate)
		{
			return Ok(_groupPageViewModelFactory.CreateViewModel(new DateOnlyPeriod(startDate, endDate), DefinedRaptorApplicationFunctionPaths.WebRequests));
		}
	}
}