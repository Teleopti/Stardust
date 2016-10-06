using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
	[EnabledBy(Toggles.ETL_SpeedUpPersonPeriodIntraday_37162_37439)]
	public class AnalyticsPersonPeriodUpdater : IHandleEvent<PersonCollectionChangedEvent>,
		IHandleEvent<PersonDeletedEvent>,
		IRunOnHangfire
	{
		private static readonly ILog logger = LogManager.GetLogger(typeof(AnalyticsPersonPeriodUpdater));
		private readonly AcdLoginPersonTransformer _analyticsAcdLoginPerson;
		private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IEventPublisher _eventPublisher;
		private readonly ICurrentAnalyticsUnitOfWork _currentAnalyticsUnitOfWork;
		private readonly IPersonPeriodFilter _personPeriodFilter;
		private readonly IPersonPeriodTransformer _personPeriodTransformer;

		public AnalyticsPersonPeriodUpdater(IPersonRepository personRepository,
			IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository,
			IEventPublisher eventPublisher,
			ICurrentAnalyticsUnitOfWork currentAnalyticsUnitOfWork,
			IPersonPeriodFilter personPeriodFilter,
			IPersonPeriodTransformer personPeriodTransformer)
		{
			_personRepository = personRepository;
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
			_eventPublisher = eventPublisher;
			_currentAnalyticsUnitOfWork = currentAnalyticsUnitOfWork;
			_personPeriodFilter = personPeriodFilter;
			_personPeriodTransformer = personPeriodTransformer;

			_analyticsAcdLoginPerson = new AcdLoginPersonTransformer(_analyticsPersonPeriodRepository);
		}

		[AnalyticsUnitOfWork]
		[UnitOfWork]
		[Attempts(10)]
		public virtual void Handle(PersonCollectionChangedEvent @event)
		{
			var persons = _personRepository.FindPeople(@event.PersonIdCollection.Distinct());

			var changedPeople = new List<Guid>();
			foreach (var personCodeGuid in @event.PersonIdCollection.Distinct())
			{
				// Check if person does exists => if not it is deleted and handled by other handle-method
				if (!persons.Any(a => a.Id.Equals(personCodeGuid)))
				{
					logger.Debug($"Person '{personCodeGuid}' was not found in application.");
					continue;
				}

				var person = persons.First(a => a.Id.Equals(personCodeGuid));

				var personPeriodsInAnalytics = _analyticsPersonPeriodRepository.GetPersonPeriods(personCodeGuid);

				foreach (var personPeriod in _personPeriodFilter.GetFiltered(person.PersonPeriodCollection))
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

						// Keep windows domain and username until external login information is availble from service bus
						updatedAnalyticsPersonPeriod.WindowsUsername = existingPeriod.WindowsUsername;
						updatedAnalyticsPersonPeriod.WindowsDomain = existingPeriod.WindowsDomain;

						_analyticsPersonPeriodRepository.UpdatePersonPeriod(updatedAnalyticsPersonPeriod);
					}
					else
					{
						logger.Debug($"Insert new person period for {person.Name}");

						// Insert
						var newAnalyticsPersonPeriod = _personPeriodTransformer.Transform(person, personPeriod, out analyticsSkills);
						_analyticsPersonPeriodRepository.AddPersonPeriod(newAnalyticsPersonPeriod);
					}

					var newOrUpdatedPersonPeriod = _analyticsPersonPeriodRepository.GetPersonPeriods(personCodeGuid).
						FirstOrDefault(a => a.PersonPeriodCode.Equals(personPeriod.Id.GetValueOrDefault()));
					if (newOrUpdatedPersonPeriod == null)
					{
						logger.Warn($"PersonPeriod '{personPeriod.Id.GetValueOrDefault()}' could not be found in analytics after update or insert.");
						continue;
					}

					if ((existingPeriod == null && newOrUpdatedPersonPeriod.SkillsetId != null) ||
						(existingPeriod != null && newOrUpdatedPersonPeriod.SkillsetId != existingPeriod.SkillsetId))
					{
						publishSkillChangeEvent(@event, personPeriod, analyticsSkills, newOrUpdatedPersonPeriod);
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
				foreach (var analyticsPersonPeriod in personPeriodsInAnalytics)
				{
					if (!person.PersonPeriodCollection.Any(a => a.Id.Equals(analyticsPersonPeriod.PersonPeriodCode)))
					{
						_analyticsPersonPeriodRepository.DeletePersonPeriod(analyticsPersonPeriod);

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
				changedPeople = changedPeople.Distinct().ToList();
				if (!changedPeople.Any()) return;
				var analyticsPersonCollectionChangedEvent = new AnalyticsPersonCollectionChangedEvent
				{
					InitiatorId = @event.InitiatorId,
					LogOnBusinessUnitId = @event.LogOnBusinessUnitId,
					LogOnDatasource = @event.LogOnDatasource,
					Timestamp = @event.Timestamp
				};
				analyticsPersonCollectionChangedEvent.SetPersonIdCollection(changedPeople);
				_eventPublisher.Publish(analyticsPersonCollectionChangedEvent);
			});

		}

		private void publishSkillChangeEvent(PersonCollectionChangedEvent @event, IPersonPeriod personPeriod, IEnumerable<AnalyticsSkill> analyticsSkills, AnalyticsPersonPeriod updatedAnalyticsPersonPeriod)
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
					AnalyticsInactiveSkillsId = inactiveSkills,
					InitiatorId = @event.InitiatorId,
					LogOnBusinessUnitId = @event.LogOnBusinessUnitId,
					LogOnDatasource = @event.LogOnDatasource,
					Timestamp = @event.Timestamp
				});
			});
		}

		[AnalyticsUnitOfWork]
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