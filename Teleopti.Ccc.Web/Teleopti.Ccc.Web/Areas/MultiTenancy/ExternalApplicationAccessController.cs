﻿using System;
using System.Linq;
using System.Text;
using System.Web.Http;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy
{
	public class ExternalApplicationAccessController : ApiController
	{
		public const string Salt = "$2a$10$WbJpBWPWsrVOlMze27m0ne";

		private readonly IPersistExternalApplicationAccess _persistExternalApplicationAccess;
		private readonly ICurrentTenantUser _currentTenantUser;
		private readonly IFindExternalApplicationAccess _findExternalApplicationAccess;
		private readonly HashFromToken _hashFromToken;

		public ExternalApplicationAccessController(IPersistExternalApplicationAccess persistExternalApplicationAccess, ICurrentTenantUser currentTenantUser, IFindExternalApplicationAccess findExternalApplicationAccess, HashFromToken hashFromToken)
		{
			_persistExternalApplicationAccess = persistExternalApplicationAccess;
			_currentTenantUser = currentTenantUser;
			_findExternalApplicationAccess = findExternalApplicationAccess;
			_hashFromToken = hashFromToken;
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("api/token")]
		public virtual IHttpActionResult Add([FromBody]NewExternalApplicationModel model)
		{
			var access = new ExternalApplicationAccess(_currentTenantUser.CurrentUser().Id, model.Name, "");
			var token = Convert.ToBase64String(Encoding.UTF8.GetBytes((Guid.NewGuid() + Guid.NewGuid().ToString()).Replace("-", "")));
			access.SetHash(_hashFromToken.Generate(Salt, token));
			_persistExternalApplicationAccess.Persist(access);
			return Ok(new NewExternalApplicationResponseModel{Token = token});
		}

		[HttpPost]
		[TenantUnitOfWork]
		[NoTenantAuthentication]
		[Route("api/token/verify")]
		public virtual IHttpActionResult Verify([FromBody]string token)
		{
			var hash = _hashFromToken.Generate(Salt, token);
			var found = _findExternalApplicationAccess.FindByTokenHash(hash);
			if (found == null)
			{
				return Ok();
			}
			return Ok(found.PersonId);
		}

		[HttpGet]
		[TenantUnitOfWork]
		[Route("api/token")]
		public virtual IHttpActionResult Get()
		{
			var applications = _findExternalApplicationAccess.FindByPerson(_currentTenantUser.CurrentUser().Id);
			return Ok(applications.Select(a => new ExternalApplicationModel{Id = a.Id,Name = a.Name,TimeStamp = a.CreatedOn}).ToArray());
		}

		[HttpDelete]
		[TenantUnitOfWork]
		[Route("api/token/{id}")]
		public virtual void Remove([FromUri]int id)
		{
			_persistExternalApplicationAccess.Remove(id,_currentTenantUser.CurrentUser().Id);
		}
	}
}