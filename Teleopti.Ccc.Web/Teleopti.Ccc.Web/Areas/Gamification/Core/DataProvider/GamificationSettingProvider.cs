using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Web.Areas.Gamification.Mapping;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider
{
	public class GamificationSettingProvider : IGamificationSettingProvider
	{
		private readonly IGamificationSettingRepository _gamificationSettingRepository;
		private readonly IGamificationSettingMapper _mapper;
		private readonly IExternalPerformanceRepository _externalPerformanceRepository;

		public GamificationSettingProvider(IGamificationSettingRepository gamificationSettingRepository, IGamificationSettingMapper mapper, IExternalPerformanceRepository externalPerformanceRepository)
		{
			_gamificationSettingRepository = gamificationSettingRepository;
			_mapper = mapper;
			_externalPerformanceRepository = externalPerformanceRepository;
		}

		public GamificationSettingViewModel GetGamificationSetting(Guid id)
		{
			var setting = _gamificationSettingRepository.Get(id);

			if (setting == null)
				return default(GamificationSettingViewModel);

			var externalMeasures = _externalPerformanceRepository.FindAllExternalPerformances();

			var result = _mapper.Map(setting);

			foreach (var externalSetting in result.ExternalBadgeSettings)
			{
				var aMeasure = externalMeasures.FirstOrDefault(x => x.ExternalId == externalSetting.QualityId && x.DataType == externalSetting.DataType);
				if (aMeasure != null)
				{
					externalSetting.Name = aMeasure.Name;
				}
			}

			result.ExternalBadgeSettings = result.ExternalBadgeSettings.OrderBy(ebs => ebs.Name).ToList();
			return result;
		}

		public IList<GamificationDescriptionViewModel> GetGamificationList()
		{
			var settings = _gamificationSettingRepository.LoadAll();

			return settings.Select(setting => new GamificationDescriptionViewModel()
					{
						GamificationSettingId = setting.Id.Value,
						Value = setting.Description
					}).ToList();
		}
	}
}