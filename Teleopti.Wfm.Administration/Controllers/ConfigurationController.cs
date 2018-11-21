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
		public virtual JsonResult<IEnumerable<ConfigurationModel>> GetAllConfigurations()
		{
			var serverConfigurations = _serverConfigurationRepository.AllConfigurations().ToArray();

			return Json(map(serverConfigurations));
		}

		private IEnumerable<ConfigurationModel> map(IEnumerable<ServerConfiguration> serverConfigurations)
		{
			const string frameAncestors = "FrameAncestors";
			const string instrumentationKey = "InstrumentationKey";
			const string asDatabase = "AS_DATABASE";
			const string asServerName = "AS_SERVER_NAME";
			const string pmInstall = "PM_INSTALL";
			const string payrollSourcePath = "PayrollSourcePath";
			

			return serverConfigurations.Select(serverConfiguration =>
			{
				var configurationModel = new ConfigurationModel
				{
					Key = serverConfiguration.Key,
					Value = serverConfiguration.Value
				};
				if (configurationModel.Key == frameAncestors)
				{
					configurationModel.Description = "Add urls to let mytime or ASM widget work in an iframe.";
				}
				if (configurationModel.Key == instrumentationKey)
				{
					configurationModel.Description = "Add Application Insights instrumentation key for log analytics.";
				}
				if (configurationModel.Key == asDatabase)
				{
					configurationModel.Description = "Analysis database for PM service.";
				}
				if (configurationModel.Key == asServerName)
				{
					configurationModel.Description = "Analysis database server for PM service.";
				}
				if (configurationModel.Key == pmInstall)
				{
					configurationModel.Description = "Indicate whether PM service is installed.";
				}
				if (configurationModel.Key == logonAttempsDays)
				{
					configurationModel.Description = "How many days the logon attempts are saved.";
				}
				if (configurationModel.Key == payrollSourcePath)
				{
					configurationModel.Description = @"OBS! This is not used in an Azure installation. The path to where the payroll files are placed. Leave blank if you want to use the the default path Payroll.DeployNew\[TENANTNAME] under the Service Bus.";
				}
				return configurationModel;
			});
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