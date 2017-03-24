using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Admin;
using Teleopti.Support.Shared;
using Teleopti.Wfm.Administration.Core;
using Teleopti.Wfm.Administration.Models;

namespace Teleopti.Wfm.Administration.Controllers
{
	[TenantTokenAuthentication]
	public class ConfigurationController : ApiController
	{
		private readonly IServerConfigurationRepository _serverConfigurationRepository;
		private readonly IFrameAncestorsUpdator _ancestorsUpdator;

		public ConfigurationController(IServerConfigurationRepository serverConfigurationRepository, IFrameAncestorsUpdator ancestorsUpdator)
		{
			_serverConfigurationRepository = serverConfigurationRepository;
			_ancestorsUpdator = ancestorsUpdator;
		}

		[HttpGet]
		[TenantUnitOfWork]
		[Route("GetAllConfigurations")]
		public virtual JsonResult<IEnumerable<ConfigurationModel>> GetAllConfigurations()
		{
			return Json(_serverConfigurationRepository.AllConfigurations().Select(c => new ConfigurationModel
			{
				Key = c.Key,
				Value = c.Value
			}));
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
				_ancestorsUpdator.Update(model.Value);
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