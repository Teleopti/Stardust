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
			if (Enum.TryParse<ServerConfigurationKey>(key, out var serverConfigurationKey))
			{
				return Ok(_appConfig.TryGetServerValue(serverConfigurationKey, defaultValue));
			}
			return Ok(defaultValue);
		}

		[TenantUnitOfWork]
		[HttpGet, Route("Configuration/TryGetTenantValue")]
		public virtual IHttpActionResult TryGetTenantValue(string key, string defaultValue)
		{
			if (Enum.TryParse<TenantApplicationConfigKey>(key, out var tenatConfigurationKey))
			{
				return Ok(_appConfig.TryGetTenantValue(tenatConfigurationKey, defaultValue));
			}
			return Ok(defaultValue);
		}
	}
}