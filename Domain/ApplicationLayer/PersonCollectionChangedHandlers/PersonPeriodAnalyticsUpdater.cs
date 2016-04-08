using System.Collections.Generic;
using System.Linq;
using log4net;
using Teleopti.Ccc.Domain.Analytics;
using Teleopti.Ccc.Domain.Analytics.Transformer;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Repositories;

namespace Teleopti.Ccc.Domain.ApplicationLayer.PersonCollectionChangedHandlers
{
	[UseOnToggle(Toggles.ETL_SpeedUpPersonPeriodIntraday_37162_37439)]
	public class PersonPeriodAnalyticsUpdater :
		IHandleEvent<PersonCollectionChangedEvent>,
		IHandleEvent<PersonDeletedEvent>,
		IRunOnServiceBus
	{
		private static readonly ILog Logger = LogManager.GetLogger(typeof(PersonPeriodAnalyticsUpdater));
		private readonly AcdLoginPersonTransformer _analyticsAcdLoginPerson;
		private readonly IAnalyticsPersonPeriodRepository _analyticsPersonPeriodRepository;
		private readonly IAnalyticsSkillRepository _analyticsSkillRepository;
		private readonly IPersonRepository _personRepository;
		private readonly IEventPublisher _eventPublisher;

		public PersonPeriodAnalyticsUpdater(IPersonRepository personRepository,
			IAnalyticsPersonPeriodRepository analyticsPersonPeriodRepository,
			IAnalyticsSkillRepository analyticsSkillRepository, IEventPublisher eventPublisher)
		{
			_personRepository = personRepository;
			_analyticsPersonPeriodRepository = analyticsPersonPeriodRepository;
			_analyticsSkillRepository = analyticsSkillRepository;
			_eventPublisher = eventPublisher;

			_analyticsAcdLoginPerson = new AcdLoginPersonTransformer(_analyticsPersonPeriodRepository);
		}

		public void Handle(PersonCollectionChangedEvent @event)
		{
			var personPeriodFilter = new PersonPeriodFilter(
				_analyticsPersonPeriodRepository.MinDate().DateDate,
				_analyticsPersonPeriodRepository.MaxDate().DateDate);

			var persons = _personRepository.FindPeople(@event.PersonIdCollection.Distinct());

			var transformer = new PersonPeriodTransformer(_analyticsPersonPeriodRepository, _analyticsSkillRepository);

			foreach (var personCodeGuid in @event.PersonIdCollection.Distinct())
			{
				// Check if person does exists => if not it is deleted and handled by other handle-method
				if (!persons.Any(a => a.Id.Equals(personCodeGuid)))
				{
					continue;
				}

				var person = persons.First(a => a.Id.Equals(personCodeGuid));
				if (person == null)
				{
					continue;
				}

				var personPeriodsInAnalytics = _analyticsPersonPeriodRepository.GetPersonPeriods(personCodeGuid);

				foreach (var personPeriod in personPeriodFilter.GetFiltered(person.PersonPeriodCollection))
				{
					// Check if person period already exists in database
					var existingPeriod = personPeriodsInAnalytics.FirstOrDefault(a => a.PersonPeriodCode.Equals(personPeriod.Id.Value));
					List<AnalyticsSkill> analyticsSkills;

					if (existingPeriod != null)
					{
						Logger.DebugFormat("Update person period for {0}", person.Name);

						// Update
						var updatedAnalyticsPersonPeriod = transformer.Transform(person, personPeriod, out analyticsSkills);

						// Keep windows domain and username until external login information is availble from service bus
						updatedAnalyticsPersonPeriod.WindowsUsername = existingPeriod.WindowsUsername;
						updatedAnalyticsPersonPeriod.WindowsDomain = existingPeriod.WindowsDomain;

						_analyticsPersonPeriodRepository.UpdatePersonPeriod(updatedAnalyticsPersonPeriod);
					}
					else
					{
						Logger.DebugFormat("Insert new person period for {0}", person.Name);

						// Insert
						var newAnalyticsPersonPeriod = transformer.Transform(person, personPeriod, out analyticsSkills);
						_analyticsPersonPeriodRepository.AddPersonPeriod(newAnalyticsPersonPeriod);
					}

					var newOrUpdatedPersonPeriod = _analyticsPersonPeriodRepository.GetPersonPeriods(personCodeGuid).FirstOrDefault(a => a.PersonPeriodCode.Equals(personPeriod.Id.Value));
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
			_eventPublisher.Publish(new AnalyticsPersonCollectionChangedEvent
			{
				InitiatorId = @event.InitiatorId,
				LogOnBusinessUnitId = @event.LogOnBusinessUnitId,
				LogOnDatasource = @event.LogOnDatasource,
				SerializedPeople = @event.SerializedPeople,
				Timestamp = @event.Timestamp
			});
		}

		private void publishSkillChangeEvent(PersonCollectionChangedEvent @event, Interfaces.Domain.IPersonPeriod personPeriod, List<AnalyticsSkill> analyticsSkills, AnalyticsPersonPeriod updatedAnalyticsPersonPeriod)
		{
			var activeSkills = personPeriod.PersonSkillCollection.Where(a => a.Active)
					.Select(a => analyticsSkills.First(b => b.SkillCode.Equals(a.Skill.Id)).SkillId).ToList();
			var inactiveSkills = personPeriod.PersonSkillCollection.Where(a => !a.Active)
					.Select(a => analyticsSkills.First(b => b.SkillCode.Equals(a.Skill.Id)).SkillId).ToList();

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
		}

		public void Handle(PersonDeletedEvent @event)
		{
			Logger.DebugFormat("Removing all person periods with person code {0}", @event.PersonId);
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