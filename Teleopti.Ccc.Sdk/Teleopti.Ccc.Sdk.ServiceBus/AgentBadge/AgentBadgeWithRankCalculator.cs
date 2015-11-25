using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class AgentBadgeWithRankCalculator : IAgentBadgeWithRankCalculator
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AgentBadgeWithRankCalculator));
		private readonly IStatisticRepository _statisticRepository;
		private readonly IAgentBadgeWithRankTransactionRepository _transactionRepository;
		private readonly IDefinedRaptorApplicationFunctionFactory _appFunctionFactory;
		private readonly IPersonRepository _personRepository;
		private readonly IScheduleRepository _scheduleRepository;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly INow _now;

		public AgentBadgeWithRankCalculator(IStatisticRepository statisticRepository,
			IAgentBadgeWithRankTransactionRepository transactionRepository,
			IDefinedRaptorApplicationFunctionFactory appFunctionFactory,
			IPersonRepository personRepository,
			IScheduleRepository scheduleRepository,
			IScenarioRepository scenarioRepository,
			INow now)
		{
			_statisticRepository = statisticRepository;
			_transactionRepository = transactionRepository;
			_appFunctionFactory = appFunctionFactory;
			_personRepository = personRepository;
			_scheduleRepository = scheduleRepository;
			_scenarioRepository = scenarioRepository;
			_now = now;
		}

		protected IList<IAgentBadgeWithRankTransaction> AddBadge<T>(HashSet<IPerson> allPersons,
			IDictionary<Guid, T> agentsListShouldGetBadge, BadgeType badgeType,
			T bronzeBadgeThreshold, T silverBadgeThreshold, T goldBadgeThreshold,
			bool largerIsBetter, DateOnly date) where T : IComparable
		{
			var newAwardedBadges = new List<IAgentBadgeWithRankTransaction>();
			if (!agentsListShouldGetBadge.Any())
			{
				return newAwardedBadges;
			}

			var viewBadgeFunc =
				_appFunctionFactory.ApplicationFunctionList.SingleOrDefault(
					x => x.ForeignId == DefinedRaptorApplicationFunctionForeignIds.ViewBadge);
			if (viewBadgeFunc == null)
			{
				return newAwardedBadges;
			}

			var agentsShouldGetBadge = agentsListShouldGetBadge.Select(
				agent => allPersons.SingleOrDefault(x => x.Id != null && x.Id.Value == agent.Key))
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

				var badge = _transactionRepository.Find(person, badgeType, date);

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
					InsertedOn = _now.UtcDateTime()
				};

				_transactionRepository.Add(newBadge);
				newAwardedBadges.Add(newBadge);
			}

			return newAwardedBadges;
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
			var agentAdherenceList =
				_statisticRepository.LoadAgentsOverThresholdForAdherence(adherenceCalculationMethod, timezoneCode, date.Date,
					setting.AdherenceBronzeThreshold, businessUnitId);

			if (agentAdherenceList.Count > 0)
			{
				var agentsWithAdherence = agentAdherenceList.Cast<object[]>()
					.ToDictionary(data => (Guid)data[0], data => double.Parse(data[2].ToString()));
				//var personIdList = (from object[] data in agentAdherenceList select (Guid)data[0]).ToList();
				var personIdList = agentsWithAdherence.Keys;
				var personsList = _personRepository.FindPeople(personIdList);
				var schedules = _scheduleRepository.FindSchedulesForPersonsOnlyInGivenPeriod(personsList,
					new ScheduleDictionaryLoadOptions(true, false),
					new DateOnlyPeriod(date, date),
					_scenarioRepository.LoadDefaultScenario());

				var agentsWithAdherenceShouldGetBadge = new Dictionary<Guid, double>();
				foreach (var personWithAdherence in agentsWithAdherence)
				{
					var personId = personWithAdherence.Key;
					var adherence = personWithAdherence.Value;
					if (personList.All(x => x.Id != personId))
					{
						if (logger.IsDebugEnabled)
						{
							logger.DebugFormat(
								"Agent with ID \"{0}\" could not be found in people list, no badge will be awarded.",
								personId);
						}
						continue;
					}
					var person = personsList.First(x => x.Id == personId);
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
			var agentsList = _statisticRepository.LoadAgentsUnderThresholdForAHT(timezoneCode, date.Date, setting.AHTBronzeThreshold, businessUnitId);

			if (agentsList.Count > 0)
			{
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("{0} agents will get badge for AHT", agentsList.Count);
				}

				var agentsWithAht = agentsList.Cast<object[]>()
					.ToDictionary(data => (Guid)data[0], data => double.Parse(data[2].ToString()));

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

		public virtual IEnumerable<IAgentBadgeWithRankTransaction> CalculateAnsweredCallsBadges(
			IEnumerable<IPerson> allPersons, string timezoneCode, DateOnly date, IGamificationSetting setting, Guid businessUnitId)
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
			var agentsList = _statisticRepository.LoadAgentsOverThresholdForAnsweredCalls(timezoneCode, date.Date,
				setting.AnsweredCallsBronzeThreshold, businessUnitId);

			if (agentsList.Count > 0)
			{
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("{0} agents will get badge for answered calls", agentsList.Count);
				}

				var agentsWithAdherence = agentsList.Cast<object[]>()
					.ToDictionary(data => (Guid)data[0], data => int.Parse(data[2].ToString()));

				var newAwardedAnsweredCallsBadges = AddBadge(personList, agentsWithAdherence, BadgeType.AnsweredCalls,
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
