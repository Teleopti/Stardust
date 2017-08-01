using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.GroupPageCreator;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Global.Core;
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
		public virtual IHttpActionResult AvailableStructuredGroupPages(DateOnlyPeriod period)
		{
			return Ok(_groupPageViewModelFactory.CreateViewModel(period));
		}
		[UnitOfWork, HttpGet, Route("api/GroupPage/AvailableGroupPages")]
		public virtual IHttpActionResult AvailableGroupPages(DateTime date)
		{
			return Ok(_groupPageViewModelFactory.CreateViewModel(new DateOnly(date)));
		}
	}
}