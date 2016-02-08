using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.SystemSetting.GlobalSetting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.AgentBadge
{
	public class AgentBadgeCalculator : IAgentBadgeCalculator
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AgentBadgeCalculator));
		private readonly IStatisticRepository _statisticRepository;
		private readonly IAgentBadgeTransactionRepository _transactionRepository;
		private readonly IDefinedRaptorApplicationFunctionFactory _appFunctionFactory;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly IScenarioRepository _scenarioRepository;
		private readonly IPersonRepository _personRepository;
		private readonly INow _now;

		public AgentBadgeCalculator(IStatisticRepository statisticRepository,
			IAgentBadgeTransactionRepository transactionRepository,
			IDefinedRaptorApplicationFunctionFactory appFunctionFactory,
			IPersonRepository personRepository,
			IScheduleStorage scheduleStorage,
			IScenarioRepository scenarioRepository,
			INow now)
		{
			_statisticRepository = statisticRepository;
			_transactionRepository = transactionRepository;
			_appFunctionFactory = appFunctionFactory;
			_scheduleStorage = scheduleStorage;
			_scenarioRepository = scenarioRepository;
			_personRepository = personRepository;
			_now = now;
		}

		protected IList<IAgentBadgeTransaction> AddBadge(IEnumerable<IPerson> allPersons, IEnumerable<Guid> agentIdsThatShouldGetBadge,
			BadgeType badgeType, int silverToBronzeBadgeRate, int goldToSilverBadgeRate, DateOnly date)
		{
			var newAwardedBadges = new List<IAgentBadgeTransaction>();

			var agentIdListShouldGetBadge = agentIdsThatShouldGetBadge as IList<Guid> ?? agentIdsThatShouldGetBadge.Where(a => a!=null).ToList();
			if (!agentIdListShouldGetBadge.Any())
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

				var badge = _transactionRepository.Find(person, badgeType, date);

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
						InsertedOn = _now.UtcDateTime()
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
				_statisticRepository.LoadAgentsOverThresholdForAdherence(adherenceCalculationMethod, timezoneCode, date.Date, setting.AdherenceThreshold, businessUnitId);
			
			if (agentAdherenceList.Count > 0)
			{
				var personIdList = (from object[] data in agentAdherenceList select (Guid)data[0]).ToList();
				var personsList = _personRepository.FindPeople(personIdList);
				var schedules = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(personsList,
					new ScheduleDictionaryLoadOptions(true, false), new DateOnlyPeriod(date, date),
					_scenarioRepository.LoadDefaultScenario());

				var idListOfPersonShouldGetBadge = new List<Guid>();
				foreach (var personId in personIdList)
				{
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

					idListOfPersonShouldGetBadge.Add(personId);
				}

				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("{0} agents will get badge for adherence", idListOfPersonShouldGetBadge.Count());
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
			var agentsList = _statisticRepository.LoadAgentsUnderThresholdForAHT(timezoneCode, date.Date, setting.AHTThreshold, businessUnitId);

			var agents = new List<Guid>();
			if (agentsList.Count > 0)
			{
				agents.AddRange(from object[] data in agentsList select (Guid)data[0]);
			}

			if (agents.Any())
			{
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("{0} agents will get badge for AHT", agents.Count());
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
			var agentsList = _statisticRepository.LoadAgentsOverThresholdForAnsweredCalls(timezoneCode, date.Date, setting.AnsweredCallsThreshold, businessUnitId);

			var agents = new List<Guid>();
			if (agentsList.Count > 0)
			{
				agents.AddRange(from object[] data in agentsList select (Guid)data[0]);
			}

			if (agents.Any())
			{
				if (logger.IsDebugEnabled)
				{
					logger.DebugFormat("{0} agents will get badge for answered calls", agents.Count());
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
