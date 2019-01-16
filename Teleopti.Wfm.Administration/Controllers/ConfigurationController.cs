using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Controllers
{
	[TenantTokenAuthentication]
	public class ConfigurationController : ApiController
	{
		private readonly IServerConfigurationRepository _serverConfigurationRepository;
		private const string logonAttempsDays = "PreserveLogonAttemptsDays";

		public ConfigurationController(IServerConfigurationRepository serverConfigurationRepository)
		{
			_serverConfigurationRepository = serverConfigurationRepository;
		}

		[HttpGet]
		[TenantUnitOfWork]
		[Route("AllConfigurations")]
		public virtual JsonResult<IEnumerable<ServerConfiguration>> GetAllConfigurations()
		{
			var serverConfigurations = _serverConfigurationRepository.AllConfigurations();
			return Json(serverConfigurations);
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("Configuration")]
		public virtual JsonResult<string> GetConfiguration([FromBody]string key)
		{
			return Json(_serverConfigurationRepository.Get(key));
		}

		[HttpPost]
		[TenantUnitOfWork]
		[Route("SaveConfiguration")]
		public virtual UpdateConfigurationResultModel Save(UpdateConfigurationModel model)
		{
			if (string.IsNullOrEmpty(model.Key))
				return new UpdateConfigurationResultModel
				{
					Success = false,
					Message = "Key can't be empty."
				};

			if (model.Key == logonAttempsDays)
			{
				if(!int.TryParse(model.Value, out _))
					return new UpdateConfigurationResultModel
					{
						Success = false,
						Message = "The value must be an integer."
					};
			}

			try
			{
				_serverConfigurationRepository.Update(model.Key, model.Value);
			}
			catch (Exception exception)
			{
				return new UpdateConfigurationResultModel
				{
					Success = false,
					Message = exception.Message
				};
			}

			return new UpdateConfigurationResultModel
			{
				Success = true,
				Message = "Updated the configuration successfully."
			};
		}
	}
}