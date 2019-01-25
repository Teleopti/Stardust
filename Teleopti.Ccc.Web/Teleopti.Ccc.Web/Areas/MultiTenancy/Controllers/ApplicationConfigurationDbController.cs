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
			return Ok(_appConfig.GetAll());
		}

		[TenantUnitOfWork]
		[HttpGet, Route("Configuration/GetServerValue")]
		public virtual IHttpActionResult GetServerValue(string key)
		{
			if (Enum.TryParse<ServerConfigurationKey>(key, out var enumKey))
			{
				return Ok(_appConfig.GetServerValue(enumKey));
			}
			return Ok<string>(null);
		}

		[TenantUnitOfWork]
		[HttpGet, Route("Configuration/GetTenantValue")]
		public virtual IHttpActionResult GetTenantValue(string key)
		{
			if (Enum.TryParse<TenantApplicationConfigKey>(key, out var enumKey))
			{
				return Ok(_appConfig.GetTenantValue(enumKey));
			}
			return Ok<string>(null);
		}
	}
}