using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Logon;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
	public class AnalyticsPersonPeriodUpdater : IHandleEvent<PersonCollectionChangedEvent>,
		IHandleEvent<PersonDeletedEvent>,
		IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsPersonPeriodUpdater));
		private readonly AcdLoginPersonTransformer _analyticsAcdLoginPerson;
		private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IEventPopulatingPublisher _eventPublisher;
		private readonly ICurrentAnalyticsUnitOfWork _currentAnalyticsUnitOfWork;
		private readonly IPersonPeriodFilter _personPeriodFilter;
		private readonly IPersonPeriodTransformer _personPeriodTransformer;
		private readonly IAnalyticsTimeZoneRepository _analyticsTimeZoneRepository;

		public AnalyticsPersonPeriodUpdater(IPersonRepository personRepository,
			IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository,
			IEventPopulatingPublisher eventPublisher,
			ICurrentAnalyticsUnitOfWork currentAnalyticsUnitOfWork,
			IPersonPeriodFilter personPeriodFilter,
			IPersonPeriodTransformer personPeriodTransformer, 
			IAnalyticsTimeZoneRepository analyticsTimeZoneRepository)
		{
			_personRepository = personRepository;
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
			_eventPublisher = eventPublisher;
			_currentAnalyticsUnitOfWork = currentAnalyticsUnitOfWork;
			_personPeriodFilter = personPeriodFilter;
			_personPeriodTransformer = personPeriodTransformer;
			_analyticsTimeZoneRepository = analyticsTimeZoneRepository;

			_analyticsAcdLoginPerson = new AcdLoginPersonTransformer(_analyticsPersonPeriodRepository);
		}

		[ImpersonateSystem]
		[AnalyticsUnitOfWork]
		[UnitOfWork]
		[Attempts(10)]
		public virtual void Handle(PersonCollectionChangedEvent @event)
		{
			var persons = _personRepository.FindPeople(@event.PersonIdCollection);

			var changedPeople = new List<Guid>();
			var peopleWithUnlinkedPersonPeriod = new HashSet<Guid>();
			foreach (var personCodeGuid in @event.PersonIdCollection)
			{
				// Check if person does exists => if not it is deleted and handled by other handle-method
				var person = persons.FirstOrDefault(a => a.Id == personCodeGuid);
				if (person == null)
				{
					logger.Debug($"Person '{personCodeGuid}' was not found in application.");
					continue;
				}
				var personPeriodsInAnalytics = _analyticsPersonPeriodRepository.GetPersonPeriods(personCodeGuid);
				var personPeriods = _personPeriodFilter.GetFiltered(person.PersonPeriodCollection).ToList();
				if (!personPeriods.Any())
				{
					// Make sure the timezone is added even though there are no periods #43986
					_analyticsTimeZoneRepository.Get(person.PermissionInformation.DefaultTimeZone().Id);
				}
				foreach (var personPeriod in personPeriods)
				{
					// Check if person period already exists in database
					var existingPeriod = personPeriodsInAnalytics.FirstOrDefault(a => a.PersonPeriodCode.Equals(personPeriod.Id.GetValueOrDefault()));
					List<AnalyticsSkill> analyticsSkills;
					changedPeople.Add(person.Id.GetValueOrDefault());
					if (existingPeriod != null)
					{
						logger.Debug($"Update person period for {person.Name}");

						// Update
						var updatedAnalyticsPersonPeriod = _personPeriodTransformer.Transform(person, personPeriod, out analyticsSkills);
						updatedAnalyticsPersonPeriod.PersonId = existingPeriod.PersonId;
						// Keep windows domain and username until external login information is availble from service bus
						updatedAnalyticsPersonPeriod.WindowsUsername = existingPeriod.WindowsUsername;
						updatedAnalyticsPersonPeriod.WindowsDomain = existingPeriod.WindowsDomain;

						if (existingPeriod.ValidFromDateIdLocal != updatedAnalyticsPersonPeriod.ValidFromDateIdLocal ||
							existingPeriod.ValidToDateIdLocal != updatedAnalyticsPersonPeriod.ValidToDateIdLocal)
						{
							peopleWithUnlinkedPersonPeriod.Add(personCodeGuid);
						}

						_analyticsPersonPeriodRepository.UpdatePersonPeriod(updatedAnalyticsPersonPeriod);
					}
					else
					{
						logger.Debug($"Insert new person period for {person.Name}");

						// Insert
						var newAnalyticsPersonPeriod = _personPeriodTransformer.Transform(person, personPeriod, out analyticsSkills);
						_analyticsPersonPeriodRepository.AddPersonPeriod(newAnalyticsPersonPeriod);
						peopleWithUnlinkedPersonPeriod.Add(personCodeGuid);
					}

					var newOrUpdatedPersonPeriod = _analyticsPersonPeriodRepository.GetPersonPeriods(personCodeGuid).
						FirstOrDefault(a => a.PersonPeriodCode.Equals(personPeriod.Id.GetValueOrDefault()));
					if (newOrUpdatedPersonPeriod == null)
					{
						logger.Warn($"PersonPeriod '{personPeriod.Id.GetValueOrDefault()}' could not be found in analytics after update or insert.");
						continue;
					}
					
					var newPeriodSkillsetId = newOrUpdatedPersonPeriod.SkillsetId == -1 ? null : newOrUpdatedPersonPeriod.SkillsetId;
					var existingPeriodSkillsetId = existingPeriod?.SkillsetId == -1 ? null : existingPeriod?.SkillsetId;
					if (newPeriodSkillsetId != existingPeriodSkillsetId)
					{
						publishSkillChangeEvent(personPeriod, analyticsSkills, newOrUpdatedPersonPeriod);
					}

					// Update/Add/Delete from Bridge Acd Login Person table
					var bridgeListForPersonPeriod = _analyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForPerson(newOrUpdatedPersonPeriod.PersonId);
					foreach (var externalLogOn in personPeriod.ExternalLogOnCollection)
					{
						if (bridgeListForPersonPeriod.All(a => a.AcdLoginId != externalLogOn.AcdLogOnMartId))
						{
							// Insert new acd login bridge
							_analyticsAcdLoginPerson.AddAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson
							{
								AcdLoginId = externalLogOn.AcdLogOnMartId,
								PersonId = newOrUpdatedPersonPeriod.PersonId,
								TeamId = newOrUpdatedPersonPeriod.TeamId,
								BusinessUnitId = newOrUpdatedPersonPeriod.BusinessUnitId,
								DatasourceId = newOrUpdatedPersonPeriod.DatasourceId,
								DatasourceUpdateDate = newOrUpdatedPersonPeriod.DatasourceUpdateDate
							});
						}
					}

					// Delete acd login bridge
					foreach (var bridgeAcdLoginPerson in bridgeListForPersonPeriod)
					{
						if (personPeriod.ExternalLogOnCollection.All(a => a.AcdLogOnMartId != bridgeAcdLoginPerson.AcdLoginId))
						{
							_analyticsAcdLoginPerson.DeleteAcdLoginPerson(bridgeAcdLoginPerson);
						}
					}
				}

				// Check deleted person periods
				foreach (var analyticsPersonPeriod in personPeriodsInAnalytics.Where(period => !period.ToBeDeleted))
				{
					if (!person.PersonPeriodCollection.Any(a => a.Id.Equals(analyticsPersonPeriod.PersonPeriodCode)))
					{
						_analyticsPersonPeriodRepository.DeletePersonPeriod(analyticsPersonPeriod);
						peopleWithUnlinkedPersonPeriod.Add(personCodeGuid);

						// Delete bridge acd login for that person period
						foreach (var bridgeAcdLoginPerson in _analyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForPerson(analyticsPersonPeriod.PersonId))
						{
							_analyticsPersonPeriodRepository.DeleteBridgeAcdLoginPerson(bridgeAcdLoginPerson.AcdLoginId,
																						bridgeAcdLoginPerson.PersonId);
						}
					}
				}
			}
			_currentAnalyticsUnitOfWork.Current().AfterSuccessfulTx(() =>
			{
				if (!changedPeople.Any()) return;
				var analyticsPersonCollectionChangedEvent = new AnalyticsPersonCollectionChangedEvent();
				analyticsPersonCollectionChangedEvent.SetPersonIdCollection(changedPeople);
				_eventPublisher.Publish(analyticsPersonCollectionChangedEvent);
			});

			_currentAnalyticsUnitOfWork.Current().AfterSuccessfulTx(() =>
			{
				if (!peopleWithUnlinkedPersonPeriod.Any()) return;
				var analyticsPersonPeriodRangeChangedEvent = new AnalyticsPersonPeriodRangeChangedEvent();
				analyticsPersonPeriodRangeChangedEvent.SetPersonIdCollection(peopleWithUnlinkedPersonPeriod);
				_eventPublisher.Publish(analyticsPersonPeriodRangeChangedEvent);
			});

		}

		private void publishSkillChangeEvent(IPersonPeriod personPeriod, IEnumerable<AnalyticsSkill> analyticsSkills, AnalyticsPersonPeriod updatedAnalyticsPersonPeriod)
		{
			var existsInAnalytics = personPeriod.PersonSkillCollection.Where(a => analyticsSkills.Any(b => b.SkillCode.Equals(a.Skill.Id))).ToList();

			var activeSkills = existsInAnalytics.Where(a => a.Active)
					.Select(a => analyticsSkills.First(b => b.SkillCode.Equals(a.Skill.Id)).SkillId)
					.ToList();
			var inactiveSkills = existsInAnalytics.Where(a => !a.Active)
					.Select(a => analyticsSkills.First(b => b.SkillCode.Equals(a.Skill.Id)).SkillId)
					.ToList();

			_currentAnalyticsUnitOfWork.Current().AfterSuccessfulTx(() =>
			{
				_eventPublisher.Publish(new AnalyticsPersonPeriodSkillsChangedEvent
				{
					AnalyticsPersonPeriodId = updatedAnalyticsPersonPeriod.PersonId,
					AnalyticsBusinessUnitId = updatedAnalyticsPersonPeriod.BusinessUnitId,
					AnalyticsActiveSkillsId = activeSkills,
					AnalyticsInactiveSkillsId = inactiveSkills
				});
			});
		}

		[AnalyticsUnitOfWork]
		[Attempts(10)]
		public virtual void Handle(PersonDeletedEvent @event)
		{
			logger.Debug($"Removing all person periods with person code {@event.PersonId}");
			var personPeriodsInAnalyticsToBeDeleted = _analyticsPersonPeriodRepository.GetPersonPeriods(@event.PersonId);

			foreach (var analyticsPersonPeriodToBeDeleted in personPeriodsInAnalyticsToBeDeleted)
			{
				_analyticsPersonPeriodRepository.DeletePersonPeriod(analyticsPersonPeriodToBeDeleted);
				foreach (var bridgeAcdLoginPerson in _analyticsPersonPeriodRepository.GetBridgeAcdLoginPersonsForPerson(analyticsPersonPeriodToBeDeleted.PersonId))
				{
					_analyticsAcdLoginPerson.DeleteAcdLoginPerson(new AnalyticsBridgeAcdLoginPerson
					{
						AcdLoginId = bridgeAcdLoginPerson.AcdLoginId,
						PersonId = bridgeAcdLoginPerson.PersonId
					});
				}
			}
		}
	}
}