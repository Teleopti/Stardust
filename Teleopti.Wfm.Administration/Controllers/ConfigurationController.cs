using System;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Controllers
{
	[TenantTokenAuthentication]
	public class ConfigurationController : ApiController
	{
		private readonly IServerConfigurationRepository _serverConfigurationRepository;

		public ConfigurationController(IServerConfigurationRepository serverConfigurationRepository)
		{
			_serverConfigurationRepository = serverConfigurationRepository;
		}

		[HttpGet]
		[TenantUnitOfWork]
		[Route("GetAllConfigurations")]
		public virtual JsonResult<IEnumerable<ConfigurationModel>> GetAllConfigurations()
		{
			var serverConfigurations = _serverConfigurationRepository.AllConfigurations();
			return Json(map(serverConfigurations));
		}

		private IEnumerable<ConfigurationModel> map(IEnumerable<ServerConfiguration> serverConfigurations)
		{
			var result = new List<ConfigurationModel>();
			foreach (var serverConfiguration in serverConfigurations)
			{
				var configurationModel = new ConfigurationModel
				{
					Key = serverConfiguration.Key,
					Value = serverConfiguration.Value
				};
				if (configurationModel.Key == "FrameAncestors")
				{
					configurationModel.Description = "Add urls to let mytime or ASM widget work in an iframe.";
				}
				result.Add(configurationModel);
			}
			return result;
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