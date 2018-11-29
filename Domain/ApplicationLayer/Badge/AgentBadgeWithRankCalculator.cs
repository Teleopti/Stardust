using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Badge
{
	public class AgentBadgeWithRankCalculator : IAgentBadgeWithRankCalculator
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AgentBadgeWithRankCalculator));
		private readonly IBadgeCalculationRepository _badgeCalculationRepository;
		private readonly IAgentBadgeWithRankTransactionRepository _transactionRepository;
		private readonly IDefinedRaptorApplicationFunctionFactory _appFunctionFactory;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly INow _now;
		private readonly IExternalPerformanceDataRepository _externalPerformanceDataRepository;

		public void ResetAgentBadges()
		{
			_transactionRepository.ResetAgentBadges();
		}

		public void RemoveAgentBadges(DateOnlyPeriod period)
		{
			_transactionRepository.Remove(period);
		}

		public AgentBadgeWithRankCalculator(IBadgeCalculationRepository badgeCalculationRepository,
			IAgentBadgeWithRankTransactionRepository transactionRepository,
			IDefinedRaptorApplicationFunctionFactory appFunctionFactory,
			IPersonRepository personRepository,
			IScheduleStorage scheduleStorage,
			IScenarioRepository scenarioRepository,
			INow now, IExternalPerformanceDataRepository externalPerformanceDataRepository)
		{
			_badgeCalculationRepository = badgeCalculationRepository;
			_transactionRepository = transactionRepository;
			_appFunctionFactory = appFunctionFactory;
			_personRepository = personRepository;
			_scheduleStorage = scheduleStorage;
			_scenarioRepository = scenarioRepository;
			_now = now;
			_externalPerformanceDataRepository = externalPerformanceDataRepository;
		}

		protected IList<IAgentBadgeWithRankTransaction> AddBadge<T>(HashSet<IPerson> allPersons,
			IDictionary<Guid, T> agentsListShouldGetBadge, int badgeType,
			T bronzeBadgeThreshold, T silverBadgeThreshold, T goldBadgeThreshold,
			bool largerIsBetter, DateOnly date, bool isExternal = false) where T : IComparable
		{
			var newAwardedBadges = new List<IAgentBadgeWithRankTransaction>();
			if (!agentsListShouldGetBadge.Any())
			{
				return newAwardedBadges;
			}

			var viewBadgeFunc =
				_appFunctionFactory.ApplicationFunctions.SingleOrDefault(
					x => x.ForeignId == DefinedRaptorApplicationFunctionForeignIds.ViewBadge);
			if (viewBadgeFunc == null)
			{
				return newAwardedBadges;
			}

			var agentsShouldGetBadge = agentsListShouldGetBadge.Select(
				agent => allPersons.SingleOrDefault(x => x.Id == agent.Key))
				.Where(agent => agent != null);
			foreach (var person in agentsShouldGetBadge)
			{
				var hasBadgePermission = person.PermissionInformation.ApplicationRoleCollection.Any(
					role => role.ApplicationFunctionCollection.Contains(viewBadgeFunc));

				if (!hasBadgePermission)
				{
					if (logger.IsDebugEnabled)
					{
						logger.DebugFormat("Agent {0} (ID: {1}) has no badge permission, no badge will be awarded.",
							person.Name, person.Id);
					}
					continue;
				}

				var badge = _transactionRepository.Find(person, badgeType, date, isExternal);

				if (badge != null)
				{
					if (logger.IsDebugEnabled)
					{
						logger.DebugFormat(
							"Agent {0} (ID: {1}) already get {2} badge for {3:yyyy-MM-dd}, no duplicate badge will awarded.",
							person.Name, person.Id, badgeType, date.Date);
					}
					continue;
				}

				var personId = person.Id;
				if (personId == null) continue;

				var value = agentsListShouldGetBadge[(Guid)personId];
				var bronzeBadgeAmount = 0;
				var silverBadgeAmount = 0;
				var goldBadgeAmount = 0;
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat(
						"largerIsBetter: {0}, value: {1}, bronzeBadgeThreshold: {2}, silverBadgeThreshold: {3}, goldBadgeThreshold: {4}",
						largerIsBetter, value, bronzeBadgeThreshold, silverBadgeThreshold, goldBadgeThreshold);
				}
				if (largerIsBetter)
				{
					if (value.CompareTo(bronzeBadgeThreshold) >= 0 && value.CompareTo(silverBadgeThreshold) < 0)
					{
						//value >= bronzeBadgeThreshold && value < silverBadgeThreshold
						bronzeBadgeAmount = 1;
					}
					else if (value.CompareTo(silverBadgeThreshold) >= 0 && value.CompareTo(goldBadgeThreshold) < 0)
					{
						//value >= silverBadgeThreshold && value < goldBadgeThreshold
						silverBadgeAmount = 1;
					}
					else if (value.CompareTo(goldBadgeThreshold) >= 0)
					{
						//value >= goldBadgeThreshold
						goldBadgeAmount = 1;
					}
				}
				else
				{
					if (value.CompareTo(bronzeBadgeThreshold) <= 0 && value.CompareTo(silverBadgeThreshold) > 0)
					{
						//value <= bronzeBadgeThreshold && value > silverBadgeThreshold
						bronzeBadgeAmount = 1;
					}
					else if (value.CompareTo(silverBadgeThreshold) <= 0 && value.CompareTo(goldBadgeThreshold) > 0)
					{
						//value <= silverBadgeThreshold && value > goldBadgeThreshold
						silverBadgeAmount = 1;
					}
					else if (value.CompareTo(goldBadgeThreshold) <= 0)
					{
						//value <= goldBadgeThreshold
						goldBadgeAmount = 1;
					}
				}

				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("Award {0} badge to agent {1} (ID: {2}), "
						+ "Bronze badge count: {3}, Silver badge count: {4}, Gold badge count: {5}.",
						badgeType, person.Name, personId, bronzeBadgeAmount, silverBadgeAmount, goldBadgeAmount);
				}

				var newBadge = new AgentBadgeWithRankTransaction
				{
					Person = person,
					BronzeBadgeAmount = bronzeBadgeAmount,
					SilverBadgeAmount = silverBadgeAmount,
					GoldBadgeAmount = goldBadgeAmount,
					BadgeType = badgeType,
					CalculatedDate = date,
					Description = "",
					InsertedOn = _now.UtcDateTime(),
					IsExternal = isExternal
				};

				_transactionRepository.Add(newBadge);
				newAwardedBadges.Add(newBadge);
			}

			return newAwardedBadges;
		}

		public IEnumerable<IAgentBadgeWithRankTransaction> CalculateBadges(IEnumerable<IPerson> allPersons, DateOnly date, IBadgeSetting badgeSetting, Guid businessId)
		{
			var newBadges = new List<IAgentBadgeWithRankTransaction>();
			var personList = new HashSet<IPerson>(allPersons);

			var personIdList = allPersons.Select(x => x.Id.Value).ToList();
			var performanceDatas = _externalPerformanceDataRepository.FindPersonsCouldGetBadgeOverThreshold(date, personIdList, badgeSetting.QualityId, badgeSetting.BronzeThreshold, businessId);

			var agentsWithBadgeValue = performanceDatas.ToDictionary(k => k.PersonId, v => v.Score);
			var newAwardedBadges = AddBadge(personList, agentsWithBadgeValue, badgeSetting.QualityId,
				badgeSetting.BronzeThreshold, badgeSetting.SilverThreshold, badgeSetting.GoldThreshold, badgeSetting.LargerIsBetter, date, true);
			newBadges.AddRange(newAwardedBadges);

			return newBadges;
		}

		public IEnumerable<IAgentBadgeWithRankTransaction> CalculateAdherenceBadges(IEnumerable<IPerson> allPersons,
			string timezoneCode, DateOnly date, AdherenceReportSettingCalculationMethod adherenceCalculationMethod,
			IGamificationSetting setting, Guid businessUnitId)
		{
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat(
					"Calculate adherence badges for timezone: {0}, date: {1:yyyy-MM-dd HH:mm:ss}, AdherenceReportSettingCalculationMethod: {2},"
					+ "bronze badge threshold: {3}, silver badge threshold: {4}, gold badge threshold: {5}", timezoneCode, date.Date,
					adherenceCalculationMethod, setting.AdherenceBronzeThreshold, setting.AdherenceSilverThreshold,
					setting.AdherenceGoldThreshold);
			}

			var personList = new HashSet<IPerson>(allPersons);
			var newAwardedBadges = new List<IAgentBadgeWithRankTransaction>();
			var agentsWithAdherence =
				_badgeCalculationRepository.LoadAgentsOverThresholdForAdherence(adherenceCalculationMethod, timezoneCode, date.Date,
					setting.AdherenceBronzeThreshold, businessUnitId);

			if (agentsWithAdherence.Count > 0)
			{
				var personIdList = agentsWithAdherence.Keys;
				var personsList = _personRepository.FindPeople(personIdList).ToDictionary(p => p.Id.GetValueOrDefault());
				var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(personsList.Values,
					new ScheduleDictionaryLoadOptions(true, false),
					new DateOnlyPeriod(date, date),
					_scenarioRepository.LoadDefaultScenario());

				var agentsWithAdherenceShouldGetBadge = new Dictionary<Guid, double>();
				foreach (var personWithAdherence in agentsWithAdherence)
				{
					var personId = personWithAdherence.Key;
					var adherence = personWithAdherence.Value;
					IPerson person;
					if (!personsList.TryGetValue(personId,out person))
					{
						if (logger.IsDebugEnabled)
						{
							logger.DebugFormat(
								"Agent with ID \"{0}\" could not be found in people list, no badge will be awarded.",
								personId);
						}
						continue;
					}

					var scheduleDay = schedules[person].ScheduledDay(date);
					var isFullDayAbsence = scheduleDay.SignificantPartForDisplay() == SchedulePartView.FullDayAbsence;

					// Agent scheduled full day absence should not get badge (Refer to bug #32388)
					if (isFullDayAbsence)
					{
						if (logger.IsDebugEnabled)
						{
							logger.DebugFormat(
								"Agent {0} (ID: {1}) scheduled full day absence for {2:yyyy-MM-dd} and will not get badge.",
								person.Name, person.Id, date.Date);
						}
						continue;
					}

					agentsWithAdherenceShouldGetBadge[personId] = adherence;
				}

				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("{0} agents will get badge for adherence", agentsWithAdherenceShouldGetBadge.Count);
				}

				var newAwardedAdherenceBadge = AddBadge(personList, agentsWithAdherenceShouldGetBadge, BadgeType.Adherence,
					setting.AdherenceBronzeThreshold.Value,
					setting.AdherenceSilverThreshold.Value, setting.AdherenceGoldThreshold.Value, true, date);
				newAwardedBadges.AddRange(newAwardedAdherenceBadge);
			}
			else
			{
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("No agents will get badge for adherence");
				}
			}

			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Total {0} new badge(s) been awarded to agents for adherence.", newAwardedBadges.Count);
			}

			return newAwardedBadges;
		}

		public virtual IEnumerable<IAgentBadgeWithRankTransaction> CalculateAHTBadges(IEnumerable<IPerson> allPersons,
			string timezoneCode, DateOnly date, IGamificationSetting setting, Guid businessUnitId)
		{
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat(
					"Calculate AHT badges for timezone: {0}, date: {1:yyyy-MM-dd HH:mm:ss},"
					+ "bronze badge threshold: {2}, silver badge threshold: {3}, gold badge threshold: {4}", timezoneCode, date.Date,
					setting.AHTBronzeThreshold, setting.AHTSilverThreshold, setting.AHTGoldThreshold);
			}

			var personList = new HashSet<IPerson>(allPersons);
			var newAwardedBadges = new List<IAgentBadgeWithRankTransaction>();
			var agentsWithAht = _badgeCalculationRepository.LoadAgentsUnderThresholdForAht(timezoneCode, date.Date, setting.AHTBronzeThreshold, businessUnitId);

			if (agentsWithAht.Count > 0)
			{
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("{0} agents will get badge for AHT", agentsWithAht.Count);
				}

				var newAwardedAhtBadges = AddBadge(personList, agentsWithAht, BadgeType.AverageHandlingTime,
					setting.AHTBronzeThreshold.TotalSeconds,
					setting.AHTSilverThreshold.TotalSeconds, setting.AHTGoldThreshold.TotalSeconds, false, date);
				newAwardedBadges.AddRange(newAwardedAhtBadges);
			}
			else
			{
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("No agents will get badge for AHT");
				}
			}
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Total {0} new badge(s) been awarded to agents for AHT.", newAwardedBadges.Count);
			}

			return newAwardedBadges;
		}

		public virtual IEnumerable<IAgentBadgeWithRankTransaction> CalculateAnsweredCallsBadges(IEnumerable<IPerson> allPersons,
			string timezoneCode, DateOnly date, IGamificationSetting setting, Guid businessUnitId)
		{
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat(
					"Calculate answered calls badges for timezone: {0}, date: {1:yyyy-MM-dd HH:mm:ss},"
					+ "bronze badge threshold: {2}, silver badge threshold: {3}, gold badge threshold: {4}", timezoneCode, date.Date,
					setting.AnsweredCallsBronzeThreshold, setting.AnsweredCallsSilverThreshold, setting.AnsweredCallsGoldThreshold);
			}

			var personList = new HashSet<IPerson>(allPersons);
			var newAwardedBadges = new List<IAgentBadgeWithRankTransaction>();
			var agentsWithAnsweredCalls = _badgeCalculationRepository.LoadAgentsOverThresholdForAnsweredCalls(timezoneCode, date.Date,
				setting.AnsweredCallsBronzeThreshold, businessUnitId);

			if (agentsWithAnsweredCalls.Any())
			{
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("{0} agents will get badge for answered calls", agentsWithAnsweredCalls.Count);
				}

				var newAwardedAnsweredCallsBadges = AddBadge(personList, agentsWithAnsweredCalls, BadgeType.AnsweredCalls,
					setting.AnsweredCallsBronzeThreshold,
					setting.AnsweredCallsSilverThreshold, setting.AnsweredCallsGoldThreshold, true, date);
				newAwardedBadges.AddRange(newAwardedAnsweredCallsBadges);
			}
			else
			{
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("No agents will get badge for answered calls");
				}
			}

			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat("Total {0} new badge(s) been awarded to agents for answered calls.", newAwardedBadges.Count);
			}

			return newAwardedBadges;
		}
	}
}