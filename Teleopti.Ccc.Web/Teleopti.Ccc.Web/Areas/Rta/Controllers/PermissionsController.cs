﻿using System;
using System.Globalization;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;

using Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	public class PermissionsController : ApiController
	{
		private readonly PermissionsViewModelBuilder _builder;

		public PermissionsController(PermissionsViewModelBuilder builder)
		{
			_builder = builder;
		}

		[UnitOfWork, HttpGet, Route("api/Adherence/Permissions")]
		public virtual IHttpActionResult Load(Guid? personId = null, string date = null)
		{
			var dateTime = default(DateTime?);
			if (date != null)
				dateTime = DateTime.ParseExact(date, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None);
			return Ok(_builder.Build(personId, dateTime));
		}
	}
}