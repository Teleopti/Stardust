using System;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Configuration;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Ccc.IocCommon.Configuration;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	public class ConfigurationController : ApiController
	{
		private readonly ICurrentDataSource _dataSource;

		public ConfigurationController(ICurrentDataSource dataSource)
		{
			_dataSource = dataSource;
		}

		[HttpGet, Route("Rta/Configuration/Validate")]
		[TenantScope]
		public virtual IHttpActionResult Change([FromUri] string tenant = null)
		{
			return Ok();
//			var random = new Random().Next(0, 100);
//			if (random < 25)
//				throw new Exception();
//			if (random < 50)
//				return Ok(Enumerable.Empty<ConfigurationValidationViewModel>());
//			if (random < 75)
//				return Ok(new[]
//				{
//					new ConfigurationValidationViewModel
//					{
//						Resource = "LoggedOutStateGroupMissingInConfiguration",
//						Data = new[] {"Blip blop unit", _dataSource.CurrentName()}
//					}
//				});
//			return Ok(new[]
//			{
//				new ConfigurationValidationViewModel
//				{
//					Resource = "LoggedOutStateGroupMissingInConfiguration",
//					Data = new[] {"Blip blop unit", _dataSource.CurrentName()}
//				},
//				new ConfigurationValidationViewModel
//				{
//					Resource = "LoggedOutStateGroupMissingInRtaService",
//					Data = new[] {"Blip blop unit", _dataSource.CurrentName()}
//				},
//				new ConfigurationValidationViewModel
//				{
//					Resource = "DefaultStateGroupMissingInConfiguration",
//					Data = new[] {"Blip blop unit", _dataSource.CurrentName()}
//				},
//				new ConfigurationValidationViewModel
//				{
//					Resource = "DefaultStateGroupMissingInRtaService",
//					Data = new[] {"Blip blop unit", _dataSource.CurrentName()}
//				},
//			});
		}
	}
}