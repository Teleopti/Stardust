using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider
{
	public class BadgeSettingProvider : IBadgeSettingProvider
	{
		private readonly IAgentBadgeSettingsRepository _repository;

		public BadgeSettingProvider(IAgentBadgeSettingsRepository repository)
		{
			_repository = repository;
		}

		public IAgentBadgeThresholdSettings GetBadgeSettings()
		{
			var result = _repository.LoadAll().FirstOrDefault();
			result = result ?? new AgentBadgeThresholdSettings()
			{
				EnableBadge = false,
			};

			return result;
		}
	}
}