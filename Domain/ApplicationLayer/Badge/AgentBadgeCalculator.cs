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
	public class AgentBadgeCalculator : IAgentBadgeCalculator
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AgentBadgeCalculator));
		private readonly IBadgeCalculationRepository _badgeCalculationRepository;
		private readonly IAgentBadgeTransactionRepository _transactionRepository;
		private readonly IDefinedRaptorApplicationFunctionFactory _appFunctionFactory;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
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

		public AgentBadgeCalculator(IBadgeCalculationRepository badgeCalculationRepository,
			IAgentBadgeTransactionRepository transactionRepository,
			IDefinedRaptorApplicationFunctionFactory appFunctionFactory,
			IPersonRepository personRepository,
			IScheduleStorage scheduleStorage,
			IScenarioRepository scenarioRepository,
			INow now, IExternalPerformanceDataRepository externalPerformanceDataRepository)
		{
			_badgeCalculationRepository = badgeCalculationRepository;
			_transactionRepository = transactionRepository;
			_appFunctionFactory = appFunctionFactory;
			_scheduleStorage = scheduleStorage;
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_now = now;
			_externalPerformanceDataRepository = externalPerformanceDataRepository;
		}

		protected IList<IAgentBadgeTransaction> AddBadge(IEnumerable<IPerson> allPersons, IEnumerable<Guid> agentIdsThatShouldGetBadge,
			int badgeType, int silverToBronzeBadgeRate, int goldToSilverBadgeRate, DateOnly date, bool isExternal = false)
		{
			var newAwardedBadges = new List<IAgentBadgeTransaction>();

			var agentIdListShouldGetBadge = agentIdsThatShouldGetBadge as IList<Guid>
				?? agentIdsThatShouldGetBadge.Where(a => a!=null).ToList();
			if (!agentIdListShouldGetBadge.Any())
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

			var persons = allPersons.Where(x => x.Id != null && agentIdListShouldGetBadge.Contains(x.Id.Value));
			foreach (var person in persons)
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

				if (badge == null)
				{
					if (logger.IsDebugEnabled)
					{
						logger.DebugFormat("Award {0} badge to agent {1} (ID: {2}).",
							badgeType, person.Name, person.Id);
					}

					var newBadge = new AgentBadgeTransaction
					{
						Person = person,
						Amount = 1,
						BadgeType = badgeType,
						CalculatedDate = date,
						Description = "",
						InsertedOn = _now.UtcDateTime(),
						IsExternal = isExternal
					};

					_transactionRepository.Add(newBadge);
					newAwardedBadges.Add(newBadge);
				}
				else
				{
					if (logger.IsDebugEnabled)
					{
						logger.DebugFormat(
							"Agent {0} (ID: {1}) already get {2} badge for {3:yyyy-MM-dd}, no duplicate badge will awarded.",
							person.Name, person.Id, badgeType, date.Date);
					}
				}
			}

			return newAwardedBadges;
		}

		public IEnumerable<IAgentBadgeTransaction> CalculateBadges(IEnumerable<IPerson> allPersons, DateOnly date, IBadgeSetting badgeSetting, Guid businessId)
		{
			var newBadges = new List<IAgentBadgeTransaction>();
			var personList = new HashSet<IPerson>(allPersons);

			var personIdList = allPersons.Select(x => x.Id.Value).ToList();
			var agentsWithBadgeData = _externalPerformanceDataRepository.FindPersonsCouldGetBadgeOverThreshold(
				date, personIdList, badgeSetting.QualityId, badgeSetting.Threshold, businessId);
			var agentsWithBadge = agentsWithBadgeData.Select(x => x.PersonId).ToList();

			var gamificationSetting = (IGamificationSetting) badgeSetting.Parent;
			var newAwardedBadges = AddBadge(personList, agentsWithBadge, badgeSetting.QualityId,
				gamificationSetting.SilverToBronzeBadgeRate, gamificationSetting.GoldToSilverBadgeRate, date, true);
			newBadges.AddRange(newAwardedBadges);

			return newBadges;
		}

		public IEnumerable<IAgentBadgeTransaction> CalculateAdherenceBadges(IEnumerable<IPerson> allPersons, string timezoneCode, DateOnly date,
			AdherenceReportSettingCalculationMethod adherenceCalculationMethod, IGamificationSetting setting, Guid businessUnitId)
		{
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat(
					"Calculate adherence badges for timezone: {0}, date: {1:yyyy-MM-dd HH:mm:ss}, AdherenceReportSettingCalculationMethod: {2},"
					+ "silver to bronze badge rate: {3}, gold to silver badge rate: {4}, adherenceThreshold: {5}", timezoneCode, date.Date,
					adherenceCalculationMethod, setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate, setting.AdherenceThreshold);
			}

			var personList = allPersons.ToList();
			var newAwardedBadges = new List<IAgentBadgeTransaction>();
			var agentAdherenceList =
				_badgeCalculationRepository.LoadAgentsOverThresholdForAdherence(adherenceCalculationMethod, timezoneCode, date.Date, setting.AdherenceThreshold, businessUnitId);

			if (agentAdherenceList.Any())
			{
				var personIdList = agentAdherenceList.Keys;
				var personsList = _personRepository.FindPeople(personIdList).ToDictionary(p => p.Id.GetValueOrDefault());
				var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(personsList.Values,
					new ScheduleDictionaryLoadOptions(true, false), new DateOnlyPeriod(date, date),
					_scenarioRepository.LoadDefaultScenario());

				var idListOfPersonShouldGetBadge = new List<Guid>();
				foreach (var personId in personIdList)
				{
					IPerson person;
					if (!personsList.TryGetValue(personId, out person))
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

					idListOfPersonShouldGetBadge.Add(personId);
				}

				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("{0} agents will get badge for adherence", idListOfPersonShouldGetBadge.Count);
				}

				var newAwardedBadge = AddBadge(personList, idListOfPersonShouldGetBadge, BadgeType.Adherence, setting.SilverToBronzeBadgeRate,
					setting.GoldToSilverBadgeRate, date);
				newAwardedBadges.AddRange(newAwardedBadge);
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

		public IEnumerable<IAgentBadgeTransaction> CalculateAHTBadges(IEnumerable<IPerson> allPersons, string timezoneCode, DateOnly date, IGamificationSetting setting, Guid businessUnitId)
		{
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat(
					"Calculate AHT badges for timezone: {0}, date: {1:yyyy-MM-dd HH:mm:ss},"
					+ "silver to bronze badge rate: {2}, gold to silver badge rate: {3}, AHT threshold: {4}.", timezoneCode, date.Date,
					 setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate, setting.AHTThreshold);
			}

			var personList = allPersons.ToList();
			var newAwardedBadges = new List<IAgentBadgeTransaction>();
			var agentsList = _badgeCalculationRepository.LoadAgentsUnderThresholdForAht(timezoneCode, date.Date, setting.AHTThreshold, businessUnitId);

			if (agentsList.Any())
			{
				var agents = agentsList.Keys;
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("{0} agents will get badge for AHT", agents.Count);
				}

				var newAwardedAhtBadges
					= AddBadge(personList, agents, BadgeType.AverageHandlingTime, setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate, date);
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

		public IEnumerable<IAgentBadgeTransaction> CalculateAnsweredCallsBadges(IEnumerable<IPerson> allPersons, string timezoneCode, DateOnly date,
			IGamificationSetting setting, Guid businessUnitId)
		{
			if (logger.IsDebugEnabled)
			{
				logger.DebugFormat(
					"Calculate answered calls badges for timezone: {0}, date: {1:yyyy-MM-dd HH:mm:ss},"
					+ "silver to bronze badge rate: {2}, gold to silver badge rate: {3}, answeredCallsThreshold: {4}", timezoneCode, date.Date,
					 setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate, setting.AnsweredCallsThreshold);
			}

			var personList = allPersons.ToList();
			var newAwardedBadges = new List<IAgentBadgeTransaction>();
			var agentsList = _badgeCalculationRepository.LoadAgentsOverThresholdForAnsweredCalls(timezoneCode, date.Date, setting.AnsweredCallsThreshold, businessUnitId);

			if (agentsList.Any())
			{
				var agents = agentsList.Keys;
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("{0} agents will get badge for answered calls", agents.Count);
				}

				var newAwardedAnsweredCallsBadges
					= AddBadge(personList, agents, BadgeType.AnsweredCalls, setting.SilverToBronzeBadgeRate, setting.GoldToSilverBadgeRate, date);
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