using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Gamification.Mapping;
using Teleopti.Ccc.Web.Areas.Gamification.Models;

namespace Teleopti.Ccc.Web.Areas.Gamification.Core.DataProvider
{
	public class GamificationSettingPersister : IGamificationSettingPersister
	{
		private readonly IGamificationSettingRepository _gamificationSettingRepository;
		private readonly IGamificationSettingMapper _mapper;

		public GamificationSettingPersister(IGamificationSettingRepository gamificationSettingRepository, IGamificationSettingMapper mapper)
		{
			_gamificationSettingRepository = gamificationSettingRepository;
			_mapper = mapper;
		}

		public GamificationSettingViewModel Persist()
		{
			var gamificationSetting = getGamificationSetting(null);
			if (!gamificationSetting.Id.HasValue)
			{
				_gamificationSettingRepository.Add(gamificationSetting);
			}

			return _mapper.Map(gamificationSetting);
		}

		public bool RemoveGamificationSetting(Guid id)
		{
			var gamificationSetting = getGamificationSetting(id);
			if (gamificationSetting == null) return false;

			_gamificationSettingRepository.Remove(gamificationSetting);
			return true;
		}

		public bool ResetGamificationSetting()
		{
			try
			{
				using (var myUow = UnitOfWorkFactory.Current.CreateAndOpenUnitOfWork())
				{
					var agentBadgeTransactionRepository = new AgentBadgeTransactionRepository(myUow);
					var agentBadgeWithRankTransactionRepository = new AgentBadgeWithRankTransactionRepository(myUow);
					agentBadgeTransactionRepository.ResetAgentBadges();
					agentBadgeWithRankTransactionRepository.ResetAgentBadges();
					myUow.PersistAll();
				}
			}
			catch (Exception)
			{
				return false;
			}

			return true;
		}

		public GamificationDescriptionViewModel PersistDescription(GamificationDescriptionViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.Description != input.Value) gamificationSetting.Description = input.Value;

			return new GamificationDescriptionViewModel(){GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.Description};
		}

		public GamificationThresholdEnabledViewModel PersistAnsweredCallsEnabled(GamificationThresholdEnabledViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AnsweredCallsBadgeEnabled != input.Value) gamificationSetting.AnsweredCallsBadgeEnabled = input.Value;

			return new GamificationThresholdEnabledViewModel(){GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AnsweredCallsBadgeEnabled};
		}

		public GamificationAnsweredCallsThresholdViewModel PersistAnsweredCallsGoldThreshold(GamificationAnsweredCallsThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AnsweredCallsGoldThreshold != input.Value) gamificationSetting.AnsweredCallsGoldThreshold = input.Value;

			return new GamificationAnsweredCallsThresholdViewModel(){GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AnsweredCallsGoldThreshold};
		}

		public GamificationAnsweredCallsThresholdViewModel PersistAnsweredCallsSilverThreshold(GamificationAnsweredCallsThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AnsweredCallsSilverThreshold != input.Value) gamificationSetting.AnsweredCallsSilverThreshold = input.Value;

			return new GamificationAnsweredCallsThresholdViewModel(){GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AnsweredCallsSilverThreshold};
		}

		public GamificationAnsweredCallsThresholdViewModel PersistAnsweredCallsBronzeThreshold(GamificationAnsweredCallsThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AnsweredCallsBronzeThreshold != input.Value) gamificationSetting.AnsweredCallsBronzeThreshold = input.Value;

			return new GamificationAnsweredCallsThresholdViewModel(){GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AnsweredCallsBronzeThreshold};
		}

		public GamificationAnsweredCallsThresholdViewModel PersistAnsweredCallsThreshold(GamificationAnsweredCallsThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AnsweredCallsThreshold != input.Value) gamificationSetting.AnsweredCallsThreshold = input.Value;

			return new GamificationAnsweredCallsThresholdViewModel(){GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AnsweredCallsThreshold};
		}

		public GamificationThresholdEnabledViewModel PersistAHTEnabled(GamificationThresholdEnabledViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AHTBadgeEnabled != input.Value) gamificationSetting.AHTBadgeEnabled = input.Value;

			return new GamificationThresholdEnabledViewModel() { GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AHTBadgeEnabled };
		}

		public GamificationAHTThresholdViewModel PersistAHTGoldThreshold(GamificationAHTThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AHTGoldThreshold != input.Value) gamificationSetting.AHTGoldThreshold = input.Value;

			return new GamificationAHTThresholdViewModel() { GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AHTGoldThreshold };
		}

		public GamificationAHTThresholdViewModel PersistAHTSilverThreshold(GamificationAHTThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AHTSilverThreshold != input.Value) gamificationSetting.AHTSilverThreshold = input.Value;

			return new GamificationAHTThresholdViewModel() { GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AHTSilverThreshold };
		}

		public GamificationAHTThresholdViewModel PersistAHTBronzeThreshold(GamificationAHTThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AHTBronzeThreshold != input.Value) gamificationSetting.AHTBronzeThreshold = input.Value;

			return new GamificationAHTThresholdViewModel() { GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AHTBronzeThreshold };
		}

		public GamificationAHTThresholdViewModel PersistAHTThreshold(GamificationAHTThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AHTThreshold != input.Value) gamificationSetting.AHTThreshold = input.Value;

			return new GamificationAHTThresholdViewModel() { GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AHTThreshold };
		}

		public GamificationThresholdEnabledViewModel PersistAdherenceEnabled(GamificationThresholdEnabledViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AdherenceBadgeEnabled != input.Value) gamificationSetting.AdherenceBadgeEnabled = input.Value;

			return new GamificationThresholdEnabledViewModel() { GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AdherenceBadgeEnabled };
		}

		public GamificationAdherenceThresholdViewModel PersistAdherenceThreshold(GamificationAdherenceThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AdherenceThreshold != input.Value) gamificationSetting.AdherenceThreshold = input.Value;

			return new GamificationAdherenceThresholdViewModel() { GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AdherenceThreshold };
		}

		public GamificationAdherenceThresholdViewModel PersistAdherenceGoldThreshold(GamificationAdherenceThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AdherenceGoldThreshold != input.Value) gamificationSetting.AdherenceGoldThreshold = input.Value;

			return new GamificationAdherenceThresholdViewModel() { GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AdherenceGoldThreshold };
		}

		public GamificationAdherenceThresholdViewModel PersistAdherenceSilverThreshold(GamificationAdherenceThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AdherenceSilverThreshold != input.Value) gamificationSetting.AdherenceSilverThreshold = input.Value;

			return new GamificationAdherenceThresholdViewModel() { GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AdherenceSilverThreshold };
		}

		public GamificationAdherenceThresholdViewModel PersistAdherenceBronzeThreshold(GamificationAdherenceThresholdViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.AdherenceBronzeThreshold != input.Value) gamificationSetting.AdherenceBronzeThreshold = input.Value;

			return new GamificationAdherenceThresholdViewModel() { GamificationSettingId = gamificationSetting.Id.Value, Value = gamificationSetting.AdherenceBronzeThreshold};
		}

		public GamificationSettingViewModel PersistRuleChange(GamificationChangeRuleForm input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			gamificationSetting.GamificationSettingRuleSet = input.Rule;
			return _mapper.Map(gamificationSetting);
		}

		public GamificationBadgeConversRateViewModel PersistGoldToSilverRate(GamificationBadgeConversRateViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.GoldToSilverBadgeRate != input.Rate) gamificationSetting.GoldToSilverBadgeRate = input.Rate;

			return new GamificationBadgeConversRateViewModel() { GamificationSettingId = gamificationSetting.Id.Value, Rate = gamificationSetting.GoldToSilverBadgeRate };
		}

		public GamificationBadgeConversRateViewModel PersistSilverToBronzeRate(GamificationBadgeConversRateViewModel input)
		{
			var gamificationSetting = getGamificationSetting(input.GamificationSettingId);
			if (gamificationSetting == null) return null;
			if (gamificationSetting.SilverToBronzeBadgeRate != input.Rate) gamificationSetting.SilverToBronzeBadgeRate = input.Rate;

			return new GamificationBadgeConversRateViewModel() { GamificationSettingId = gamificationSetting.Id.Value, Rate = gamificationSetting.SilverToBronzeBadgeRate };
		}

		public ExternalBadgeSettingViewModel PersistExternalBadgeSetting(UpdateExternalBadgeSettingViewModel input)
		{
			var setting = getGamificationSetting(input.Id);
			if (setting == null) return null;

			if (setting.ExternalBadgeSettings == null)
			{
				setting.ExternalBadgeSettings = new List<IExternalBadgeSetting>();
			}

			var externalBadgeSetting = setting.ExternalBadgeSettings.FirstOrDefault(x => x.QualityId == input.QualityId);
			if (externalBadgeSetting == null)
			{
				externalBadgeSetting = new ExternalBadgeSetting
				{
					Name = input.Name,
					QualityId = input.QualityId,
					LargerIsBetter = input.LargerIsBetter, // TODO: Should get from quality_info
					Enabled = input.Enabled,
					Threshold = input.Threshold,
					BronzeThreshold = input.BronzeThreshold,
					SilverThreshold = input.SilverThreshold,
					GoldThreshold = input.GoldThreshold,
					UnitType = input.UnitType // TODO: Should get from quality_info
				};

				setting.AddExternalBadgeSetting(externalBadgeSetting);
			}
			else
			{
				externalBadgeSetting.Name = input.Name;
				externalBadgeSetting.Enabled = input.Enabled;
				externalBadgeSetting.Threshold = input.Threshold;
				externalBadgeSetting.BronzeThreshold = input.BronzeThreshold;
				externalBadgeSetting.SilverThreshold = input.SilverThreshold;
				externalBadgeSetting.GoldThreshold = input.GoldThreshold;
			}

			return new ExternalBadgeSettingViewModel
			{
				Id = externalBadgeSetting.Id ?? Guid.Empty, // How to get Id of the new created badge setting?
				Name = externalBadgeSetting.Name,
				QualityId = externalBadgeSetting.QualityId,
				LargerIsBetter = externalBadgeSetting.LargerIsBetter,
				Enabled = externalBadgeSetting.Enabled,
				Threshold = externalBadgeSetting.Threshold,
				BronzeThreshold = externalBadgeSetting.BronzeThreshold,
				SilverThreshold = externalBadgeSetting.SilverThreshold,
				GoldThreshold = externalBadgeSetting.GoldThreshold,
				UnitType = externalBadgeSetting.UnitType
			};
		}

		private IGamificationSetting getGamificationSetting(Guid? id)
		{
			return id == null ? new GamificationSetting(getNewName()) : _gamificationSettingRepository.Get((Guid)id);
		}

		private string getNewName()
		{
			var currentCount = _gamificationSettingRepository.LoadAll().Count;
			return currentCount > 0 ? string.Format(Resources.NewGamificationSetting + "{0}", currentCount) : Resources.NewGamificationSetting;
		}
	}
}