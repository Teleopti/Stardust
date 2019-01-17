using System;
using System.Web.Http;
using Teleopti.Ccc.Domain.MultiTenancy;

namespace Teleopti.Ccc.Web.Areas.MultiTenancy
{
	public class ApplicationConfigurationDbController : ApiController
	{
		private readonly IApplicationConfigurationDbProvider _appConfig;

		public ApplicationConfigurationDbController(IApplicationConfigurationDbProvider appConfig)
		{
			_appConfig = appConfig;
		}

		[TenantUnitOfWork]
		[HttpGet,Route("Configuration/GetAll")]
		public virtual IHttpActionResult GetAll()
		{
			return Ok(_appConfig.GetConfiguration());
		}

		[TenantUnitOfWork]
		[HttpGet, Route("Configuration/TryGetServerValue")]
		public virtual IHttpActionResult TryGetServerValue(string key, string defaultValue)
		{
			return Ok(_appConfig.TryGetServerValue(key, defaultValue));
		}

		[TenantUnitOfWork]
		[HttpGet, Route("Configuration/TryGetTenantValue")]
		public virtual IHttpActionResult TryGetTenantValue(string key, string defaultValue)
		{
			return Ok(_appConfig.TryGetTenantValue(key, defaultValue));
		}
	}
}