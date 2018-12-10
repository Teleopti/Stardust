using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Logon.Aspects;
using Teleopti.Wfm.Adherence.Configuration;

namespace Teleopti.Ccc.Web.Areas.Rta.Controllers
{
	public class ConfigurationController : ApiController
	{
		private readonly ConfigurationValidator _validator;
		private readonly ICurrentDataSource _dataSource;

		public ConfigurationController(ConfigurationValidator validator, ICurrentDataSource dataSource)
		{
			_validator = validator;
			_dataSource = dataSource;
		}

		[HttpGet, Route("Rta/Configuration/Validate")]
		[TenantScope]
		public virtual IHttpActionResult Validate([FromUri] string tenant = null)
		{
			if( _dataSource.Current() == null )
				return Ok();

			return Ok(validate());
		}

		[AllBusinessUnitsUnitOfWork]
		protected virtual IEnumerable<ConfigurationValidationViewModel> validate()
		{
			return _validator.Validate();
		}
	}
}