using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.Global.Core;
using Teleopti.Ccc.Web.Core.Extensions;


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
		public virtual IHttpActionResult AvailableStructuredGroupPages(DateTime startDate, DateTime endDate)
		{
			return Ok(_groupPageViewModelFactory.CreateViewModel(new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate)), DefinedRaptorApplicationFunctionPaths.MyTeamSchedules));
		}

		[UnitOfWork, HttpGet, Route("api/GroupPage/AvailableGroupPages")]
		public virtual IHttpActionResult AvailableGroupPages(DateTime date)
		{
			return Ok(_groupPageViewModelFactory.CreateViewModel(new DateOnly(date)));
		}

		[UnitOfWork, HttpGet, Route("api/GroupPage/AvailableStructuredGroupPagesForRequests")]
		public virtual IHttpActionResult AvailableStructuredGroupPagesForRequests(DateTime startDate, DateTime endDate)
		{
			return Ok(_groupPageViewModelFactory.CreateViewModel(new DateOnlyPeriod(new DateOnly(startDate), new DateOnly(endDate)), DefinedRaptorApplicationFunctionPaths.WebRequests));
		}
	}
}