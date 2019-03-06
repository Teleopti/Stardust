using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Badge
{
	public class CalculateBadges
	{
		private readonly ITeamGamificationSettingRepository _teamSettingsRepository;
		private readonly IAgentBadgeCalculator _calculator;
		private readonly IAgentBadgeWithRankCalculator _badgeWithRankCalculator;
		private static readonly ILog logger = LogManager.GetLogger(typeof(CalculateBadges));
		private readonly IGlobalSettingDataRepository _globalSettingRep;
		private readonly IPersonRepository _personRepository;
		private readonly IPushMessageSender _pushMessageSender;
		private readonly IPurgeSettingRepository _purgeSettingRepository;

		public CalculateBadges(
			ITeamGamificationSettingRepository teamSettingsRepository,
			IAgentBadgeCalculator calculator,
			IAgentBadgeWithRankCalculator badgeWithRankCalculator,
			IGlobalSettingDataRepository globalSettingRep,
			IPersonRepository personRepository, IPushMessageSender pushMessageSender, IPurgeSettingRepository purgeSettingRepository)
		{
			_teamSettingsRepository = teamSettingsRepository;
			_calculator = calculator;
			_badgeWithRankCalculator = badgeWithRankCalculator;
			_globalSettingRep = globalSettingRep;
			_personRepository = personRepository;
			_pushMessageSender = pushMessageSender;
			_purgeSettingRepository = purgeSettingRepository;
		}

		public bool ResetBadge()
		{
			try
			{
				_badgeWithRankCalculator.ResetAgentBadges();
				_calculator.ResetAgentBadges();
			}
			catch (Exception e)
			{
				logger.Error(e);
				return false;
			}

			return true;
		}

		public void RemoveAgentBadges(DateOnlyPeriod period)
		{
			_badgeWithRankCalculator.RemoveAgentBadges(period);
			_calculator.RemoveAgentBadges(period);
		}

		public void Calculate(CalculateBadgeMessage message)
		{
			var teamSettings = _teamSettingsRepository.FindAllTeamGamificationSettingsSortedByTeam().Where(t => t.GamificationSetting != null).ToLookup(t => t.GamificationSetting);
			var settings = teamSettings.Select(t => t.Key);
			var calculateDate = new DateOnly(message.CalculationDate);

			foreach (var setting in settings)
			{
				if (setting.IsDeleted)
				{
					continue;
				}

				var agentsWithSetting = new List<IPerson>();
				foreach (var teamSetting in teamSettings[setting])
				{
					agentsWithSetting.AddRange(_personRepository.FindPeopleBelongTeam(teamSetting.Team, calculateDate.ToDateOnlyPeriod().Inflate(1)));
				}
				agentsWithSetting = agentsWithSetting.Distinct().ToList();

				var isRuleWithDifferentThreshold =
					setting.GamificationSettingRuleSet == GamificationSettingRuleSet.RuleWithDifferentThreshold;

				calculateAdherenceBadge(message, setting, isRuleWithDifferentThreshold, agentsWithSetting, calculateDate);
				calculateAhtBadge(message, setting, isRuleWithDifferentThreshold, agentsWithSetting, calculateDate);
				calculateAnsweredCallsBadge(message, setting, isRuleWithDifferentThreshold, agentsWithSetting, calculateDate);
			}
		}

		public int GetExternalPerformanceDataPurgeDays()
		{
			var setting = _purgeSettingRepository.FindAllPurgeSettings()
				.FirstOrDefault(s => s.Key == "DaysToKeepExternalPerformanceData");
			return setting?.Value ?? 30;
		}

		public void CalculateExternalBadge(CalculateBadgeMessage message)
		{
			var teamSettings = _teamSettingsRepository.FindAllTeamGamificationSettingsSortedByTeam().Where(t => t.GamificationSetting != null).ToLookup(t => t.GamificationSetting);
			var settings = teamSettings.Select(t => t.Key);
			var calculateDate = new DateOnly(message.CalculationDate);

			foreach (var setting in settings)
			{
				if (setting.IsDeleted)
				{
					continue;
				}

				var agentsWithSetting = new List<IPerson>();
				foreach (var teamSetting in teamSettings[setting])
				{
					agentsWithSetting.AddRange(_personRepository.FindPeopleBelongTeam(teamSetting.Team, calculateDate.ToDateOnlyPeriod().Inflate(1)));
				}
				agentsWithSetting = agentsWithSetting.Distinct().ToList();

				calculateBadges(setting, agentsWithSetting, calculateDate, message.LogOnBusinessUnitId);
			}
		}

		private void calculateBadges(IGamificationSetting setting, IList<IPerson> agentsWithSetting, DateOnly calculateDate, Guid businessId)
		{
			foreach (var badgeSetting in setting.BadgeSettings)
			{
				if (!badgeSetting.Enabled) continue;
				if (setting.GamificationSettingRuleSet == GamificationSettingRuleSet.RuleWithDifferentThreshold)
				{
					var agentBadgeWithRank = _badgeWithRankCalculator.CalculateBadges(agentsWithSetting, calculateDate, badgeSetting, businessId);
					_pushMessageSender.SendMessage(agentBadgeWithRank, badgeSetting, calculateDate);
				}
				else
				{
					var agentBadge = _calculator.CalculateBadges(agentsWithSetting, calculateDate, badgeSetting, businessId);
					_pushMessageSender.SendMessage(agentBadge, badgeSetting, calculateDate, setting);
				}
			}
		}

		private void calculateAdherenceBadge(CalculateBadgeMessage message, IGamificationSetting setting,
			bool isRuleWithDifferentThreshold, IEnumerable<IPerson> agentsWithSetting, DateOnly calculateDate)
		{
			if (!setting.AdherenceBadgeEnabled) return;

			var adherenceReportSetting = _globalSettingRep.FindValueByKey(AdherenceReportSetting.Key,
				new AdherenceReportSetting());

			if (isRuleWithDifferentThreshold)
			{
				var newAwardedBadgesWithRankForAdherence =
					_badgeWithRankCalculator.CalculateAdherenceBadges(agentsWithSetting, message.TimeZoneCode, calculateDate,
						adherenceReportSetting.CalculationMethod, setting, message.LogOnBusinessUnitId).ToList();
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("Total {0} agents will get new badge for adherence",
						newAwardedBadgesWithRankForAdherence.Count);
				}
				_pushMessageSender.SendMessagesToPeopleGotABadge(newAwardedBadgesWithRankForAdherence, setting, calculateDate, BadgeType.Adherence);
			}
			else
			{
				var newAwardedBadgesForAdherence =
					_calculator.CalculateAdherenceBadges(agentsWithSetting, message.TimeZoneCode, calculateDate,
						adherenceReportSetting.CalculationMethod, setting, message.LogOnBusinessUnitId).ToList();
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("Total {0} agents will get new badge for adherence", newAwardedBadgesForAdherence.Count);
				}
				_pushMessageSender.SendMessagesToPeopleGotABadge(newAwardedBadgesForAdherence, setting, calculateDate, BadgeType.Adherence);
			}
		}

		private void calculateAhtBadge(CalculateBadgeMessage message, IGamificationSetting setting,
			bool isRuleWithDifferentThreshold, IEnumerable<IPerson> agentsWithSetting, DateOnly calculateDate)
		{
			if (!setting.AHTBadgeEnabled) return;

			if (isRuleWithDifferentThreshold)
			{
				var newAwardedBadgesWithRankForAht =
					_badgeWithRankCalculator.CalculateAHTBadges(agentsWithSetting, message.TimeZoneCode, calculateDate, setting,
						message.LogOnBusinessUnitId).ToList();
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("Total {0} agents will get new badge for AHT", newAwardedBadgesWithRankForAht.Count);
				}
				_pushMessageSender.SendMessagesToPeopleGotABadge(newAwardedBadgesWithRankForAht, setting, calculateDate,
					BadgeType.AverageHandlingTime);
			}
			else
			{
				var newAwardedBadgesForAht =
					_calculator.CalculateAHTBadges(agentsWithSetting, message.TimeZoneCode, calculateDate, setting,
						message.LogOnBusinessUnitId).ToList();
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("Total {0} agents will get new badge for AHT", newAwardedBadgesForAht.Count);
				}
				_pushMessageSender.SendMessagesToPeopleGotABadge(newAwardedBadgesForAht, setting, calculateDate, BadgeType.AverageHandlingTime);
			}
		}

		private void calculateAnsweredCallsBadge(CalculateBadgeMessage message, IGamificationSetting setting,
			bool isRuleWithDifferentThreshold, IEnumerable<IPerson> agentsWithSetting, DateOnly calculateDate)
		{
			if (!setting.AnsweredCallsBadgeEnabled) return;

			if (isRuleWithDifferentThreshold)
			{
				var newAwardedBadgesWithRankForAnsweredCalls =
					_badgeWithRankCalculator.CalculateAnsweredCallsBadges(agentsWithSetting, message.TimeZoneCode, calculateDate,
						setting, message.LogOnBusinessUnitId).ToList();
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("Total {0} agents will get new badge for answered calls",
						newAwardedBadgesWithRankForAnsweredCalls.Count);
				}
				_pushMessageSender.SendMessagesToPeopleGotABadge(newAwardedBadgesWithRankForAnsweredCalls, setting, calculateDate,
					BadgeType.AnsweredCalls);
			}
			else
			{
				var newAwardedBadgesForAnsweredCalls =
					_calculator.CalculateAnsweredCallsBadges(agentsWithSetting, message.TimeZoneCode,
						calculateDate, setting, message.LogOnBusinessUnitId).ToList();
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("Total {0} agents will get new badge for answered calls",
						newAwardedBadgesForAnsweredCalls.Count);
				}
				_pushMessageSender.SendMessagesToPeopleGotABadge(newAwardedBadgesForAnsweredCalls, setting, calculateDate, BadgeType.AnsweredCalls);
			}
		}
	}
}